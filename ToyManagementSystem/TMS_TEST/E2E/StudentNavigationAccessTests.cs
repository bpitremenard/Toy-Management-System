using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

/// <summary>
/// Student navigation and access-control E2E tests.
/// Covers TC-040 to TC-053 and TC-088, TC-089.
/// Uses GlobalSetup for authentication and auth state reuse.
/// </summary>
/// 

//TEST MUST BE RUN USING STUDENT ACCOUNT - NOT OUR TESTS ACCOUNTS
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class StudentNavigationAccessTests : BrowserTest
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    private string StudentBaseUrl => GlobalSetup.StudentBaseUrl;
    private string TeacherBaseUrl => GlobalSetup.TeacherBaseUrl;

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

    private async Task GoToStudentHomeAsync()
    {
        await _page.GotoAsync($"{StudentBaseUrl}/Home/Index");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task GoToStudentToyPageAsync()
    {
        await _page.GotoAsync($"{StudentBaseUrl}/Toy");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task GoToStudentPrivacyPageAsync()
    {
        await _page.GotoAsync($"{StudentBaseUrl}/Home/Privacy");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task<int?> GetFirstToyIdFromReserveOrReturnLinkAsync()
    {
        await GoToStudentToyPageAsync();

        var actionLink = _page.Locator("a:has-text('Reserve'), a:has-text('Return')").First;
        if (await actionLink.CountAsync() == 0)
            return null;

        var href = await actionLink.GetAttributeAsync("href");
        if (string.IsNullOrWhiteSpace(href))
            return null;

        var match = Regex.Match(href, @"/(Borrow|Return)/(\d+)$");
        if (!match.Success)
            return null;

        return int.Parse(match.Groups[2].Value);
    }

    // ====================================================================
    // TC-040: Student Home route loads correctly
    // ====================================================================

    [Test]
    [Category("TC-040")]
    public async Task TC040_NavigationLinks_StudentHomeRouteLoadsCorrectly()
    {
        await GoToStudentHomeAsync();

        Assert.That(_page.Url, Does.Contain("/Home/Index").Or.Contain("/Toy"),
            "The authenticated student Home route should load or redirect to the student landing page.");
    }

    // ====================================================================
    // TC-041: Student Toy List route loads correctly
    // ====================================================================

    [Test]
    [Category("TC-041")]
    public async Task TC041_NavigationLinks_StudentToyListLoadsCorrectly()
    {
        await GoToStudentToyPageAsync();

        Assert.That(_page.Url, Does.Contain("/Toy"),
            "The student Toy page should load successfully.");
    }

    // ====================================================================
    // TC-042: Student direct access to teacher-only page is denied
    // ====================================================================

    [Test]
    [Category("TC-042")]
    public async Task TC042_AccessControl_StudentDirectTeacherBackupPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Backup");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Backup page.");
    }

    // ====================================================================
    // TC-043: Student Privacy route loads correctly
    // ====================================================================

    [Test]
    [Category("TC-043")]
    public async Task TC043_NavigationLinks_StudentPrivacyPageLoadsCorrectly()
    {
        await GoToStudentPrivacyPageAsync();

        Assert.That(_page.Url, Does.Contain("/Home/Privacy"),
            "The student Privacy page should load successfully.");
    }

    // ====================================================================
    // TC-045: Student cannot escalate to restricted teacher page by direct URL
    // ====================================================================

    [Test]
    [Category("TC-045")]
    public async Task TC045_NavigationLinks_DirectRestrictedBorrowHistoryAccessDenied()
    {
        await GoToStudentToyPageAsync();

        await _page.GotoAsync($"{TeacherBaseUrl}/Student/BorrowHistory/2");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access teacher borrow history pages by direct URL.");
    }

    // ====================================================================
    // TC-046: Student can navigate between all student-accessible pages
    // ====================================================================

    [Test]
    [Category("TC-046")]
    public async Task TC046_NavigationLinks_AllStudentAccessiblePagesLoadCorrectly()
    {
        await GoToStudentHomeAsync();
        Assert.That(_page.Url, Does.Contain("/Home/Index").Or.Contain("/Toy"));

        await GoToStudentToyPageAsync();
        Assert.That(_page.Url, Does.Contain("/Toy"));

        await GoToStudentPrivacyPageAsync();
        Assert.That(_page.Url, Does.Contain("/Home/Privacy"));
    }

    // ====================================================================
    // TC-047: Student remains authenticated while moving between pages
    // ====================================================================

    [Test]
    [Category("TC-047")]
    public async Task TC047_NavigationLinks_MultiPageNavigationDoesNotBreakSession()
    {
        await GoToStudentToyPageAsync();
        Assert.That(_page.Url, Does.Contain("/Toy"));

        await GoToStudentPrivacyPageAsync();
        Assert.That(_page.Url, Does.Contain("/Home/Privacy"));

        await GoToStudentHomeAsync();
        Assert.That(_page.Url, Does.Not.Contain("login.microsoftonline.com").And.Not.Contain("/Home/Login"),
            "The student should remain authenticated while moving between allowed pages.");
    }

    // ====================================================================
    // TC-088: Protected student page blocked after logout
    // ====================================================================

    [Test]
    [Category("TC-088")]
    public async Task TC088_LogOut_ProtectedStudentPageBlockedAfterLogout()
    {
        await GoToStudentToyPageAsync();
        var protectedPageUrl = _page.Url;

        await _page.GotoAsync($"{StudentBaseUrl}/Home/SignOutUser");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var freshContext = await Browser.NewContextAsync();
        var freshPage = await freshContext.NewPageAsync();

        await freshPage.GotoAsync(protectedPageUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        Assert.That(
            freshPage.Url,
            Does.Contain("login.microsoftonline.com").Or.Contain("/Home/Login").Or.Contain("AccessDenied"),
            "After logout, a student should not be able to access protected pages.");

        await freshContext.CloseAsync();
    }


    // ====================================================================
    // TC-089: Student direct access to teacher Toy Create page is denied
    // ====================================================================

    [Test]
    [Category("TC-089")]
    public async Task TC089_AccessControl_StudentDirectTeacherCreateToyPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Toy/Create");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Create Toy page.");
    }

    [Test]
    [Category("TC-048")]
    public async Task TC048_AccessControl_StudentDirectTeacherReportsPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Reports page.");
    }

    [Test]
    [Category("TC-049")]
    public async Task TC049_AccessControl_StudentDirectTeacherEditToyPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Toy/Edit/1");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Edit Toy page.");
    }

    [Test]
    [Category("TC-050")]
    public async Task TC050_AccessControl_StudentDirectTeacherDeleteToyPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Toy/Delete/1");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Delete Toy page.");
    }

    [Test]
    [Category("TC-051")]
    public async Task TC051_AccessControl_StudentDirectTeacherBackupPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Backup");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Backup page.");
    }

    [Test]
    [Category("TC-052")]
    public async Task TC052_AccessControl_StudentDirectTeacherBorrowHistoryPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Student/BorrowHistory/1");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Borrow History page.");
    }

    [Test]
    [Category("TC-053")]
    public async Task TC053_AccessControl_StudentDirectTeacherCreateToyPageDenied()
    {
        await _page.GotoAsync($"{TeacherBaseUrl}/Toy/Create");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.That(
            _page.Url,
            Does.Contain("AccessDenied").Or.Contain("Login").Or.Contain("login.microsoftonline.com"),
            "A student should not be able to access the teacher Create Toy page.");
    }
}
