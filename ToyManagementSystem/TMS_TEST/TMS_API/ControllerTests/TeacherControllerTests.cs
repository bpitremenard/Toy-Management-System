using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TMS_API.Controllers;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Helpers;
using TMS_SharedLibrary.Models;

namespace TMS_TEST.TMS_API.ControllerTests
{
    public class TeacherControllerTests
    {
        [Fact]
        public async Task Get_ReturnsMappedTeachers()
        {
            var mockRepo = new Mock<ITeacherRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(
                new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                new Mock<IAuditLogger>().Object
                );

            var teachers = new List<Teacher>
            {
                new Teacher { TeacherId = 1, UserId = 5 },
                new Teacher { TeacherId = 2, UserId = 7 }
            };

            var teacherDtos = new List<TeacherDTO>
            {
                new TeacherDTO { TeacherId = 1, OID = "OID1" },
                new TeacherDTO { TeacherId = 2, OID = "OID2" }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(teachers);
            mockMapper.Setup(m => m.Map<IEnumerable<TeacherDTO>>(teachers))
                      .Returns(teacherDtos);

            var controller = new TeachersController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);

            var result = await controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTeachers = Assert.IsAssignableFrom<IEnumerable<TeacherDTO>>(okResult.Value);

            Assert.Collection(returnedTeachers,
                t => Assert.Equal(1, t.TeacherId),
                t => Assert.Equal(2, t.TeacherId));
        }
    }
}
