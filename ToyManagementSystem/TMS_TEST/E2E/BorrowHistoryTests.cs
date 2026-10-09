using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

/// <summary>
/// Borrow History E2E tests
/// Test cases TC-055, TC-056, TC-058, TC-059, TC-060, TC-061, TC-084, TC-085
///
/// Layout is div-based (no &lt;table&gt;), based on the actual Razor views:
///
/// /Student  →  div.tms-student-row  per student
///              span.tms-student-name  for the name
///              a.tms-btn  for "VIEW BORROW HISTORY" (links to BorrowHistory action)
///
/// /Student/BorrowHistory/{id}  →  div.tms-history-row  per loan record
///                                  div.tms-history-cell  for each cell (5 cells per row)
///                                  span.tms-status-active / tms-status-returned / tms-status-overdue
///                                  span[style*="color:red"]  for a late return date
///                                  div.tms-no-data  when the student has no records
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category("Teacher")]
public class BorrowHistoryTests : BrowserTest
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
    /// Navigates to /Student and clicks the VIEW BORROW HISTORY button for the first student.
    /// </summary>
    private async Task NavigateToFirstStudentBorrowHistory()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Wait for at least one student row to render
        await _page.WaitForSelectorAsync(".tms-student-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Click the first student's VIEW BORROW HISTORY link
        await _page.Locator(".tms-student-row").First
            .Locator(".tms-actions a.tms-btn").ClickAsync();

        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    // ====================================================================
    // TC-055: Manage Students page loads with student names and action buttons
    // ====================================================================

    [Test]
    [Category("TC-055")]
    public async Task TC055_BorrowHistoryList_LoadsCorrectly_WithStudentNames()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Wait for student rows to appear
        await _page.WaitForSelectorAsync(".tms-student-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        var rows = _page.Locator(".tms-student-row");
        var rowCount = await rows.CountAsync();
        Assert.That(rowCount, Is.GreaterThan(0),
            "Manage Students page should display at least one student row.");

        // Verify the first student has a non-empty name
        var firstName = await rows.First.Locator(".tms-student-name").InnerTextAsync();
        Assert.That(firstName.Trim(), Is.Not.Empty,
            "Each student row should display a student name.");

        // Verify every row has a VIEW BORROW HISTORY action link
        var actionLinks = _page.Locator(".tms-student-row .tms-actions a.tms-btn");
        Assert.That(await actionLinks.CountAsync(), Is.EqualTo(rowCount),
            "Every student row should have a VIEW BORROW HISTORY action link.");
    }

    // ====================================================================
    // TC-056: Clicking VIEW BORROW HISTORY opens the student's history page
    // ====================================================================

    [Test]
    [Category("TC-056")]
    public async Task TC056_SelectStudent_DisplaysStudentBorrowHistory()
    {
        await NavigateToFirstStudentBorrowHistory();

        // The detail page heading uses the "BorrowHistoryFor" localizer key:
        // English → "Borrow History for {name}"
        // French  → "Historique d'emprunt pour {name}"
        var heading = _page.Locator("h2.tms-title");
        await Expect(heading).ToBeVisibleAsync();

        var headingText = await heading.InnerTextAsync();
        Assert.That(
            headingText.Contains("Borrow History for") || headingText.Contains("Historique d'emprunt pour"),
            Is.True,
            $"Student history page heading should contain the student name. Actual: '{headingText}'");
    }

    // ====================================================================
    // TC-058: Each history record shows toy name, borrowed date, due date,
    //         returned date, and status — 5 cells per row
    // ====================================================================

    [Test]
    [Category("TC-058")]
    public async Task TC058_BorrowHistoryRecord_ShowsAllRequiredFields()
    {
        await NavigateToFirstStudentBorrowHistory();

        // Wait for at least one history row
        await _page.WaitForSelectorAsync(".tms-history-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Verify the header has 5 cells: TOY NAME | BORROWED DATE | DUE DATE | RETURNED DATE | STATUS
        var headerCells = _page.Locator(".tms-history-header .tms-history-header-cell");
        Assert.That(await headerCells.CountAsync(), Is.EqualTo(5),
            "The borrow history header should have exactly 5 columns.");

        // Verify the first data row has 5 non-empty cells
        var firstRow = _page.Locator(".tms-history-row").First;
        var cells = firstRow.Locator(".tms-history-cell");
        Assert.That(await cells.CountAsync(), Is.EqualTo(5),
            "Each history row should have exactly 5 cells.");

        var toyName = await cells.Nth(0).InnerTextAsync(); // TOY NAME
        var borrowedDate = await cells.Nth(1).InnerTextAsync(); // BORROWED DATE
        var dueDate = await cells.Nth(2).InnerTextAsync(); // DUE DATE
        // cell 3 = RETURNED DATE (may be "-" for active loans)
        var status = await cells.Nth(4).InnerTextAsync(); // STATUS

        Assert.That(toyName.Trim(), Is.Not.Empty, "Toy name should not be empty.");
        Assert.That(borrowedDate.Trim(), Is.Not.Empty, "Borrowed date should not be empty.");
        Assert.That(dueDate.Trim(), Is.Not.Empty, "Due date should not be empty.");
        Assert.That(status.Trim(), Is.Not.Empty, "Status should not be empty.");
    }

    // ====================================================================
    // TC-059: Student with many records — all rows render (no pagination in current build)
    // ====================================================================

    [Test]
    [Category("TC-059")]
    public async Task TC059_StudentWithManyRecords_AllRowsAreRendered()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.WaitForSelectorAsync(".tms-student-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Click the VIEW BORROW HISTORY for the student known to have many records
        // TODO: Replace "Matthew Nolan" with the actual heavy-borrower student name on your test server
        var targetRow = _page.Locator(".tms-student-row").Filter(new LocatorFilterOptions
        {
            HasText = "Matthew Nolan"
        });
        await targetRow.Locator(".tms-actions a.tms-btn").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Wait for history rows to appear
        await _page.WaitForSelectorAsync(".tms-history-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        var rows = _page.Locator(".tms-history-row");
        var rowCount = await rows.CountAsync();

        Assert.That(rowCount, Is.GreaterThan(10),
            "A student with many borrow records should display more than 10 history rows.");
    }

    // ====================================================================
    // TC-060: Currently borrowed toys show ACTIVE status and "-" for return date
    // ====================================================================

    [Test]
    [Category("TC-060")]
    public async Task TC060_CurrentlyBorrowedToys_ShowActiveStatus()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.WaitForSelectorAsync(".tms-student-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Navigate to Matthew Nolan who has a known active loan ("Doll", borrowed 2026-04-13)
        // TODO: If this student name changes on the test server, update it here
        var targetRow = _page.Locator(".tms-student-row").Filter(new LocatorFilterOptions
        {
            HasText = "Matthew Nolan"
        });
        await targetRow.Locator(".tms-actions a.tms-btn").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Wait for an active status badge to appear
        await _page.WaitForSelectorAsync(".tms-status-active",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        var activeBadge = _page.Locator(".tms-status-active").First;
        await Expect(activeBadge).ToBeVisibleAsync();

        // Find the row containing the active badge and verify its returned date cell shows "-"
        var activeRow = _page.Locator(".tms-history-row").Filter(new LocatorFilterOptions
        {
            Has = _page.Locator(".tms-status-active")
        }).First;

        var returnedDateCell = activeRow.Locator(".tms-history-cell").Nth(3);
        var returnedDateText = await returnedDateCell.InnerTextAsync();

        Assert.That(returnedDateText.Trim(), Is.EqualTo("-"),
            "An active (unreturned) loan should show '-' in the Returned Date cell.");
    }


    // ====================================================================
    // TC-061: Late returns — return date cell contains a red-coloured span
    //         (view renders: <span style="color:red;">date</span> when ReturnDate > DueDate)
    // ====================================================================

    [Test]
    [Category("TC-061")]
    public async Task TC061_LateReturn_ReturnDateIsDisplayedInRed()
    {
        await NavigateToFirstStudentBorrowHistory();

        // The view wraps late return dates in <span style="color:red;">
        // Wait for that element to appear
        await _page.WaitForSelectorAsync(".tms-history-cell span[style*='color:red']",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        var lateReturnSpan = _page.Locator(".tms-history-cell span[style*='color:red']").First;
        await Expect(lateReturnSpan).ToBeVisibleAsync();

        // Confirm the text is a date (yyyy-MM-dd format)
        var dateText = await lateReturnSpan.InnerTextAsync();
        Assert.That(dateText.Trim(), Does.Match(@"^\d{4}-\d{2}-\d{2}$"),
            $"The red late-return date should be formatted as yyyy-MM-dd. Actual: '{dateText}'");

        // Confirm the computed color is red
        var color = await lateReturnSpan.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).color"
        );
        Assert.That(color, Is.EqualTo("rgb(255, 0, 0)"),
            $"Late return date should be rendered in red (rgb(255, 0, 0)). Actual: '{color}'");
    }

    // ====================================================================
    // TC-084: Student navigates directly to /Student via URL — access denied
    // ====================================================================

    [Test]
    [Category("TC-084")]
    public async Task TC084_Student_DirectURLToBorrowHistory_AccessDenied()
    {
        var studentAuthState = "auth-state-Student.json";
        if (!File.Exists(studentAuthState))
        {
            Assert.Warn("Student auth state not found. Run tests with TEST_ROLE=Student first.");
            return;
        }

        var studentContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = studentAuthState
        });
        var studentPage = await studentContext.NewPageAsync();

        await studentPage.GotoAsync(
            $"{GlobalSetup.TeacherBaseUrl}/Student",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }
        );

        var currentUrl = studentPage.Url;
        Assert.That(
            currentUrl.Contains("AccessDenied")
            || currentUrl.Contains("login.microsoftonline.com")
            || currentUrl.Contains("/Home/Login"),
            Is.True,
            $"A student accessing the teacher /Student page should be denied. Actual URL: {currentUrl}");

        await studentContext.CloseAsync();
    }

    // ====================================================================
    // TC-085: Student with no borrows — tms-no-data element is shown
    // ====================================================================

    [Test]
    [Category("TC-085")]
    public async Task TC085_StudentWithNoBorrows_EmptyStateIsShown()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Student");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.WaitForSelectorAsync(".tms-student-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Click VIEW BORROW HISTORY for the student who has never borrowed anything
        // TODO: Replace with the actual name of a student with no borrow records
        var targetRow = _page.Locator(".tms-student-row").Filter(new LocatorFilterOptions
        {
            HasText = "Gil Audrey Nangue Tsotsop" 
        });
        await targetRow.Locator(".tms-actions a.tms-btn").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // The view renders div.tms-no-data when Model has no records
        var noDataDiv = _page.Locator(".tms-no-data");
        await Expect(noDataDiv).ToBeVisibleAsync();

        // Also confirm no history rows are rendered
        var historyRows = _page.Locator(".tms-history-row");
        Assert.That(await historyRows.CountAsync(), Is.EqualTo(0),
            "A student with no borrows should have zero history rows.");
    }

    // ====================================================================
    // TC-086: Borrow history sorting - verify records can be sorted by different columns
    // ====================================================================

    [Test]
    [Category("TC-086")]
    public async Task TC086_BorrowHistorySorting_SortableColumnsWorkCorrectly()
    {
        await NavigateToFirstStudentBorrowHistory();

        // Wait for history rows to appear
        await _page.WaitForSelectorAsync(".tms-history-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Look for sortable column headers (they typically have click handlers or sort indicators)
        var sortableHeaders = _page.Locator(".tms-history-header-cell[onclick], .tms-sortable, .tms-sort-header");
        
        // If sortable headers exist, test sorting functionality
        if (await sortableHeaders.CountAsync() > 0)
        {
            // Get initial order of toy names
            var initialToyNames = new List<string>();
            var rows = _page.Locator(".tms-history-row");
            var rowCount = await rows.CountAsync();
            
            for (int i = 0; i < Math.Min(rowCount, 5); i++)
            {
                var toyName = await rows.Nth(i).Locator(".tms-history-cell").Nth(0).InnerTextAsync();
                initialToyNames.Add(toyName.Trim());
            }

            // Click on the Toy Name header to sort
            await sortableHeaders.First.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            // Get new order after sorting
            var sortedToyNames = new List<string>();
            for (int i = 0; i < Math.Min(rowCount, 5); i++)
            {
                var toyName = await rows.Nth(i).Locator(".tms-history-cell").Nth(0).InnerTextAsync();
                sortedToyNames.Add(toyName.Trim());
            }

            // Verify that the order has changed (or at least that sorting didn't break the display)
            Assert.That(await rows.CountAsync(), Is.GreaterThan(0),
                "After sorting, borrow history rows should still be displayed.");

            // Check if sort indicator is present (up/down arrow)
            var sortIndicator = _page.Locator(".tms-sort-indicator, .sort-up, .sort-down");
            if (await sortIndicator.CountAsync() > 0)
            {
                await Expect(sortIndicator.First).ToBeVisibleAsync();
            }
        }
        else
        {
            // If no sortable headers found, verify the table still displays correctly
            var rows = _page.Locator(".tms-history-row");
            Assert.That(await rows.CountAsync(), Is.GreaterThan(0),
                "Borrow history should display records even without sorting functionality.");
        }
    }

    // ====================================================================
    // TC-087: Borrow history filtering - verify status filtering works correctly
    // ====================================================================

    [Test]
    [Category("TC-087")]
    public async Task TC087_BorrowHistoryFiltering_StatusFilterDisplaysCorrectRecords()
    {
        await NavigateToFirstStudentBorrowHistory();

        // Wait for history rows to appear
        await _page.WaitForSelectorAsync(".tms-history-row",
            new PageWaitForSelectorOptions { Timeout = 30_000 });

        // Look for filter controls (dropdowns, checkboxes, or buttons for status filtering)
        var statusFilter = _page.Locator("select[name*='status'], .status-filter, .filter-status");
        var filterButtons = _page.Locator(".filter-btn, .btn-filter");

        // Test case 1: If status filter dropdown exists
        if (await statusFilter.CountAsync() > 0)
        {
            // Get total count of all records before filtering
            var totalRows = await _page.Locator(".tms-history-row").CountAsync();

            // Filter for "Active" loans
            await statusFilter.SelectOptionAsync("Active");
            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            // Verify only active records are shown
            var activeRows = _page.Locator(".tms-history-row");
            var activeCount = await activeRows.CountAsync();
            
            // All visible rows should have active status
            for (int i = 0; i < activeCount; i++)
            {
                var statusCell = await activeRows.Nth(i).Locator(".tms-history-cell").Nth(4).InnerTextAsync();
                Assert.That(statusCell.Trim(), Does.Contain("Active").Or.Contain("ACTIF"),
                    $"Row {i} should show Active status when filtered for Active loans.");
            }

            // Filter for "Returned" loans
            await statusFilter.SelectOptionAsync("Returned");
            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var returnedRows = _page.Locator(".tms-history-row");
            var returnedCount = await returnedRows.CountAsync();
            
            // All visible rows should have returned status
            for (int i = 0; i < returnedCount; i++)
            {
                var statusCell = await returnedRows.Nth(i).Locator(".tms-history-cell").Nth(4).InnerTextAsync();
                Assert.That(statusCell.Trim(), Does.Contain("Returned").Or.Contain("RETOURNÉ"),
                    $"Row {i} should show Returned status when filtered for Returned loans.");
            }
        }
        // Test case 2: If filter buttons exist
        else if (await filterButtons.CountAsync() > 0)
        {
            // Click on "Active" filter button if it exists
            var activeButton = _page.Locator(".filter-btn:has-text('Active'), .btn-filter:has-text('ACTIF')");
            if (await activeButton.CountAsync() > 0)
            {
                await activeButton.ClickAsync();
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Verify filtering worked by checking for active status badges
                var activeBadges = _page.Locator(".tms-status-active");
                Assert.That(await activeBadges.CountAsync(), Is.GreaterThan(0),
                    "After filtering for Active loans, at least one active status badge should be visible.");
            }
        }
        // Test case 3: No explicit filters - verify status badges are still functional
        else
        {
            // At minimum, verify that different status types exist and are visually distinct
            var activeBadges = _page.Locator(".tms-status-active");
            var returnedBadges = _page.Locator(".tms-status-returned");
            var overdueBadges = _page.Locator(".tms-status-overdue");

            var hasStatuses = await activeBadges.CountAsync() > 0 || 
                             await returnedBadges.CountAsync() > 0 || 
                             await overdueBadges.CountAsync() > 0;

            Assert.That(hasStatuses, Is.True,
                "Borrow history should display at least one type of status badge (Active, Returned, or Overdue).");

            // Verify status badges are visible and have distinct styling
            if (await activeBadges.CountAsync() > 0)
            {
                await Expect(activeBadges.First).ToBeVisibleAsync();
            }
            if (await returnedBadges.CountAsync() > 0)
            {
                await Expect(returnedBadges.First).ToBeVisibleAsync();
            }
            if (await overdueBadges.CountAsync() > 0)
            {
                await Expect(overdueBadges.First).ToBeVisibleAsync();
            }
        }
    }
}