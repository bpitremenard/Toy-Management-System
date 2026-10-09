using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using TMS_SharedLibrary.Audit.Enums;
using System.Globalization;
using TMS_SharedLibrary.Models;
using TMSTeacher.Services;

namespace TMSTeacher.Controllers;

[Authorize(Roles = "Admin,Teacher")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserAPIService _userAPIService;

    public HomeController(
        ILogger<HomeController> logger,
        INotificationService notificationService,
        IUserAPIService userAPIService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _userAPIService = userAPIService;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        await EnsureUserExists();
        return RedirectToAction("Index", "Toy");
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> TeacherDashboard()
    {
        await EnsureUserExists();
        return RedirectToAction("Index", "Toy");
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            await EnsureUserExists();

            if (User.IsInRole("Admin"))
                return RedirectToAction("AdminDashboard");
            else if (User.IsInRole("Teacher"))
                return RedirectToAction("TeacherDashboard");
            return RedirectToAction("AccessDenied");
        }
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("AdminDashboard");
            else if (User.IsInRole("Teacher"))
                return RedirectToAction("TeacherDashboard");
            return RedirectToAction("AccessDenied");
        }
        return View();
    }

    [AllowAnonymous]
    public IActionResult SignIn()
    {
        var redirectUrl = Url.Action("Index", "Home");
        return Challenge(
            new AuthenticationProperties { RedirectUri = redirectUrl },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    public async Task<IActionResult> SignOutUser()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var oidClaim = User.FindFirst("oid")?.Value ?? "unknown";

            // Log logout to the API
            await _userAPIService.LogAuditAsync(
                AuditAction.Logout,
                nameof(User),
                0,
                "logged out"
            );
        }

        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return SignOut(
     new AuthenticationProperties(), 
     OpenIdConnectDefaults.AuthenticationScheme,
     CookieAuthenticationDefaults.AuthenticationScheme
 );
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var oidClaim = User.FindFirst("oid") ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (oidClaim == null) return Json(new List<Notification>());

        var notifications = await _notificationService.GetUserNotificationsAsync(oidClaim.Value);
        return Json(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationCount()
    {
        var oidClaim = User.FindFirst("oid") ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (oidClaim == null) return Json(0);

        var count = await _notificationService.GetUnreadCountAsync(oidClaim.Value);
        return Json(count);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteNotification([FromBody] int notificationId)
    {
        var result = await _notificationService.DeleteNotificationAsync(notificationId);
        return Json(new { success = result });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        var oidClaim = User.FindFirst("oid") ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (oidClaim == null) return Json(new { success = false });

        var result = await _notificationService.DeleteAllNotificationsAsync(oidClaim.Value);
        return Json(new { success = result });
    }

    [HttpGet]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = "en-CA";
        }

        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            // For authenticated users, redirect to the current page to stay in context
            returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
            
            // If the current path is empty or root, redirect to appropriate dashboard
            if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    returnUrl = Url.Action("Index", "Toy") ?? "/";
                }
                else
                {
                    returnUrl = Url.Action("Index", "Home") ?? "/";
                }
            }
        }

        // Ensure the URL starts with /teacher/ for the teacher application
        if (!returnUrl.StartsWith("/teacher/"))
        {
            returnUrl = "/teacher" + (returnUrl.StartsWith("/") ? returnUrl : "/" + returnUrl);
        }

        var requestCulture = new RequestCulture(new CultureInfo(culture));
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(requestCulture),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

        // Use Redirect instead of LocalRedirect to preserve the application base path
        return Redirect(returnUrl);
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task EnsureUserExists()
    {
        try
        {
            var oid = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                   ?? User.FindFirst("oid")?.Value;

            if (string.IsNullOrEmpty(oid))
            {
                _logger.LogWarning("No OID claim found for user");
                return;
            }

            var user = await _userAPIService.GetUserByOidAsync(oid);

            if (user == null)
            {
                // Validate user has at least one valid role before creating account
                if (!User.IsInRole("Student") && !User.IsInRole("Teacher") && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning($"User with OID: {oid} attempted to create account but lacks any valid role");
                    return;
                }

                string userType = "Student"; 
                if (User.IsInRole("Admin") || User.IsInRole("Teacher"))
                {
                    userType = "Teacher";
                }

                var newUser = new User
                {
                    Oid = oid,
                    UserType = userType
                };

                await _userAPIService.CreateUserAsync(newUser);
                _logger.LogInformation($"Created new {userType} user with OID: {oid}");
            }
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            _logger.LogInformation("User needs to consent to API access. Redirecting to consent page.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring user exists");
        }
    }

}
