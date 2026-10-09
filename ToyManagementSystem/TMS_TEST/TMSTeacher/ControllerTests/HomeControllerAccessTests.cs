using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TMSTeacher.Controllers;
using TMSTeacher.Services;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests
{
    public class HomeControllerAccessTests
    {
        [Fact]
        public async Task Index_RedirectsToAccessDenied_WhenAuthenticatedButNotTeacherOrAdmin()
        {
            var logger = new Mock<ILogger<HomeController>>();
            var notificationService = new Mock<INotificationService>();
            var userApiService = new Mock<IUserAPIService>();

            var controller = new HomeController(logger.Object, notificationService.Object, userApiService.Object);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "student@example.test"),
                new Claim(ClaimTypes.Role, "Student")
            }, authenticationType: "TestAuth");

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            var result = await controller.Index();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirect.ActionName);
        }

        [Fact]
        public void Login_RedirectsToAccessDenied_WhenAuthenticatedButNotTeacherOrAdmin()
        {
            var logger = new Mock<ILogger<HomeController>>();
            var notificationService = new Mock<INotificationService>();
            var userApiService = new Mock<IUserAPIService>();

            var controller = new HomeController(logger.Object, notificationService.Object, userApiService.Object);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "student@example.test"),
                new Claim(ClaimTypes.Role, "Student")
            }, authenticationType: "TestAuth");

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            var result = controller.Login();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirect.ActionName);
        }
    }
}
