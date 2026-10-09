using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

/// <summary>
/// Reports Download E2E tests
/// Test cases TC-050, TC-051, TC-052, TC-053, TC-083
///
/// The export buttons are on the individual student borrow history page (/Student/BorrowHistory/{id}).
/// Both buttons are always present regardless of the app's current UI language.
///
/// From the Razor view:
///   English PDF → form with input[name="languageOption"][value="true"]  → button.tms-btn
///   French PDF  → form with input[name="languageOption"][value="false"] → button.tms-btn
///
/// PDF filenames (from screenshots):
///   English → BorrowHistory_{StudentName}_{Date}.pdf
///   French  → HistoriqueEmprunt_{StudentName}_{Date}.pdf
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category("Teacher")]
public class ReportsDownloadTests : BrowserTest
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

    /// <summary>
    /// Navigates to /Student and clicks VIEW BORROW HISTORY for the first student,
    /// landing on the page that contains both PDF export buttons.
    /// </summary>
    private async Task NavigateToStudentBorrowHistoryPage()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.WaitForSelectorAsync(".tms-student-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        await _page.Locator(".tms-student-row").First
            .Locator(".tms-actions a.tms-btn").ClickAsync();

        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Wait for the visible export buttons inside .download-btn-left to confirm the page loaded
        // (the languageOption inputs are type="hidden" and are never "visible" to Playwright)
        await _page.WaitForSelectorAsync(".download-btn-left button.tms-btn",
            new PageWaitForSelectorOptions { Timeout = 30_000 });
    }

    // ====================================================================
    // TC-050: Teacher exports PDF in English — file downloads successfully
    // ====================================================================

    [Test]
    [Category("TC-050")]
    public async Task TC050_ExportPDF_English_DownloadsSuccessfully()
    {
        await NavigateToStudentBorrowHistoryPage();

        // English export: the form with languageOption=true
        var downloadTask = _page.WaitForDownloadAsync();
        await _page.Locator("form:has(input[name='languageOption'][value='true']) button.tms-btn").ClickAsync();
        var download = await downloadTask;

        Assert.That(download, Is.Not.Null,
            "Clicking the English PDF export button should trigger a file download.");
        Assert.That(download.SuggestedFilename, Does.EndWith(".pdf"),
            "Downloaded file should have a .pdf extension.");
    }

    // ====================================================================
    // TC-051: Teacher exports PDF in French — file downloads successfully
    // ====================================================================

    [Test]
    [Category("TC-051")]
    public async Task TC051_ExportPDF_French_DownloadsSuccessfully()
    {
        await NavigateToStudentBorrowHistoryPage();

        // French export: the form with languageOption=false
        var downloadTask = _page.WaitForDownloadAsync();
        await _page.Locator("form:has(input[name='languageOption'][value='false']) button.tms-btn").ClickAsync();
        var download = await downloadTask;

        Assert.That(download, Is.Not.Null,
            "Clicking the French PDF export button should trigger a file download.");
        Assert.That(download.SuggestedFilename, Does.EndWith(".pdf"),
            "Downloaded file should have a .pdf extension.");
    }

    // ====================================================================
    // TC-052: English PDF — filename starts with "BorrowHistory"
    // ====================================================================

    [Test]
    [Category("TC-052")]
    public async Task TC052_ExportPDF_English_FilenameIsInEnglish()
    {
        await NavigateToStudentBorrowHistoryPage();

        var downloadTask = _page.WaitForDownloadAsync();
        await _page.Locator("form:has(input[name='languageOption'][value='true']) button.tms-btn").ClickAsync();
        var download = await downloadTask;

        // English PDF filename pattern: BorrowHistory_{StudentName}_{Date}.pdf
        Assert.That(download.SuggestedFilename, Does.StartWith("BorrowHistory"),
            $"English PDF filename should start with 'BorrowHistory'. Actual: '{download.SuggestedFilename}'");
        Assert.That(download.SuggestedFilename, Does.EndWith(".pdf"),
            "Downloaded file should have a .pdf extension.");
    }

    // ====================================================================
    // TC-053: French PDF — filename starts with "HistoriqueEmprunt"
    // ====================================================================

    [Test]
    [Category("TC-053")]
    public async Task TC053_ExportPDF_French_FilenameIsInFrench()
    {
        await NavigateToStudentBorrowHistoryPage();

        var downloadTask = _page.WaitForDownloadAsync();
        await _page.Locator("form:has(input[name='languageOption'][value='false']) button.tms-btn").ClickAsync();
        var download = await downloadTask;

        // French PDF filename pattern: HistoriqueEmprunt_{StudentName}_{Date}.pdf
        Assert.That(download.SuggestedFilename, Does.StartWith("HistoriqueEmprunt"),
            $"French PDF filename should start with 'HistoriqueEmprunt'. Actual: '{download.SuggestedFilename}'");
        Assert.That(download.SuggestedFilename, Does.EndWith(".pdf"),
            "Downloaded file should have a .pdf extension.");
    }

    // ====================================================================
    // TC-083: Student with no borrow records — export still works and page
    //         shows the tms-no-data empty state (no history rows rendered)
    // ====================================================================

    [Test]
    [Category("TC-083")]
    public async Task TC083_ExportPDF_NoToyData_EmptyStateIsShownAndExportSucceeds()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.WaitForSelectorAsync(".tms-student-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Navigate to the student who has no borrow records
        // TODO: Replace with the actual name of a student with no records on the test server
        var targetRow = _page.Locator(".tms-student-row").Filter(new LocatorFilterOptions
        {
            HasText = "Gil Audrey" 
        });
        await targetRow.Locator(".tms-actions a.tms-btn").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Verify the empty state message is shown (view renders div.tms-no-data when Model is empty)
        var noDataDiv = _page.Locator(".tms-no-data");
        await Expect(noDataDiv).ToBeVisibleAsync();

        // Verify no history rows are rendered
        Assert.That(await _page.Locator(".tms-history-row").CountAsync(), Is.EqualTo(0),
            "There should be no history rows when the student has no borrow records.");

        // Wait for the visible export button (not the hidden input) before clicking
        await _page.WaitForSelectorAsync(".download-btn-left button.tms-btn",
            new PageWaitForSelectorOptions { Timeout = 10_000 });

        var downloadTask = _page.WaitForDownloadAsync();
        await _page.Locator("form:has(input[name='languageOption'][value='true']) button.tms-btn").ClickAsync();
        var download = await downloadTask;

        Assert.That(download, Is.Not.Null,
            "PDF export should complete without error even when the student has no borrow records.");
        Assert.That(download.SuggestedFilename, Does.EndWith(".pdf"),
            "Downloaded file should still be a PDF for an empty dataset.");
    }
}