using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Localization;
using Microsoft.Identity.Web;
using TMS_SharedLibrary.Audit.Enums;
using TMS_SharedLibrary.Auth;
using TMS_SharedLibrary.Models;
using TMS_SharedLibrary.Utilities;
using TMSStudent.Services;

namespace TMSStudent.Controllers;

[Authorize(Roles = "Student")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserAPIService _userAPIService;

    public HomeController(IUserAPIService userAPIService, ILogger<HomeController> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _userAPIService = userAPIService;
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            await EnsureUserExists();
            return RedirectToAction("Index", "Toy");
        }
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Dashboard");
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

        // Clear cookies and sign out properly
        var callbackUrl = Url.Action("Login", "Home", null, Request.Scheme);

        // Add no-cache headers to prevent back-button login
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        // This triggers OIDC middleware sign-out and redirect
        return SignOut(
            new AuthenticationProperties { RedirectUri = callbackUrl },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme
        );
    }

    [Authorize(Roles = "Student")]
    public IActionResult Dashboard()
    {
        return View("Index");
    }

    [Authorize(Roles = "Student")]
    public IActionResult Privacy()
    {
        return View();
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
            returnUrl = Url.Action("Index", "Home") ?? "/";
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

        return LocalRedirect(returnUrl);
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

            if (!User.IsInRole("Student"))
            {
                _logger.LogWarning($"User with OID: {oid} attempted to access student portal without Student role");
                return;
            }

            var user = await _userAPIService.GetUserByOidAsync(oid);

            if (user == null)
            {
                var newUser = new User
                {
                    Oid = oid,
                    UserType = "Student"
                };

                await _userAPIService.CreateUserAsync(newUser);
                _logger.LogInformation($"Created new Student user with OID: {oid}");
            }
            else if (user.UserType != "Student")
            {
                _logger.LogWarning($"User with OID: {oid} exists as '{user.UserType}' but is accessing the student portal. No Student record will exist.");
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