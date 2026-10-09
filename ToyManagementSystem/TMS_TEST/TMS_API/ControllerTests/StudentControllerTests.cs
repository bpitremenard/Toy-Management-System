using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS_API.Controllers;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Helpers;
using TMS_SharedLibrary.Models;

namespace TMS_TEST.TMS_API.ControllerTests
{
    public class StudentControllerTests
    {
        private static StudentsController CreateController(
            Mock<IStudentRepo> studentRepo,
            Mock<IToyRepo> toyRepo,
            Mock<IMapper>? mapper = null,
            Mock<IAuditLogger>? auditLogger = null)
        {
            var resolvedMapper = mapper ?? new Mock<IMapper>();
            var resolvedAuditLogger = auditLogger ?? new Mock<IAuditLogger>();
            var userHelper = new UserHelper(
                new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                resolvedAuditLogger.Object);

            return new StudentsController(
                studentRepo.Object,
                toyRepo.Object,
                resolvedMapper.Object,
                resolvedAuditLogger.Object,
                userHelper);
        }

        [Fact]
        public async Task Get_ReturnsMappedStudents()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockToyRepo = new Mock<IToyRepo>();
            var userHelper = new UserHelper(
                new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                new Mock<IAuditLogger>().Object
                );
            var auditLogger = new Mock<IAuditLogger>();

            var students = new List<Student>
            {
                new Student { StudentId = 1, Placement = "A", Year = 1 },
                new Student { StudentId = 2, Placement = "B", Year = 2 }
            };

            var studentDtos = new List<StudentDTO>
            {
                new StudentDTO { StudentId = 1, Placement = "A", Year = 1 },
                new StudentDTO { StudentId = 2, Placement = "B", Year = 2 }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(students);
            mockMapper.Setup(m => m.Map<IEnumerable<StudentDTO>>(students))
                      .Returns(studentDtos);

            var controller = new StudentsController(mockRepo.Object, mockToyRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);

            var result = await controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStudents = Assert.IsAssignableFrom<IEnumerable<StudentDTO>>(okResult.Value);

            Assert.Collection(returnedStudents,
                s => Assert.Equal(1, s.StudentId),
                s => Assert.Equal(2, s.StudentId));
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_WhenNotExists()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockToyRepo = new Mock<IToyRepo>();
            var userHelper = new UserHelper(
                new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                new Mock<IAuditLogger>().Object
                );
            var auditLogger = new Mock<IAuditLogger>();

            mockRepo.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((Student)null);

            var controller = new StudentsController(mockRepo.Object, mockToyRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);

            var result = await controller.Get(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ById_ReturnsMappedStudent()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockToyRepo = new Mock<IToyRepo>();
            var userHelper = new UserHelper(
                new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                new Mock<IAuditLogger>().Object
                );
            var auditLogger = new Mock<IAuditLogger>();

            var student = new Student
            {
                StudentId = 5,
                Placement = "C",
                Year = 3
            };

            var dto = new StudentDTO
            {
                StudentId = 5,
                Placement = "C",
                Year = 3
            };

            mockRepo.Setup(r => r.FindByIdAsync(5)).ReturnsAsync(student);
            mockMapper.Setup(m => m.Map<StudentDTO>(student)).Returns(dto);

            var controller = new StudentsController(mockRepo.Object, mockToyRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);

            var result = await controller.Get(5);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStudent = Assert.IsType<StudentDTO>(okResult.Value);

            Assert.Equal(5, returnedStudent.StudentId);
            Assert.Equal("C", returnedStudent.Placement);
            Assert.Equal(3, returnedStudent.Year);
        }

        [Fact]
        public async Task BorrowToy_ReturnsBadRequest_WhenStudentIdIsInvalid()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockToyRepo = new Mock<IToyRepo>();
            var controller = CreateController(mockRepo, mockToyRepo);

            var result = await controller.BorrowToy(0, 3);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student ID must be greater than 0.", badRequest.Value);
        }

        [Fact]
        public async Task BorrowToy_ReturnsBadRequest_WhenToyAlreadyHasActiveLoan()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockToyRepo = new Mock<IToyRepo>();

            mockRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new Student { StudentId = 1 });
            mockToyRepo.Setup(r => r.FindByIdAsync(4)).ReturnsAsync(new Toy { ToyId = 4, IsAvailable = true, Name = "Blocks", LocationCode = "A01" });
            mockRepo.Setup(r => r.HasActiveLoanForToy(4)).Returns(true);

            var controller = CreateController(mockRepo, mockToyRepo);

            var result = await controller.BorrowToy(1, 4);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Toy already has an active loan.", badRequest.Value);
            mockRepo.Verify(r => r.BookToyAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task BorrowToy_ReturnsBadRequest_WhenStudentAlreadyHasToyCheckedOut()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockToyRepo = new Mock<IToyRepo>();

            mockRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new Student { StudentId = 1 });
            mockToyRepo.Setup(r => r.FindByIdAsync(4)).ReturnsAsync(new Toy { ToyId = 4, IsAvailable = true, Name = "Blocks", LocationCode = "A01" });
            mockRepo.Setup(r => r.HasActiveLoanForToy(4)).Returns(false);
            mockRepo.Setup(r => r.CheckToyIsBorrowedByStudentAsync(1, 4)).ReturnsAsync(true);

            var controller = CreateController(mockRepo, mockToyRepo);

            var result = await controller.BorrowToy(1, 4);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student already has this toy checked out.", badRequest.Value);
            mockRepo.Verify(r => r.BookToyAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task BorrowToy_BooksToy_WhenRequestIsValid()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockToyRepo = new Mock<IToyRepo>();

            mockRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new Student { StudentId = 1 });
            mockToyRepo.Setup(r => r.FindByIdAsync(4)).ReturnsAsync(new Toy { ToyId = 4, IsAvailable = true, Name = "Blocks", LocationCode = "A01" });
            mockRepo.Setup(r => r.HasActiveLoanForToy(4)).Returns(false);
            mockRepo.Setup(r => r.CheckToyIsBorrowedByStudentAsync(1, 4)).ReturnsAsync(false);
            mockRepo.Setup(r => r.BookToyAsync(1, 4)).Returns(Task.CompletedTask);

            var controller = CreateController(mockRepo, mockToyRepo);

            var result = await controller.BorrowToy(1, 4);

            Assert.IsType<NoContentResult>(result);
            mockRepo.Verify(r => r.BookToyAsync(1, 4), Times.Once);
        }

        [Fact]
        public async Task ReturnToy_ReturnsBadRequest_WhenStudentDoesNotHaveCurrentLoan()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockToyRepo = new Mock<IToyRepo>();

            mockRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new Student { StudentId = 1 });
            mockToyRepo.Setup(r => r.FindByIdAsync(4)).ReturnsAsync(new Toy { ToyId = 4, Name = "Blocks", LocationCode = "A01" });
            mockRepo.Setup(r => r.CheckToyIsBorrowedByStudentAsync(1, 4)).ReturnsAsync(false);

            var controller = CreateController(mockRepo, mockToyRepo);

            var result = await controller.ReturnToy(1, 4);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Could not find a current toy loan for this student and toy", badRequest.Value);
            mockRepo.Verify(r => r.ReturnToyAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ReturnToy_Completes_WhenCurrentLoanExists()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockToyRepo = new Mock<IToyRepo>();

            mockRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new Student { StudentId = 1 });
            mockToyRepo.Setup(r => r.FindByIdAsync(4)).ReturnsAsync(new Toy { ToyId = 4, Name = "Blocks", LocationCode = "A01" });
            mockRepo.Setup(r => r.CheckToyIsBorrowedByStudentAsync(1, 4)).ReturnsAsync(true);
            mockRepo.Setup(r => r.ReturnToyAsync(1, 4)).Returns(Task.CompletedTask);

            var controller = CreateController(mockRepo, mockToyRepo);

            var result = await controller.ReturnToy(1, 4);

            Assert.IsType<NoContentResult>(result);
            mockRepo.Verify(r => r.ReturnToyAsync(1, 4), Times.Once);
        }
    }
}
