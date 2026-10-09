using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using TMS_SharedLibrary.Models;
using TMSTeacher.Controllers;
using TMSTeacher.Services;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests
{
    public class StudentControllerTests
    {
        // Helper that builds a controller with all dependencies mocked,
        // so each test only needs to set up what it actually cares about.
        private static (StudentController controller,
                        Mock<IStudentAPIService> studentService,
                        Mock<IGraphAPIService> graphService,
                        Mock<ReportsPDFService> pdfService)
            BuildController()
        {
            var mockStudentService = new Mock<IStudentAPIService>();
            var mockUserService = new Mock<IUserAPIService>();
            var mockGraphService = new Mock<IGraphAPIService>();
            var mockLogger = new Mock<ILogger<StudentController>>();

            var mockLocalizerFactory = new Mock<IStringLocalizerFactory>();
            var mockStringLocalizer = new Mock<IStringLocalizer>();
            mockLocalizerFactory
                .Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockStringLocalizer.Object);

            var mockPdfService = new Mock<ReportsPDFService>(mockLocalizerFactory.Object);

            var controller = new StudentController(
                mockStudentService.Object,
                mockLogger.Object,
                mockUserService.Object,
                mockGraphService.Object,
                mockPdfService.Object
            );

            return (controller, mockStudentService, mockGraphService, mockPdfService);
        }

        [Fact]
        public async Task Download_GeneratesEnglishFileNameAndType()
        {
            var (controller, studentService, graphService, pdfService) = BuildController();

            var student = new Student
            {
                StudentId = 1,
                User = new User { Oid = "oid-123" }
            };

            var loans = new List<ToyLoan> { new ToyLoan { LoanId = 1 } };
            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

            studentService.Setup(s => s.Get(1)).ReturnsAsync(student);
            studentService.Setup(s => s.GetBorrowHistory(1)).ReturnsAsync(loans);

            // Mock IGraphAPIService instead of wiring up a real GraphServiceClient
            graphService
                .Setup(g => g.GetStudentInfoByOidAsync("oid-123"))
                .ReturnsAsync(new StudentGraphInfo
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                });

            pdfService
                .Setup(p => p.GenerateBorrowHistoryPdf(loans, "John Doe", true))
                .Returns(pdfBytes);

            var result = await controller.Download(1, languageOption: true);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.StartsWith("BorrowHistory_John Doe_", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task Download_GeneratesFrenchFileNameAndType()
        {
            var (controller, studentService, graphService, pdfService) = BuildController();

            var student = new Student
            {
                StudentId = 1,
                User = new User { Oid = "oid-123" }
            };

            var loans = new List<ToyLoan> { new ToyLoan { LoanId = 1 } };
            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

            studentService.Setup(s => s.Get(1)).ReturnsAsync(student);
            studentService.Setup(s => s.GetBorrowHistory(1)).ReturnsAsync(loans);

            graphService
                .Setup(g => g.GetStudentInfoByOidAsync("oid-123"))
                .ReturnsAsync(new StudentGraphInfo
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                });

            pdfService
                .Setup(p => p.GenerateBorrowHistoryPdf(loans, "John Doe", false))
                .Returns(pdfBytes);

            var result = await controller.Download(1, languageOption: false);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.StartsWith("HistoriqueEmprunt_John Doe_", fileResult.FileDownloadName);
        }
    }
}