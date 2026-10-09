using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using TMS_SharedLibrary.Models;
using TMS_SharedLibrary.ViewModels;
using TMSTeacher.Services;

namespace TMSTeacher.Controllers
{
    [Authorize(Policy = "RequireTeacherRole")]
    public class StudentController : Controller
    {
        private readonly ReportsPDFService _pdfService;
        private readonly IStudentAPIService _studentAPIService;
        private readonly IUserAPIService _userService;
        private readonly IGraphAPIService _graphAPIService; 
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IStudentAPIService studentAPIService,
            ILogger<StudentController> logger,
            IUserAPIService userAPIService,
            IGraphAPIService graphAPIService,              
            ReportsPDFService pdfService)
        {
            _pdfService = pdfService;
            _studentAPIService = studentAPIService;
            _userService = userAPIService;
            _graphAPIService = graphAPIService;       
            _logger = logger;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var students = await _studentAPIService.GetAll();
                var studentViewModels = new List<StudentViewModel>();

                foreach (var student in students)
                {
                    if (student.User == null || string.IsNullOrEmpty(student.User.Oid))
                    {
                        studentViewModels.Add(new StudentViewModel
                        {
                            StudentId = student.StudentId,
                            FirstName = "User",
                            LastName = "Not Found",
                            Email = "N/A",
                            Oid = "Missing OID"
                        });
                        continue;
                    }

                    var info = await _graphAPIService.GetStudentInfoByOidAsync(student.User.Oid);

                    studentViewModels.Add(new StudentViewModel
                    {
                        StudentId = student.StudentId,
                        FirstName = info?.FirstName ?? "User",
                        LastName = info?.LastName ?? "Not Found in AD",
                        Email = info?.Email ?? "N/A",
                        Oid = student.User.Oid
                    });
                }

                return View(studentViewModels);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading students");
                TempData["ErrorMessage"] = "Unable to load students right now. Please try again shortly.";
                return View(Enumerable.Empty<StudentViewModel>());
            }
        }

        [Authorize]
        public async Task<IActionResult> BorrowHistory(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null) return NotFound();

                var borrowHistory = await _studentAPIService.GetBorrowHistory(id.Value);

                var info = await _graphAPIService.GetStudentInfoByOidAsync(student.User?.Oid ?? "");
                ViewBag.StudentName = info != null
                    ? $"{info.FirstName} {info.LastName}"
                    : "Student";
                ViewBag.StudentId = id.Value;

                return View(borrowHistory);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student borrow history");
                TempData["ErrorMessage"] = "Unable to load the borrow history right now. Please try again shortly.";
                ViewBag.StudentName = "Student";
                ViewBag.StudentId = id.Value;
                return View(Enumerable.Empty<ToyLoan>());
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Download(int? studentId, bool languageOption)
        {
            if (studentId == null) return NotFound();

            try
            {
                var student = await _studentAPIService.Get(studentId.Value);
                if (student == null) return NotFound();

                var loans = await _studentAPIService.GetBorrowHistory(studentId.Value);

                var info = await _graphAPIService.GetStudentInfoByOidAsync(student.User?.Oid ?? "");
                var studentName = info != null ? $"{info.FirstName} {info.LastName}" : "Student";

                var originalCulture = CultureInfo.CurrentCulture;
                var originalUICulture = CultureInfo.CurrentUICulture;

                try
                {
                    var culture = languageOption ? "en" : "fr";
                    var cultureInfo = new CultureInfo(culture);
                    CultureInfo.CurrentCulture = cultureInfo;
                    CultureInfo.CurrentUICulture = cultureInfo;

                    var pdfBytes = _pdfService.GenerateBorrowHistoryPdf(loans, studentName, languageOption);
                    var safeStudentName = string.Join("_", studentName.Split(Path.GetInvalidFileNameChars()));
                    var fileName = languageOption
                        ? $"BorrowHistory_{safeStudentName}_{DateTime.Now:yyyyMMdd}.pdf"
                        : $"HistoriqueEmprunt_{safeStudentName}_{DateTime.Now:yyyyMMdd}.pdf";

                    return File(pdfBytes, "application/pdf", fileName);
                }
                finally
                {
                    CultureInfo.CurrentCulture = originalCulture;
                    CultureInfo.CurrentUICulture = originalUICulture;
                }
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating or downloading PDF");
                TempData["ErrorMessage"] = "Unable to generate the PDF right now. Please try again shortly.";
                return RedirectToAction(nameof(BorrowHistory), new { id = studentId });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null) return NotFound();

                return View(student);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id}");
                return StatusCode(500, "An error occurred while retrieving the student details.");
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null) return NotFound();

                return View(student);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id} for editing");
                return StatusCode(500, "An error occurred while retrieving the student for editing.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentId,UserId,Placement,Year")] Student student)
        {
            if (id != student.StudentId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _studentAPIService.Update(student);
                    return RedirectToAction(nameof(Index));
                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    _logger.LogInformation("Triggering consent flow for API access");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating student with ID {id}");
                    ModelState.AddModelError("", "An error occurred while updating the student.");
                }
            }
            return View(student);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null) return NotFound();

                return View(student);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id} for deletion");
                return StatusCode(500, "An error occurred while retrieving the student for deletion.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _studentAPIService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting student with ID {id}");
                return StatusCode(500, "An error occurred while deleting the student.");
            }
        }
    }
}