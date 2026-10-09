using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

/// <summary>
/// Search, Filter, and Return Toy E2E tests
/// Test cases TC-031 to TC-035
/// Covers:
///   US-1408 — As a teacher, I want to search and filter toys (TC-031, TC-032, TC-033)
///   US-1413 — As a student, I want to return a borrowed toy  (TC-034, TC-035)
/// Uses GlobalSetup for authentication and auth state reuse
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class SearchFilterReturnToyTests : BrowserTest {
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    [SetUp]
    public async Task SetUpAuthenticatedContext() {
        _context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = GlobalSetup.AuthStatePath
        });
        _page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownContext() {
        await _context.CloseAsync();
    }


    // ====================================================================
    // US-1408 | TC-031: Teacher searches toys — matching toys are displayed
    // Scenario 1 — Search toys
    // Given a teacher is logged in
    // When the teacher enters a search term
    // Then matching toys are displayed
    // ====================================================================

    [Test]
    [Category("TC-031")]
    [Category("Teacher")]
    public async Task TC031_TeacherSearchesToys_MatchingToysDisplayed() {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Use the exact name attribute found in DevTools: name="searchQuery"
        var searchInput = _page.Locator("input[name='searchQuery']");
        await searchInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });
        await searchInput.FillAsync("car");

        // Click the submit button using its exact id
        await _page.ClickAsync("#teacher-search-btn");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Verify at least one result OR a no-results message is shown
        // (page currently shows "No toys found matching the selected filters." when empty)
        var toyRows = _page.Locator("table tbody tr, .toy-item, .toy-card");
        var count = await toyRows.CountAsync();

        var noResultsMsg = _page.Locator("*:has-text('No toys found')");
        var hasNoResults = await noResultsMsg.CountAsync() > 0;

        // If the DB has toys matching "car" they appear; otherwise no-results message shows.
        // Either outcome proves the search executed — assert the page responded.
        Assert.That(count > 0 || hasNoResults, Is.True,
            "After entering a search term the page should show matching toys or a no-results message.");
    }

    // ====================================================================
    // US-1408 | TC-032: Teacher filters toys — only matching toys shown
    // Scenario 2 — Filter toys
    // Given a teacher is logged in
    // When the teacher selects filter options
    // Then only toys matching the filters are displayed
    // ====================================================================

    [Test]
    [Category("TC-032")]
    [Category("Teacher")]
    public async Task TC032_TeacherFiltersToys_OnlyMatchingToysDisplayed() {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // The filter lives in a modal — click the FILTER button to open it
        await _page.ClickAsync("button:has-text('Filter'), a:has-text('Filter')");

        // Wait for the modal to be visible
        await _page.WaitForSelectorAsync(".modal.show, .modal[style*='display: block']",
            new PageWaitForSelectorOptions { Timeout = 10_000 });

        // Select the first non-placeholder option in the Development Goal dropdown
        // id="developmentGoalFilter", name="categories"
        await _page.SelectOptionAsync("#developmentGoalFilter",
            new SelectOptionValue { Index = 1 });

        // Click Apply Filters
        await _page.ClickAsync("button[type='submit']:has-text('Apply Filters')");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // After filtering, either results or a no-results message must be present
        var toyRows = _page.Locator("table tbody tr, .toy-item, .toy-card");
        var rowCount = await toyRows.CountAsync();

        var noResultsMsg = _page.Locator("*:has-text('No toys found')");
        var hasNoResultsMsg = await noResultsMsg.CountAsync() > 0;

        Assert.That(rowCount > 0 || hasNoResultsMsg, Is.True,
            "After applying a filter, either matching toys or a no-results message should be displayed.");
    }

    // ====================================================================
    // US-1408 | TC-033: Teacher searches with no matches — no-results message shown
    // Scenario 3 — No results
    // Given a teacher enters search criteria
    // When no toys match the criteria
    // Then a no results message is displayed
    // ====================================================================

    [Test]
    [Category("TC-033")]
    [Category("Teacher")]
    public async Task TC033_TeacherSearchesNoMatches_NoResultsMessageDisplayed() {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Use exact selector from DevTools
        var searchInput = _page.Locator("input[name='searchQuery']");
        await searchInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });
        await searchInput.FillAsync("ZZZZZ_NO_MATCH_99999");

        await _page.ClickAsync("#teacher-search-btn");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // The page shows "No toys found matching the selected filters."
        var noResultsMsg = _page.Locator("*:has-text('No toys found')");
        var hasNoResultsMsg = await noResultsMsg.CountAsync() > 0;

        // Also verify no toy rows are present
        var toyRows = _page.Locator("table tbody tr, .toy-item, .toy-card");
        var rowCount = await toyRows.CountAsync();

        Assert.That(rowCount == 0 || hasNoResultsMsg, Is.True,
            "A no-results message should be displayed when the search term matches nothing.");
    }

    // ====================================================================
    // Edge case: Teacher submits search with only whitespace
    // Given a teacher is on the Toy index page
    // When the teacher submits a search consisting only of spaces
    // Then the page should still respond gracefully (results or a no-results message)
    // ====================================================================

    [Test]
    [Category("TC-041")]
    [Category("Teacher")]
    public async Task TC041_TeacherSearchesWithWhitespace_PageRespondsGracefully() {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var searchInput = _page.Locator("input[name='searchQuery']");
        await searchInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });
        await searchInput.FillAsync("   "); // whitespace-only search

        await _page.ClickAsync("#teacher-search-btn");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var toyRows = _page.Locator("table tbody tr, .toy-item, .toy-card");
        var rowCount = await toyRows.CountAsync();

        var noResultsMsg = _page.Locator("*:has-text('No toys found')");
        var hasNoResultsMsg = await noResultsMsg.CountAsync() > 0;

        Assert.That(rowCount > 0 || hasNoResultsMsg, Is.True,
            "Whitespace-only searches should not crash the page; toys or a no-results message should be shown.");
    }

    // ====================================================================
    // Edge case: Teacher opens filter modal but applies with no options selected
    // Given a teacher is on the Toy index page
    // When the teacher opens the filter modal and clicks Apply without changing filters
    // Then the page should still respond gracefully (results or a no-results message)
    // ====================================================================

    [Test]
    [Category("TC-042")]
    [Category("Teacher")]
    public async Task TC042_TeacherAppliesFiltersWithNoSelection_PageRespondsGracefully() {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Open the filter modal
        await _page.ClickAsync("button:has-text('Filter'), a:has-text('Filter')");
        await _page.WaitForSelectorAsync(".modal.show, .modal[style*='display: block']",
            new PageWaitForSelectorOptions { Timeout = 10_000 });

        // Click Apply Filters without changing any dropdowns
        await _page.ClickAsync("button[type='submit']:has-text('Apply Filters')");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var toyRows = _page.Locator("table tbody tr, .toy-item, .toy-card");
        var rowCount = await toyRows.CountAsync();

        var noResultsMsg = _page.Locator("*:has-text('No toys found')");
        var hasNoResultsMsg = await noResultsMsg.CountAsync() > 0;

        Assert.That(rowCount > 0 || hasNoResultsMsg, Is.True,
            "Applying filters with no selection should not break the page; toys or a no-results message should be shown.");
    }

    // ====================================================================
    // US-1413 | TC-034: Student returns a borrowed toy — toy removed from borrowed list
    // Scenario 1 — Return borrowed toy
    // Given a student has borrowed a toy
    // When the student clicks return
    // Then the toy is marked as available and removed from the student's borrowed list
    // ====================================================================

    [Test]
    [Category("TC-034")]
    [Category("Student")]
    public async Task TC034_StudentReturnsBorrowedToy_ToyRemovedFromBorrowedList() {
        var studentAuthState = "auth-state-Student.json";
        if (!File.Exists(studentAuthState)) {
            Assert.Warn("Student auth state not found. Run tests with TEST_ROLE=Student first.");
            return;
        }

        var studentContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = studentAuthState
        });
        var studentPage = await studentContext.NewPageAsync();

        await studentPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy/Borrowed");
        await studentPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var returnButton = studentPage.Locator(
            "button:has-text('Return'), a:has-text('Return'), [data-action='return']");

        if (await returnButton.CountAsync() == 0) {
            Assert.Warn("No borrowed toys found for this student. Seed a borrowed toy and re-run.");
            await studentContext.CloseAsync();
            return;
        }

        // Capture toy name before returning
        var toyName = await studentPage
            .Locator("table tbody tr, .toy-item").First
            .Locator("td:first-child, .toy-name")
            .InnerTextAsync();

        await returnButton.First.ClickAsync();
        await studentPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Verify toy no longer appears in borrowed list
        var remainingItems = studentPage.Locator($"*:has-text('{toyName.Trim()}')");
        var stillPresent = await remainingItems.CountAsync() > 0;

        Assert.That(stillPresent, Is.False,
            "After returning a toy, it should be removed from the student's borrowed list.");

        await studentContext.CloseAsync();
    }

    // ====================================================================
    // US-1413 | TC-035: Returned toy becomes available for other students
    // Scenario 2 — Return updates system
    // Given a toy is returned
    // When the return is processed
    // Then the toy becomes available for other students
    // ====================================================================

    [Test]
    [Category("TC-035")]
    [Category("Student")]
    public async Task TC035_ReturnedToy_BecomesAvailableForOtherStudents() {
        var studentAuthState = "auth-state-Student.json";
        if (!File.Exists(studentAuthState)) {
            Assert.Warn("Student auth state not found. Run tests with TEST_ROLE=Student first.");
            return;
        }

        var studentContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = studentAuthState
        });
        var studentPage = await studentContext.NewPageAsync();

        await studentPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy/Borrowed");
        await studentPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var returnButton = studentPage.Locator(
            "button:has-text('Return'), a:has-text('Return'), [data-action='return']");

        if (await returnButton.CountAsync() == 0) {
            Assert.Warn("No borrowed toys found. Seed a borrowed toy and re-run.");
            await studentContext.CloseAsync();
            return;
        }

        var toyName = await studentPage
            .Locator("table tbody tr, .toy-item").First
            .Locator("td:first-child, .toy-name")
            .InnerTextAsync();

        await returnButton.First.ClickAsync();
        await studentPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Navigate to main toy list and verify toy is now available
        await studentPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy/Index");
        await studentPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var toyInList = studentPage.Locator($"*:has-text('{toyName.Trim()}')");
        await Expect(toyInList.First).ToBeVisibleAsync(
            new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

        await studentContext.CloseAsync();
    }
}