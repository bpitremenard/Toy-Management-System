using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Moq;
using TMS_SharedLibrary.Models;
using TMSStudent.Controllers;
using TMSStudent.Services;
using Xunit;

namespace TMS_TEST.TMSStudent.ControllerTests
{
    public class ToyControllerSearchTests
    {
        [Fact]
        public async Task Index_SearchQuery_FiltersToys_AndBuildsBorrowedFlags()
        {
            var toyService = new Mock<IToyAPIService>();
            var studentService = new Mock<IStudentAPIService>();
            var tokenAcquisition = new Mock<ITokenAcquisition>();

            toyService.Setup(s => s.GetAll()).ReturnsAsync(new List<Toy>
            {
                new Toy { ToyId = 1, Name = "Car", Description = "Red toy", LocationCode = "A-1", IsAvailable = true, IsActive = true },
                new Toy { ToyId = 2, Name = "Rocket", Description = "Space toy", LocationCode = "SHELF-A", IsAvailable = false, IsActive = true },
                new Toy { ToyId = 3, Name = "Puzzle", Description = "Brain game", LocationCode = "C-9", IsAvailable = true, IsActive = true }
            });

            studentService.Setup(s => s.GetStudentID("student-oid")).ReturnsAsync(123);
            studentService.Setup(s => s.GetCurrentLoans(123)).ReturnsAsync(new List<ToyLoan>
            {
                new ToyLoan { LoanId = 9, ToyId = 2, StudentId = 123 }
            });

            var controller = new ToyController(
                toyService.Object,
                studentService.Object,
                tokenAcquisition.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[]
                        {
                            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "student-oid")
                        }, "mock"))
                }
            };

            var result = await controller.Index(searchQuery: "  shelf-a ");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ToyViewModel>>(viewResult.Model);

            var singleItem = Assert.Single(model);
            Assert.Equal(2, singleItem.Toy.ToyId);
            Assert.True(singleItem.BorrowedByCurrentStudent);
            Assert.Equal("shelf-a", controller.ViewBag.SearchQuery);
        }
    }
}
