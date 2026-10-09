using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TMS_API.Controllers;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Helpers;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMS_API.ControllerTests
{
    public class ToysControllerTests
    {
        private static ClaimsPrincipal GetTestUser(string role = "Admin")
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, role)
            }, "TestAuth"));
        }

        private static void AttachUserToController(ControllerBase controller, ClaimsPrincipal user)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
        }

        [Fact]
        public async Task Get_ReturnsMappedToys()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var toys = new List<Toy>
            {
                new Toy { ToyId = 1, Name = "Ball", IsActive = true },
                new Toy { ToyId = 2, Name = "Truck", IsActive = true }
            };

            var toyDtos = new List<ToyDTO>
            {
                new ToyDTO { ToyId = 1, Name = "Ball" },
                new ToyDTO { ToyId = 2, Name = "Truck" }
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(toys);
            mockMapper.Setup(m => m.Map<IEnumerable<ToyDTO>>(toys)).Returns(toyDtos);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth"))
                }
            };

            var result = await controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<ToyDTO>>(okResult.Value);

            Assert.Collection(returned,
                t => Assert.Equal(1, t.ToyId),
                t => Assert.Equal(2, t.ToyId));
        }

        [Fact]
        public async Task Get_ById_ReturnsNotFound_WhenToyDoesNotExist()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            mockRepo.Setup(r => r.FindByIdAsync(999)).ReturnsAsync((Toy)null);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);

            // Attach a user so GetRole() doesn't throw
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth"))
                }
            };

            var result = await controller.Get(999);

            // unwrap the ActionResult<T>
            var actionResult = result.Result;

            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task Get_ById_ReturnsMappedToy()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var toy = new Toy { ToyId = 5, Name = "Doll", IsActive = true };
            var dto = new ToyDTO { ToyId = 5, Name = "Doll" };

            mockRepo.Setup(r => r.FindByIdAsync(5)).ReturnsAsync(toy);
            mockMapper.Setup(m => m.Map<ToyDTO>(toy)).Returns(dto);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);

            // Attach user
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth"))
                }
            };

            var result = await controller.Get(5);

            // Since controller returns ActionResult<ToyDTO>, use Result to access
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<ToyDTO>(okResult.Value);

            Assert.Equal(5, returned.ToyId);
            Assert.Equal("Doll", returned.Name);
        }

        [Fact]
        public async Task Create_AddsToy_AndReturnsCreated()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var dto = new ToyDTO { ToyId = 10, Name = "Puzzle" };
            var toy = new Toy { ToyId = 10, Name = "Puzzle", IsActive = false };

            mockMapper.Setup(m => m.Map<Toy>(dto)).Returns(toy);
            mockRepo.Setup(r => r.AddAsync(toy)).Returns(Task.CompletedTask);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var result = await controller.Create(dto);

            mockRepo.Verify(r => r.AddAsync(toy), Times.Once);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("Create", created.ActionName);
            Assert.Equal(toy.ToyId, created.RouteValues["id"]);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var dto = new ToyDTO { ToyId = 2, Name = "Test" };

            var result = await controller.Update(5, dto);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenToyMissing()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            mockRepo.Setup(r => r.FindByIdAsync(10)).ReturnsAsync((Toy)null);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var dto = new ToyDTO { ToyId = 10, Name = "Blocks" };

            var result = await controller.Update(10, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_UpdatesToy_AndReturnsNoContent()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var existing = new Toy { ToyId = 3, Name = "Car", IsActive = true, LocationCode = "A01", ToyLoans = new List<ToyLoan>() };
            var updated = new Toy { ToyId = 3, Name = "Updated Car", IsActive = true };
            var dto = new ToyDTO { ToyId = 3, Name = "Updated Car" };

            mockRepo.Setup(r => r.FindByIdAsync(3)).ReturnsAsync(existing);
            mockMapper.Setup(m => m.Map<Toy>(dto)).Returns(updated);
            mockRepo.Setup(r => r.UpdateAsync(3, updated)).Returns(Task.CompletedTask);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var result = await controller.Update(3, dto);

            mockRepo.Verify(r => r.UpdateAsync(3, updated), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenBorrowedToyIsMarkedAvailable()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var existing = new Toy
            {
                ToyId = 3,
                Name = "Car",
                IsActive = true,
                IsAvailable = false,
                ToyLoans = new List<ToyLoan>
                {
                    new() { LoanId = 1, ToyId = 3, StudentId = 2, ReturnDate = null }
                }
            };

            mockRepo.Setup(r => r.FindByIdAsync(3)).ReturnsAsync(existing);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var dto = new ToyDTO { ToyId = 3, Name = "Car", LocationCode = "A01", IsAvailable = true };

            var result = await controller.Update(3, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("A toy with an active loan cannot be marked as available.", badRequest.Value);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Toy>()), Times.Never);
        }

        [Fact]
        public async Task Create_TrimmedValues_ArePassedToRepository()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            Toy? savedToy = null;
            mockMapper
                .Setup(m => m.Map<Toy>(It.IsAny<ToyDTO>()))
                .Returns<ToyDTO>(dto => new Toy
                {
                    ToyId = dto.ToyId,
                    Name = dto.Name,
                    Description = dto.Description,
                    AdditionalInformation = dto.AdditionalInformation,
                    Category = dto.Category,
                    Material = dto.Material,
                    LocationCode = dto.LocationCode,
                    ImagePath = dto.ImagePath
                });

            mockRepo.Setup(r => r.AddAsync(It.IsAny<Toy>()))
                .Callback<Toy>(toy => savedToy = toy)
                .Returns(Task.CompletedTask);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var dto = new ToyDTO
            {
                ToyId = 10,
                Name = "  Puzzle  ",
                Description = "  Fun puzzle.  ",
                AdditionalInformation = "  12 pieces.  ",
                Category = "  Cognitive  ",
                Material = "  Wood  ",
                LocationCode = "  b09  ",
                ImagePath = "  toy.jpg  "
            };

            var result = await controller.Create(dto);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(savedToy);
            Assert.Equal("Puzzle", savedToy!.Name);
            Assert.Equal("Fun puzzle.", savedToy.Description);
            Assert.Equal("12 pieces.", savedToy.AdditionalInformation);
            Assert.Equal("Cognitive", savedToy.Category);
            Assert.Equal("Wood", savedToy.Material);
            Assert.Equal("B09", savedToy.LocationCode);
            Assert.Equal("toy.jpg", savedToy.ImagePath);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            mockRepo.Setup(r => r.FindByIdAsync(77)).ReturnsAsync((Toy)null);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var result = await controller.Delete(77);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_DeletesToy_AndReturnsNoContent()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var toy = new Toy { ToyId = 44, Name = "Plane", IsAvailable = true, IsActive = true };
            mockRepo.Setup(r => r.FindByIdAsync(44)).ReturnsAsync(toy);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var result = await controller.Delete(44);

            mockRepo.Verify(r => r.DeleteAsync(44), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_WhenToyIsBorrowed()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();
            var auditLogger = new Mock<IAuditLogger>();
            var userHelper = new UserHelper(new HttpContextAccessor(), auditLogger.Object);

            var toy = new Toy { ToyId = 50, Name = "Train", IsAvailable = false, IsActive = true };
            mockRepo.Setup(r => r.FindByIdAsync(50)).ReturnsAsync(toy);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object, auditLogger.Object, userHelper);
            AttachUserToController(controller, GetTestUser());

            var result = await controller.Delete(50);

            Assert.IsType<BadRequestObjectResult>(result);
            mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
