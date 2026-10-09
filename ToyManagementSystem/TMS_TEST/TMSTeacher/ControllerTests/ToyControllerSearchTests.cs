using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using TMSTeacher.Controllers;
using TMSTeacher.Models;
using TMSTeacher.Services;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests
{
    public class ToyControllerSearchTests
    {
        private static ToyController BuildController(IEnumerable<Toy> toys)
        {
            var studentService = new Mock<IStudentAPIService>();
            var toyService = new Mock<IToyAPIService>();
            var teacherService = new Mock<ITeacherAPIService>();
            var geminiService = new Mock<IGeminiToySuggestionService>();
            var graphService = new Mock<IGraphAPIService>();
            var webHost = new Mock<IWebHostEnvironment>();
            var config = new Mock<IConfiguration>();
            var logger = new Mock<ILogger<ToyController>>();

            toyService.Setup(s => s.GetAll()).ReturnsAsync(toys);

            var controller = new ToyController(
                studentAPIService: studentService.Object,
                toyAPIService: toyService.Object,
                teacherAPIService: teacherService.Object,
                geminiToySuggestionService: geminiService.Object,
                graphAPIService: graphService.Object,
                logger: logger.Object,
                imgs: webHost.Object,
                config: config.Object);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            return controller;
        }

        [Fact]
        public async Task Index_SearchByExactToyName_ReturnsOnlyMatchingToys()
        {
            var controller = BuildController(new List<Toy>
            {
                new Toy { ToyId = 1, Name = "Blue Ball", IsActive = true, IsAvailable = true },
                new Toy { ToyId = 2, Name = "Red Truck", IsActive = true, IsAvailable = true }
            });

            var result = await controller.Index(null, null, "Blue Ball", true);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ToyWithStudentViewModel>>(viewResult.Model).ToList();

            var onlyResult = Assert.Single(model);
            Assert.Equal("Blue Ball", onlyResult.Toy.Name);
        }

        [Fact]
        public async Task Index_SearchByPartialName_ReturnsAllContainingKeyword()
        {
            var controller = BuildController(new List<Toy>
            {
                new Toy { ToyId = 1, Name = "Ball", IsActive = true, IsAvailable = true },
                new Toy { ToyId = 2, Name = "Beach Ball", IsActive = true, IsAvailable = true },
                new Toy { ToyId = 3, Name = "Soccer Ball", IsActive = true, IsAvailable = false },
                new Toy { ToyId = 4, Name = "Truck", IsActive = true, IsAvailable = true }
            });

            var result = await controller.Index(null, null, "ball", true);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ToyWithStudentViewModel>>(viewResult.Model).ToList();

            Assert.Equal(3, model.Count);
            Assert.DoesNotContain(model, t => t.Toy.Name == "Truck");
        }

        [Fact]
        public async Task Index_NoMatches_WhenAlreadyRedirected_ReturnsEmptyResultAndNoToysFoundMessage()
        {
            var controller = BuildController(new List<Toy>
            {
                new Toy { ToyId = 1, Name = "Truck", IsActive = true, IsAvailable = true },
                new Toy { ToyId = 2, Name = "Puzzle", IsActive = true, IsAvailable = true }
            });

            var result = await controller.Index(null, null, "spaceship", true);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ToyWithStudentViewModel>>(viewResult.Model);

            Assert.Empty(model);
            Assert.Equal("No toys found", controller.TempData["NoToysFoundMessage"]);
        }

    }
}