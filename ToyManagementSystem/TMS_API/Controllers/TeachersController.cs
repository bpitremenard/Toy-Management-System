using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS_API.DAL;
using TMS_API.Helpers;
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
    [Authorize(Roles = "Admin,Teacher")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherRepo _teacherRepo;
        private readonly IMapper _mapper;
        private readonly IAuditLogger _auditLogger;
        private readonly UserHelper _userHelper;

        public TeachersController(ITeacherRepo teacherRepo, IMapper mapper, IAuditLogger auditLogger, UserHelper userHelper)
        {
            _teacherRepo = teacherRepo;
            _mapper = mapper;
            _auditLogger = auditLogger;
            _userHelper = userHelper;
        }

        // GET: api/<TeachersController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherDTO>>> Get()
        {
            List<Teacher> teachers = await _teacherRepo.GetAllAsync();
            IEnumerable<TeacherDTO> result = _mapper.Map<IEnumerable<TeacherDTO>>(teachers);
            return Ok(result);
        }

        // GET api/<TeachersController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TeacherDTO>> Get(int id)
        {
            var teacher = await _teacherRepo.FindByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<TeacherDTO>(teacher));
        }

        // POST api/<TeachersController>
        [HttpPost]
        public async Task<ActionResult<TeacherDTO>> Post([FromBody] TeacherDTO teacherDto)
        {
            if (teacherDto == null)
            {
                return BadRequest("Teacher data is required");
            }

            var teacher = _mapper.Map<Teacher>(teacherDto);
            var createdTeacher = await _teacherRepo.CreateAsync(teacher);

            var oid = User.GetOid();

            var performedByName =
                await _userHelper.GetUserNameFromOidAsync(oid);

            _auditLogger.Log(new AuditLogEntry
            {
                Action = AuditAction.Create,
                Entity = AuditEntity.From<Teacher>(),
                EntityId = createdTeacher.TeacherId.ToString(),
                PerformedByOid = oid,
                Role = User.GetRole(),
                Details = $"{performedByName} Created {createdTeacher.TeacherId}"
            });
            return CreatedAtAction(nameof(Get), new { id = createdTeacher.TeacherId }, _mapper.Map<TeacherDTO>(createdTeacher));
        }
    }
}
