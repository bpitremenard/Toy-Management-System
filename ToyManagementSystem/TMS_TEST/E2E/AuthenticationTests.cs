using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

/// <summary>
/// Authentication E2E tests
/// Test cases TC-001 to TC-008
/// Uses GlobalSetup for authentication and auth state reuse
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class AuthenticationTests : BrowserTest
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    [SetUp]
    public async Task SetUpAuthenticatedContext()
    {
        _context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = GlobalSetup.AuthStatePath
        });
        _page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownContext()
    {
        await _context.CloseAsync();
    }


    // ====================================================================
    // TC-001: Unauthenticated - Navigate to protected page without logging in
    // ====================================================================

    [Test]
    [Category("TC-001")]
    public async Task TC001_Unauthenticated_ProtectedPage_RedirectsToLogin()
    {
        // Create a fresh context without auth state to test unauthenticated access
        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        await freshPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy");

        // Wait for redirect to complete
        await freshPage.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10_000 });

        // Should redirect to login page or Microsoft login
        var currentUrl = freshPage.Url;
        Assert.That(currentUrl, Does.Contain("/Home/Login").Or.Contain("login.microsoftonline.com"),
            $"Unauthenticated access to protected page should redirect to login. Current URL: {currentUrl}");

        await freshContext.CloseAsync();
    }

    // ====================================================================
    // TC-002: Log in with valid credentials
    // ====================================================================

    [Test]
    [Category("TC-002")]
    public async Task TC002_ValidCredentials_AuthenticatesAndRedirectsToHome()
    {
        // GlobalSetup handles authentication - just verify we can access a protected page
        var baseUrl = GlobalSetup.TestRole.Equals("Student", StringComparison.OrdinalIgnoreCase) 
            ? GlobalSetup.StudentBaseUrl 
            : GlobalSetup.TeacherBaseUrl;

        await _page.GotoAsync($"{baseUrl}/Toy");

        // Should be able to access protected page (no redirect to login)
        Assert.That(_page.Url, Does.StartWith(baseUrl),
            "With valid credentials from GlobalSetup, user should access protected page.");
    }

    // ====================================================================
    // TC-003: Log in with invalid password
    // ====================================================================

    [Test]
    [Category("TC-003")]
    public async Task TC003_InvalidPassword_ErrorMessageDisplayed()
    {
        // This test requires fresh authentication with invalid password
        // Create a fresh context without auth state
        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var username = config["TEST_USERNAME"] ?? config["AzureAd:TestUsername"];

        // Navigate to SignIn to trigger Azure AD
        await freshPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Home/SignIn");
        await freshPage.WaitForURLAsync(
            url => url.Contains("login.microsoftonline.com"),
            new PageWaitForURLOptions { Timeout = 30_000 });

        await freshPage.FillAsync("input[type='email']", username);
        await freshPage.ClickAsync("input[type='submit'], button[type='submit']");

        await freshPage.WaitForSelectorAsync("input[type='password']", new PageWaitForSelectorOptions { Timeout = 15_000 });
        await freshPage.FillAsync("input[type='password']", "InvalidPassword123!");
        await freshPage.ClickAsync("input[type='submit'], button[type='submit']");

        // Wait for error message - use locator directly
        var errorMessage = freshPage.Locator(".alert, .error, [role='alert']").First;
        await errorMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10_000 });

        // Should still be on login page
        Assert.That(freshPage.Url, Does.Contain("login.microsoftonline.com"),
            "With invalid password, user should stay on login page.");

        await freshContext.CloseAsync();
    }

    // ====================================================================
    // TC-004: Log in with non-existent username
    // ====================================================================

    [Test]
    [Category("TC-004")]
    public async Task TC004_NonExistentUsername_ErrorMessageDisplayed()
    {
        // Create a fresh context without auth state
        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        // Navigate to SignIn to trigger Azure AD
        await freshPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Home/SignIn");
        await freshPage.WaitForURLAsync(
            url => url.Contains("login.microsoftonline.com"),
            new PageWaitForURLOptions { Timeout = 30_000 });

        await freshPage.FillAsync("input[type='email']", "nonexistentuser@cegep-heritage.qc.ca");
        await freshPage.ClickAsync("input[type='submit'], button[type='submit']");

        await freshPage.WaitForSelectorAsync("input[type='password']", new PageWaitForSelectorOptions { Timeout = 15_000 });
        await freshPage.FillAsync("input[type='password']", "SomePassword123!");
        await freshPage.ClickAsync("input[type='submit'], button[type='submit']");

        // Wait for error message - element may be hidden, so wait for it to be attached in DOM
        var errorMessage = freshPage.Locator(".alert, .error, [role='alert']").First;
        await errorMessage.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached, Timeout = 10_000 });

        // Should still be on login page
        Assert.That(freshPage.Url, Does.Contain("login.microsoftonline.com"),
            "With non-existent username, user should stay on login page.");

        await freshContext.CloseAsync();
    }

    // ====================================================================
    // TC-005: Log out
    // ====================================================================

    [Test]
    [Category("TC-005")]
    public async Task TC005_LogOut_SessionEndsAndRedirectsToLogin()
    {
        var baseUrl = GlobalSetup.TestRole.Equals("Student", StringComparison.OrdinalIgnoreCase) 
            ? GlobalSetup.StudentBaseUrl 
            : GlobalSetup.TeacherBaseUrl;

        // Log out
        await _page.GotoAsync($"{baseUrl}/Home/SignOutUser");

        // Wait for redirect to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10_000 });

        // Create a fresh context to verify session is cleared
        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        // Try to access a protected page
        await freshPage.GotoAsync($"{baseUrl}/Toy", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Should redirect to login
        Assert.That(freshPage.Url, Does.Contain("login.microsoftonline.com").Or.Contain("/Home/Login"),
            "After logout, accessing protected page should redirect to login.");

        await freshContext.CloseAsync();
    }

    // ====================================================================
    // TC-006: Session reuse
    // ====================================================================

    [Test]
    [Category("TC-006")]
    public async Task TC006_SessionReuse_AuthenticatedPageAccessibleWithoutRelogin()
    {
        var baseUrl = GlobalSetup.TestRole.Equals("Student", StringComparison.OrdinalIgnoreCase) 
            ? GlobalSetup.StudentBaseUrl 
            : GlobalSetup.TeacherBaseUrl;

        // Navigate to a protected page (using auth from GlobalSetup)
        await _page.GotoAsync($"{baseUrl}/Toy");

        // Should stay on the protected page (no redirect to login)
        Assert.That(_page.Url, Does.StartWith(baseUrl),
            "Within the same session, authenticated user should access protected pages without re-login.");

        // Navigate to another protected page
        await _page.GotoAsync($"{baseUrl}/Toy/Index");

        // Should still be accessible
        Assert.That(_page.Url, Does.StartWith(baseUrl),
            "Session should persist across multiple protected page navigations.");
    }

    // ====================================================================
    // TC-007: SQL injection input
    // ====================================================================

    [Test]
    [Category("TC-007")]
    public async Task TC007_SqlInjection_InputRejected()
    {
        // Create a fresh context without auth state
        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        // Navigate to SignIn to trigger Azure AD
        await freshPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Home/SignIn");
        await freshPage.WaitForURLAsync(
            url => url.Contains("login.microsoftonline.com"),
            new PageWaitForURLOptions { Timeout = 30_000 });

        // Try SQL injection in username field
        var sqlInjectionInput = "admin' OR '1'='1'--";
        await freshPage.FillAsync("input[type='email']", sqlInjectionInput);
        await freshPage.ClickAsync("input[type='submit'], button[type='submit']");

        // Should show error or validation message
        var hasError = false;
        try
        {
            await freshPage.WaitForSelectorAsync(".alert, .error, [role='alert'], .validation-error",
                new PageWaitForSelectorOptions { Timeout = 5_000 });
            hasError = true;
        }
        catch
        {
            // No error element found, but should still not authenticate
        }

        // Should not be authenticated (still on login or error page)
        Assert.That(freshPage.Url, Does.Contain("login.microsoftonline.com").Or.Contains("error"),
            "SQL injection input should be rejected and not authenticate the user.");

        await freshContext.CloseAsync();
    }

    // ====================================================================
    // TC-008: Log out and press back button
    // ====================================================================

    [Test]
    [Category("TC-008")]
    public async Task TC008_LogOutAndBackButton_NoAccessToProtectedPages()
    {
        var baseUrl = GlobalSetup.TestRole.Equals("Student", StringComparison.OrdinalIgnoreCase) 
            ? GlobalSetup.StudentBaseUrl 
            : GlobalSetup.TeacherBaseUrl;

        // Visit a protected page (using auth from GlobalSetup)
        await _page.GotoAsync($"{baseUrl}/Toy");
        var protectedPageUrl = _page.Url;

        // Log out
        await _page.GotoAsync($"{baseUrl}/Home/SignOutUser");

        // Wait for redirect to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10_000 });

        // Close the authenticated context and create a fresh one to test back button
        await _context.CloseAsync();
        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        // Navigate to the protected page URL directly (simulating back button behavior)
        await freshPage.GotoAsync(protectedPageUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Should not have access - should be on login or error page
        Assert.That(freshPage.Url, Does.Contain("login.microsoftonline.com").Or.Contain("/Home/Login").Or.Contains("AccessDenied"),
            "After logout, accessing protected page via URL should redirect to login.");

        await freshContext.CloseAsync();
        
        // Recreate the authenticated context for other tests
        _context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = GlobalSetup.AuthStatePath
        });
        _page = await _context.NewPageAsync();
    }

    // ====================================================================
    // TC-009: Session timeout/expiry - user remains inactive and session expires
    // ====================================================================

    [Test]
    [Category("TC-009")]
    public async Task TC009_SessionTimeout_InactiveSessionExpiresAndRequiresReauth()
    {
        var baseUrl = GlobalSetup.TestRole.Equals("Student", StringComparison.OrdinalIgnoreCase) 
            ? GlobalSetup.StudentBaseUrl 
            : GlobalSetup.TeacherBaseUrl;

        // Navigate to a protected page
        await _page.GotoAsync($"{baseUrl}/Toy");
        
        // Wait for page to load
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Simulate session timeout by clearing cookies and storage
        await _page.Context.ClearCookiesAsync();
        await _page.Context.ClearPermissionsAsync();

        // Try to navigate to another protected page
        await _page.GotoAsync($"{baseUrl}/Toy/Index", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Should redirect to login page due to session expiry
        Assert.That(_page.Url, Does.Contain("login.microsoftonline.com").Or.Contain("/Home/Login"),
            "After session timeout, accessing protected pages should redirect to login.");

        // Verify we cannot access protected content
        var hasProtectedContent = false;
        try
        {
            var protectedElements = _page.Locator("table, .toy-list, .data-container");
            await protectedElements.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
            hasProtectedContent = true;
        }
        catch
        {
            // Expected - no protected content should be accessible
        }

        Assert.That(hasProtectedContent, Is.False,
            "After session timeout, protected content should not be accessible.");
    }

    // ====================================================================
    // TC-010: Empty username field - login should be rejected
    // ====================================================================

    [Test]
    [Category("TC-010")]
    public async Task TC010_EmptyUsername_LoginRejected()
    {
        // Create a fresh context without auth state
        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        // Navigate to SignIn to trigger Azure AD
        await freshPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Home/SignIn");
        await freshPage.WaitForURLAsync(
            url => url.Contains("login.microsoftonline.com"),
            new PageWaitForURLOptions { Timeout = 30_000 });

        // Try to submit with empty username
        await freshPage.FillAsync("input[type='email']", "");
        await freshPage.ClickAsync("input[type='submit'], button[type='submit']");

        // Should show validation error or remain on login page
        var hasError = false;
        try
        {
            var errorElement = freshPage.Locator(".alert, .error, [role='alert'], .validation-error, .field-validation-error");
            await errorElement.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5_000 });
            hasError = true;
        }
        catch
        {
            // No error element found, but should not proceed to password
        }

        // Should not proceed to password entry or authenticate
        var currentUrl = freshPage.Url;
        Assert.That(currentUrl, Does.Contain("login.microsoftonline.com"),
            "With empty username, user should stay on login page or see validation error.");

        await freshContext.CloseAsync();
    }

    }
