using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Models;

namespace TMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly IMapper _mapper;

        public UsersController(IUserRepo userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> Get()
        {
            var users = await _userRepo.GetAllAsync();
            var result = _mapper.Map<IEnumerable<UserDTO>>(users);
            return Ok(result);
        }

        // GET api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> Get(int id)
        {
            var user = await _userRepo.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<UserDTO>(user));
        }

        // GET api/Users/oid/abc123
        [HttpGet("oid/{oid}")]
        public async Task<ActionResult<UserDTO>> GetByOid(string oid)
        {
            var user = await _userRepo.FindByOidAsync(oid);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<UserDTO>(user));
        }

        // POST api/Users
        [HttpPost]
        public async Task<ActionResult<UserDTO>> Post([FromBody] UserDTO userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User data is required");
            }

            var user = _mapper.Map<User>(userDto);
            var createdUser = await _userRepo.CreateAsync(user);
            
            return CreatedAtAction(nameof(Get), new { id = createdUser.UserId }, _mapper.Map<UserDTO>(createdUser));
        }
    }
}
