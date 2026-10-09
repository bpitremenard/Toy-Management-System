using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

/// <summary>
/// Theme toggle and filtering E2E tests
/// Joshua-owned test cases:
/// TC-028, TC-029, TC-030, TC-086
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ThemeAndFilterTests : BrowserTest
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
    // TC-028: Enable Dark Mode - Teacher
    // ====================================================================

    [Test]
    [Category("TC-028")]
    [Category("Teacher")]
    public async Task TC028_TeacherDarkMode_TogglesBetweenDarkAndLightThemes()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.EvaluateAsync(@"() => {
            document.body.classList.remove('dark-mode');
            window.localStorage.setItem('theme', 'light');
        }");

        await Expect(_page.Locator(".theme-toggle")).ToBeVisibleAsync();

        var isDarkMode = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedTheme = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkMode, Is.False);
        Assert.That(storedTheme, Is.EqualTo("light"));

        await _page.ClickAsync(".theme-toggle");

        await _page.WaitForFunctionAsync(@"() => {
            return document.body.classList.contains('dark-mode') &&
                   window.localStorage.getItem('theme') === 'dark';
        }");

        isDarkMode = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        storedTheme = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkMode, Is.True);
        Assert.That(storedTheme, Is.EqualTo("dark"));

        await _page.WaitForTimeoutAsync(500);
        await _page.ClickAsync(".theme-toggle");

        await _page.WaitForFunctionAsync(@"() => {
            return !document.body.classList.contains('dark-mode') &&
                   window.localStorage.getItem('theme') === 'light';
        }");

        isDarkMode = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        storedTheme = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkMode, Is.False);
        Assert.That(storedTheme, Is.EqualTo("light"));
    }

    // ====================================================================
    // TC-029: Enable Dark Mode - Student
    // ====================================================================

    [Test]
    [Category("TC-029")]
    [Category("Student")]
    public async Task TC029_StudentDarkMode_TogglesBetweenDarkAndLightThemes()
    {
        await _page.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.EvaluateAsync(@"() => {
            document.body.classList.remove('dark-mode');
            window.localStorage.setItem('theme', 'light');
        }");

        await Expect(_page.Locator(".theme-toggle")).ToBeVisibleAsync();

        var isDarkMode = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedTheme = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkMode, Is.False);
        Assert.That(storedTheme, Is.EqualTo("light"));

        await _page.ClickAsync(".theme-toggle");

        await _page.WaitForFunctionAsync(@"() => {
            return document.body.classList.contains('dark-mode') &&
                   window.localStorage.getItem('theme') === 'dark';
        }");

        isDarkMode = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        storedTheme = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkMode, Is.True);
        Assert.That(storedTheme, Is.EqualTo("dark"));

        await _page.WaitForTimeoutAsync(500);
        await _page.ClickAsync(".theme-toggle");

        await _page.WaitForFunctionAsync(@"() => {
            return !document.body.classList.contains('dark-mode') &&
                   window.localStorage.getItem('theme') === 'light';
        }");

        isDarkMode = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        storedTheme = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkMode, Is.False);
        Assert.That(storedTheme, Is.EqualTo("light"));
    }

    // ====================================================================
    // TC-030: Filter Toy List - Student
    // ====================================================================

    [Test]
    [Category("TC-030")]
    [Category("Student")]
    public async Task TC030_StudentFilterToyList_AppliesAndClearsSelectedFilters()
    {
        await _page.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var allCards = _page.Locator(".toy-card");
        var initialCardCount = await allCards.CountAsync();
        Assert.That(initialCardCount, Is.GreaterThan(0),
            "Student toy list should contain toys before filtering.");

        await _page.ClickAsync("[data-bs-target='#filterModal']");
        await _page.WaitForSelectorAsync("#filterModal.show");

        await _page.SelectOptionAsync("#developmentGoalFilter", "Cognitive");
        await _page.ClickAsync("#filterModal button[type='submit']");

        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _page.WaitForURLAsync(
            url => url.Contains("categories=Cognitive"),
            new PageWaitForURLOptions { Timeout = 10_000 });

        var filteredCards = _page.Locator(".toy-card");
        await Expect(filteredCards.First).ToBeVisibleAsync();

        var filteredCardCount = await filteredCards.CountAsync();
        Assert.That(filteredCardCount, Is.GreaterThan(0),
            "Expected at least one toy after applying a valid filter.");

        for (var i = 0; i < filteredCardCount; i++)
        {
            var cardText = await filteredCards.Nth(i).InnerTextAsync();
            Assert.That(cardText, Does.Contain("Cognitive"));
        }

        await _page.ClickAsync("a.clear-btn");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        Assert.That(_page.Url, Does.Not.Contain("categories=").And.Not.Contain("materials="),
            "Clearing filters should return the student toy list to its unfiltered state.");

        var clearedCardCount = await _page.Locator(".toy-card").CountAsync();
        Assert.That(clearedCardCount, Is.GreaterThanOrEqualTo(filteredCardCount),
            "Clearing filters should not leave the list in the narrowed filtered state.");
    }

    // ====================================================================
    // TC-086: Filter Toy List, Non Existing Toy - Student
    // ====================================================================

    [Test]
    [Category("TC-086")]
    [Category("Student")]
    public async Task TC086_StudentFilterToyList_NoMatchingToy_ShowsEmptyStateAndCanBeCleared()
    {
        await _page.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.ClickAsync("[data-bs-target='#filterModal']");
        await _page.WaitForSelectorAsync("#filterModal.show");

        await _page.EvaluateAsync(@"() => {
            const categorySelect = document.querySelector('#developmentGoalFilter');
            if (categorySelect && !categorySelect.querySelector(""option[value='NoMatchingCategory']"")) {
                const option = document.createElement('option');
                option.value = 'NoMatchingCategory';
                option.text = 'NoMatchingCategory';
                categorySelect.add(option);
            }
        }");

        await _page.SelectOptionAsync("#developmentGoalFilter", "NoMatchingCategory");
        await _page.ClickAsync("#filterModal button[type='submit']");

        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _page.WaitForURLAsync(
            url => url.Contains("categories=NoMatchingCategory"),
            new PageWaitForURLOptions { Timeout = 10_000 });

        await Expect(_page.Locator(".alert-info")).ToBeVisibleAsync();
        Assert.That(await _page.Locator(".toy-card").CountAsync(), Is.EqualTo(0),
            "A non-matching filter should leave the student toy list empty.");

        await _page.ClickAsync("a.clear-btn");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        Assert.That(_page.Url, Does.Not.Contain("NoMatchingCategory"));
        Assert.That(await _page.Locator(".toy-card").CountAsync(), Is.GreaterThan(0),
            "Clearing the non-matching filter should restore the available toy list.");
    }

    // ====================================================================
    // TC-087: Theme Persistence Across Navigation - Teacher
    // ====================================================================

    [Test]
    [Category("TC-087")]
    [Category("Teacher")]
    public async Task TC087_TeacherThemePersistence_MaintainsThemeAcrossPageNavigation()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.EvaluateAsync(@"() => {
            document.body.classList.remove('dark-mode');
            window.localStorage.setItem('theme', 'light');
        }");

        await Expect(_page.Locator(".theme-toggle")).ToBeVisibleAsync();
        await _page.ClickAsync(".theme-toggle");

        await _page.WaitForFunctionAsync(@"() => {
            return document.body.classList.contains('dark-mode') &&
                   window.localStorage.getItem('theme') === 'dark';
        }");

        var isDarkModeAfterToggle = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedThemeAfterToggle = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkModeAfterToggle, Is.True);
        Assert.That(storedThemeAfterToggle, Is.EqualTo("dark"));

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Home/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var isDarkModeOnHome = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedThemeOnHome = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkModeOnHome, Is.True, "Dark mode should persist when navigating to Home page");
        Assert.That(storedThemeOnHome, Is.EqualTo("dark"), "Theme should remain 'dark' in localStorage");

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var isDarkModeBackToToy = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedThemeBackToToy = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkModeBackToToy, Is.True, "Dark mode should persist when returning to Toy page");
        Assert.That(storedThemeBackToToy, Is.EqualTo("dark"), "Theme should remain 'dark' in localStorage");
    }

    // ====================================================================
    // TC-088: Theme Persistence Across Navigation - Student
    // ====================================================================

    [Test]
    [Category("TC-088")]
    [Category("Student")]
    public async Task TC088_StudentThemePersistence_MaintainsThemeAcrossPageNavigation()
    {
        await _page.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.EvaluateAsync(@"() => {
            document.body.classList.remove('dark-mode');
            window.localStorage.setItem('theme', 'light');
        }");

        await Expect(_page.Locator(".theme-toggle")).ToBeVisibleAsync();
        await _page.ClickAsync(".theme-toggle");

        await _page.WaitForFunctionAsync(@"() => {
            return document.body.classList.contains('dark-mode') &&
                   window.localStorage.getItem('theme') === 'dark';
        }");

        var isDarkModeAfterToggle = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedThemeAfterToggle = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkModeAfterToggle, Is.True);
        Assert.That(storedThemeAfterToggle, Is.EqualTo("dark"));

        await _page.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Home/Index");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var isDarkModeOnHome = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedThemeOnHome = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkModeOnHome, Is.True, "Dark mode should persist when navigating to Home page");
        Assert.That(storedThemeOnHome, Is.EqualTo("dark"), "Theme should remain 'dark' in localStorage");

        await _page.GotoAsync($"{GlobalSetup.StudentBaseUrl}/Toy");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var isDarkModeBackToToy = await _page.EvaluateAsync<bool>("() => document.body.classList.contains('dark-mode')");
        var storedThemeBackToToy = await _page.EvaluateAsync<string?>("() => window.localStorage.getItem('theme')");
        Assert.That(isDarkModeBackToToy, Is.True, "Dark mode should persist when returning to Toy page");
        Assert.That(storedThemeBackToToy, Is.EqualTo("dark"), "Theme should remain 'dark' in localStorage");
    }
}
