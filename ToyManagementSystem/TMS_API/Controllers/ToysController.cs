using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Audit.Enums;
using TMS_SharedLibrary.Auth;
using TMS_SharedLibrary.Helpers;
using TMS_SharedLibrary.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Teacher, Admin")]
    public class ToysController : ControllerBase
    {
        private readonly IToyRepo _repo;
        private readonly IMapper _mapper;
        private readonly IAuditLogger _auditLogger;
        private readonly UserHelper _userHelper;

        public ToysController(IToyRepo repo, IMapper mapper, IAuditLogger auditLogger, UserHelper userHelper)
        {
            _repo = repo;
            _mapper = mapper;
            _auditLogger = auditLogger;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToyDTO>>> Get()
        {
            List<Toy> toys = await _repo.GetAllAsync();
            IEnumerable<ToyDTO> result = _mapper.Map<IEnumerable<ToyDTO>>(toys);
            return Ok(result);
        }
        
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<ToyDTO>>> GetFiltered(
            [FromQuery] string[]? materials,
            [FromQuery] string[]? categories)
        {
            List<Toy> toys;
            
            if ((materials == null || !materials.Any()) && 
                (categories == null || !categories.Any()))
            {
                toys = await _repo.GetAllAsync();
            }
            else
            {
                toys = await _repo.GetFilteredAsync(materials, categories);
            }
            
            var result = _mapper.Map<IEnumerable<ToyDTO>>(toys);
            return Ok(result);
        }

        // GET api/<ToysController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ToyDTO>> Get(int id)
        {
            var toy = await _repo.FindByIdAsync(id);
            if (toy == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<ToyDTO>(toy));
        }

        // POST api/<ToysController>
        [HttpPost]
        public async Task<IActionResult> Create(ToyDTO toyDTO)
        {
            NormalizeToyDto(toyDTO);

            var toy = _mapper.Map<Toy>(toyDTO);
            toy.IsActive = true;
            toy.IsAvailable = true;
            await _repo.AddAsync(toy);

            var oid = User.GetOid();

            var performedByName =
                await _userHelper.GetUserNameFromOidAsync(oid);

            _auditLogger.Log(new AuditLogEntry
            {
                Action = AuditAction.Create,
                Entity = AuditEntity.From<Toy>(),
                EntityId = toy.ToyId.ToString(),
                PerformedByOid = oid,
                Role = User.GetRole(),
                Details = $"{performedByName} Created {toy.Name}"
            });

            return CreatedAtAction(nameof(Create), new { id = toy.ToyId }, toy);
        }

        // PUT api/<ToysController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ToyDTO toyDTO)
        {
            if (id != toyDTO.ToyId)
            {
                return BadRequest();
            }

            NormalizeToyDto(toyDTO);

            var existingToy = await _repo.FindByIdAsync(id);
            if (existingToy == null)
            {
                return NotFound();
            }

            var toyHasActiveLoan = existingToy.ToyLoans.Any(tl => tl.ReturnDate == null);
            if (toyHasActiveLoan && toyDTO.IsAvailable)
            {
                return BadRequest("A toy with an active loan cannot be marked as available.");
            }

            var toy = _mapper.Map<Toy>(toyDTO);
            toy.IsActive = existingToy.IsActive;
            if (toyHasActiveLoan)
            {
                toy.IsAvailable = false;
            }
            await _repo.UpdateAsync(id, toy);

            var oid = User.GetOid();

            var performedByName =
                await _userHelper.GetUserNameFromOidAsync(oid);

            _auditLogger.Log(new AuditLogEntry
            {
                Action = AuditAction.Update,
                Entity = AuditEntity.From<Toy>(),
                EntityId = toy.ToyId.ToString(),
                PerformedByOid = oid,
                Role = User.GetRole(),
                Details = $"{performedByName} Updated {toy.Name}"
            });

            return NoContent();
        }

        // DELETE api/<ToysController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var toy = await _repo.FindByIdAsync(id);
            if (toy == null)
            {
                return NotFound();
            }

            if (toy.IsAvailable == false)
                return BadRequest("Cannot delete this toy because it is currently borrowed.");

            await _repo.DeleteAsync(id);

            var oid = User.GetOid();

            var performedByName =
                await _userHelper.GetUserNameFromOidAsync(oid);

            _auditLogger.Log(new AuditLogEntry
            {
                Action = AuditAction.Delete,
                Entity = AuditEntity.From<Toy>(),
                EntityId = toy.ToyId.ToString(),
                PerformedByOid = oid,
                Role = User.GetRole(),
                Details = $"{performedByName} Deleted {toy.Name}"
            });
            return NoContent();
        }

        private static void NormalizeToyDto(ToyDTO toyDto)
        {
            toyDto.Name = toyDto.Name?.Trim() ?? string.Empty;
            toyDto.Description = string.IsNullOrWhiteSpace(toyDto.Description) ? null : toyDto.Description.Trim();
            toyDto.AdditionalInformation = string.IsNullOrWhiteSpace(toyDto.AdditionalInformation) ? null : toyDto.AdditionalInformation.Trim();
            toyDto.Category = string.IsNullOrWhiteSpace(toyDto.Category) ? null : toyDto.Category.Trim();
            toyDto.Material = string.IsNullOrWhiteSpace(toyDto.Material) ? null : toyDto.Material.Trim();
            toyDto.LocationCode = toyDto.LocationCode?.Trim().ToUpperInvariant() ?? string.Empty;
            toyDto.ImagePath = string.IsNullOrWhiteSpace(toyDto.ImagePath) ? null : toyDto.ImagePath.Trim();
        }
    }
}
