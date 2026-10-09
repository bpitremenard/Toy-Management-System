//using Microsoft.Playwright;
//using Microsoft.Playwright.NUnit;
//using NUnit.Framework;
//using Assert = NUnit.Framework.Assert;

//namespace TMS_TEST.E2E;

///// <summary>
///// Backup & Restore E2E tests
///// Test cases TC-030 to TC-034
///// Uses GlobalSetup for authentication and auth state reuse
///// </summary>
//[TestFixture]
//[Parallelizable(ParallelScope.Self)]
//[Category("Teacher")]
//public class BackupRestoreTests : BrowserTest
//{
//    private IBrowserContext _context = null!;
//    private IPage _page = null!;

//    [SetUp]
//    public async Task SetUpAuthenticatedContext()
//    {
//        _context = await Browser.NewContextAsync(new BrowserNewContextOptions
//        {
//            StorageStatePath = GlobalSetup.AuthStatePath
//        });
//        _page = await _context.NewPageAsync();
//    }

//    [TearDown]
//    public async Task TearDownContext()
//    {
//        await _context.CloseAsync();
//    }

//    // ====================================================================
//    // TC-030: Teacher initiates data backup – backup file is created
//    // ====================================================================

//    [Test]
//    [Category("TC-030")]
//    [Timeout(240000)]
//    public async Task TC030_TeacherInitiatesBackup_BackupFileIsCreated()
//    {
//        // Navigate to backup page
//        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Backup/Index");

//        // Wait for backup page to load
//        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

//        // Click the backup button
//        await _page.ClickAsync("#backupBtn");

//        // Wait for success status - element may be hidden, so check class
//        await _page.WaitForFunctionAsync(@"() => {
//            const statusEl = document.querySelector('#statusMessage');
//            return statusEl && statusEl.classList.contains('status-success');
//        }", new PageWaitForFunctionOptions { Timeout = 90_000 });

//        // Verify backup was added to the list
//        await _page.WaitForFunctionAsync(@"() => {
//            const select = document.querySelector('#backupList');
//            return select && select.options.length > 1;
//        }", new PageWaitForFunctionOptions { Timeout = 30_000 });

//        Assert.That(_page.Url, Does.Contain("/Backup"),
//            "After successful backup, user should remain on backup page.");
//    }

//    // ====================================================================
//    // TC-031: Teacher restores system from valid backup – data is restored correctly
//    // ====================================================================

//    [Test]
//    [Category("TC-031")]
//    [Timeout(240000)]
//    public async Task TC031_TeacherRestoresFromValidBackup_DataRestoredCorrectly()
//    {
//        // Navigate to backup page
//        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Backup/Index");

//        // Wait for backup page to load
//        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

//        // Wait for backup list to be populated
//        await _page.WaitForFunctionAsync(@"() => {
//            const select = document.querySelector('#backupList');
//            return select && select.options.length > 1;
//        }", new PageWaitForFunctionOptions { Timeout = 30_000 });

//        // Select the first backup (skip the placeholder option)
//        await _page.SelectOptionAsync("#backupList", new SelectOptionValue { Index = 1 });

//        // Handle the confirmation dialog (browser's confirm dialog) - register BEFORE clicking
//        _page.Dialog += async (_, dialog) =>
//        {
//            await dialog.AcceptAsync();
//        };

//        // Click the restore button
//        await _page.ClickAsync("#restoreBtn");

//        // Wait for success status
//        await _page.WaitForFunctionAsync(@"() => {
//            const statusEl = document.querySelector('#statusMessage');
//            return statusEl && statusEl.classList.contains('status-success');
//        }", new PageWaitForFunctionOptions { Timeout = 90_000 });

//        // Navigate to a data page to verify data was restored
//        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
//        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

//        // Verify data is present (at least one item should be visible)
//        var dataItems = _page.Locator("table tbody tr, .data-item, .toy-item");
//        await Expect(dataItems.First).ToBeVisibleAsync();
//    }

//    // ====================================================================
//    // TC-032: Attempt to restore using an invalid/corrupted file – error displayed
//    // ====================================================================

//    [Test]
//    [Category("TC-032")]
//    public async Task TC032_InvalidBackupFile_ErrorDisplayed()
//    {
//        // Navigate to backup page
//        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Backup/Index");

//        // Wait for backup page to load
//        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

//        // Add an invalid option to the select dropdown via JavaScript
//        await _page.EvaluateAsync(@"() => {
//            const select = document.querySelector('#backupList');
//            const option = document.createElement('option');
//            option.value = 'invalid_backup_corrupted.json';
//            option.text = 'invalid_backup_corrupted.json';
//            select.add(option);
//        }");

//        // Select the invalid backup
//        await _page.SelectOptionAsync("#backupList", "invalid_backup_corrupted.json");

//        // Handle the confirmation dialog (browser's confirm dialog) - register BEFORE clicking
//        _page.Dialog += async (_, dialog) =>
//        {
//            await dialog.AcceptAsync();
//        };

//        // Click the restore button
//        await _page.ClickAsync("#restoreBtn");

//        // Wait for error status
//        await _page.WaitForFunctionAsync(@"() => {
//            const statusEl = document.querySelector('#statusMessage');
//            return statusEl && statusEl.classList.contains('status-error');
//        }", new PageWaitForFunctionOptions { Timeout = 30_000 });
//    }

//    // ====================================================================
//    // TC-033: Student attempts to access backup/restore feature — access denied
//    // ====================================================================

//    [Test]
//    [Category("TC-033")]
//    public async Task TC033_StudentAccessesBackupRestore_AccessDenied()
//    {
//        // Create a fresh context with Student auth state
//        // Note: This requires running tests with TEST_ROLE=Student
//        var studentAuthState = "auth-state-Student.json";
//        if (!File.Exists(studentAuthState))
//        {
//            Assert.Warn("Student auth state not found. Run tests with TEST_ROLE=Student first.");
//            return;
//        }

//        var studentContext = await Browser.NewContextAsync(new BrowserNewContextOptions
//        {
//            StorageStatePath = studentAuthState
//        });
//        var studentPage = await studentContext.NewPageAsync();

//        // Try to access backup page as student
//        await studentPage.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Backup",
//            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

//        // Should be redirected to access denied or login
//        Assert.That(studentPage.Url, Does.Contain("AccessDenied").Or.Contain("login.microsoftonline.com").Or.Contains("/Home/Login"),
//            "Student accessing backup/restore should be denied access.");

//        await studentContext.CloseAsync();
//    }

//    // ====================================================================
//    // TC-034: Perform backup when system has no data — backup completes successfully
//    // ====================================================================

//    [Test]
//    [Category("TC-034")]
//    [Timeout(240000)]
//    public async Task TC034_BackupWithNoData_BackupCompletesSuccessfully()
//    {
//        // Navigate to backup page
//        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Backup/Index");

//        // Wait for backup page to load
//        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

//        // Click the backup button
//        await _page.ClickAsync("#backupBtn");

//        // Wait for success status
//        await _page.WaitForFunctionAsync(@"() => {
//            const statusEl = document.querySelector('#statusMessage');
//            return statusEl && statusEl.classList.contains('status-success');
//        }", new PageWaitForFunctionOptions { Timeout = 90_000 });

//        // Verify backup was created even with no data
//        await _page.WaitForFunctionAsync(@"() => {
//            const select = document.querySelector('#backupList');
//            return select && select.options.length > 1;
//        }", new PageWaitForFunctionOptions { Timeout = 30_000 });
//    }
//}
