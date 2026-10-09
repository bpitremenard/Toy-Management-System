using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Auth;
using TMS_SharedLibrary.Helpers;
using TMS_SharedLibrary.Models;

namespace TMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepo _studentRepo;
        private readonly IToyRepo _toyRepo;
        private readonly IMapper _mapper;
        private readonly IAuditLogger _auditLogger;
        private readonly UserHelper _userHelper;

        public StudentsController(
            IStudentRepo studentRepo,
            IToyRepo toyRepo,
            IMapper mapper,
            IAuditLogger auditLogger,
            UserHelper userHelper)
        {
            _studentRepo = studentRepo;
            _toyRepo = toyRepo;
            _mapper = mapper;
            _auditLogger = auditLogger;
            _userHelper = userHelper;
        }

        // GET: /Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> Get()
        {
            var students = await _studentRepo.GetAllAsync();
            var result = _mapper.Map<IEnumerable<StudentDTO>>(students);
            return Ok(result);
        }

        // GET: /Students/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDTO>> Get(int id)
        {
            var student = await _studentRepo.FindByIdAsync(id);
            if (student == null) return NotFound();
            return Ok(_mapper.Map<StudentDTO>(student));
        }

        // GET: /Students/ByOID/{id}
        [HttpGet("ByOID/{id}")]
        public async Task<ActionResult<int>> GetByOID(string id)
        {
            var student = await _studentRepo.FindByOidAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        // PUT: /Students/BorrowToy/{id}?toyId={toyId}
        [HttpPut("BorrowToy/{id}")]
        public async Task<IActionResult> BorrowToy(int id, int toyId)
        {
            if (id <= 0)
            {
                return BadRequest("Student ID must be greater than 0.");
            }

            var student = await _studentRepo.FindByIdAsync(id);
            var toy = await _toyRepo.FindByIdAsync(toyId);

            if (student == null) return NotFound();
            if (toy == null) return BadRequest("Toy not found");
            if (toy.IsAvailable == false) return BadRequest("Toy is not available");
            if (_studentRepo.HasActiveLoanForToy(toyId)) return BadRequest("Toy already has an active loan.");
            if (await _studentRepo.CheckToyIsBorrowedByStudentAsync(id, toyId)) return BadRequest("Student already has this toy checked out.");

            await _studentRepo.BookToyAsync(id, toyId);

            return NoContent();
        }

        // PUT: /Students/ReturnToy/{id}?toyId={toyId}
        [HttpPut("ReturnToy/{id}")]
        public async Task<IActionResult> ReturnToy(int id, int toyId)
        {
            if (id <= 0)
            {
                return BadRequest("Student ID must be greater than 0.");
            }

            var student = await _studentRepo.FindByIdAsync(id);
            var toy = await _toyRepo.FindByIdAsync(toyId);

            if (student == null) return NotFound();
            if (toy == null) return BadRequest("Toy not found");
            if (!await _studentRepo.CheckToyIsBorrowedByStudentAsync(id, toyId))
                return BadRequest("Could not find a current toy loan for this student and toy");

            await _studentRepo.ReturnToyAsync(id, toyId);

            return NoContent();
        }

        // GET: /Students/{id}/BorrowHistory
        [HttpGet("{id}/BorrowHistory")]
        public async Task<IActionResult> GetBorrowHistory(int id)
        {
            var student = await _studentRepo.FindByIdAsync(id);
            if (student == null) return NotFound("Student not found");

            var borrowHistory = await _studentRepo.GetBorrowHistoryAsync(id);
            var borrowHistoryDTO = _mapper.Map<List<ToyLoanDTO>>(borrowHistory);
            return Ok(borrowHistoryDTO);
        }

        // GET: /Students/{id}/CurrentLoans
        [HttpGet("{id}/CurrentLoans")]
        public async Task<IActionResult> GetCurrentLoans(int id)
        {
            var student = await _studentRepo.FindByIdAsync(id);
            if (student == null) return NotFound("Student not found");

            var borrowHistory = (await _studentRepo.GetBorrowHistoryAsync(id))
                .Where(tl => tl.ReturnDate == null)
                .ToList();
            var borrowHistoryDTO = _mapper.Map<List<ToyLoanDTO>>(borrowHistory);
            return Ok(borrowHistoryDTO);
        }
    }
}
