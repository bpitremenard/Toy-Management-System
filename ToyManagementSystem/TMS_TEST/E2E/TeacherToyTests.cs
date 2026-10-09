using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category("Teacher")]
public class TeacherToyTests : BrowserTest
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

    private async Task DeleteToyByNameAsync(string toyName)
    {
        var toyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName })
            .First;

        await toyCard.WaitForAsync();

        await toyCard.Locator("a:has-text('Delete')").ClickAsync();

        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await _page.Locator("form button:has-text('Delete')").ClickAsync();

        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var deletedCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName });

        Assert.That(await deletedCard.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    [Category("TC_Teacher_Create_Edit_Delete_Toy")]
    public async Task TC_Teacher_Create_Edit_Delete_Toy()
    {
        string toyName = $"Playwright Test Toy {DateTime.Now.Ticks}";
        string updatedToyName = $"{toyName} Updated";

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("#Name").FillAsync(toyName);
        await _page.Locator("#LocationCode").FillAsync("A1");

        await _page.RunAndWaitForNavigationAsync(async () =>
        {
            await _page.Locator("button[type='submit']").ClickAsync();
        });

        var toyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName })
            .First;

        await toyCard.WaitForAsync();

        await toyCard.Locator("a:has-text('Update')").ClickAsync();

        await _page.Locator("#Name").FillAsync(updatedToyName);

        await _page.RunAndWaitForNavigationAsync(async () =>
        {
            await _page.Locator("button[type='submit']").ClickAsync();
        });

        var updatedToyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = updatedToyName })
            .First;

        await updatedToyCard.WaitForAsync();

        await DeleteToyByNameAsync(updatedToyName);
    }

    [Test]
    [Category("TC_Teacher_Create_Unsuccessful")]
    public async Task TC_Create_Toy_Unsuccessful()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("button[type='submit']").ClickAsync();

        var errors = _page.Locator("span.text-danger");

        Assert.That(await errors.CountAsync(), Is.GreaterThan(0));
    }

    [Test]
    [Category("TC_Teacher_Edit_Unsuccessful")]
    public async Task TC_Edit_Toy_Unsuccessful()
    {
        string toyName = $"ToyForEditTest {DateTime.Now.Ticks}";

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("#Name").FillAsync(toyName);
        await _page.Locator("#LocationCode").FillAsync("A1");

        await _page.RunAndWaitForNavigationAsync(async () =>
        {
            await _page.Locator("button[type='submit']").ClickAsync();
        });

        var toyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName })
            .First;

        await toyCard.WaitForAsync();

        await toyCard.Locator("a:has-text('Update')").ClickAsync();

        await _page.Locator("#Name").FillAsync("");

        await _page.Locator("button[type='submit']").ClickAsync();

        var errors = _page.Locator("span.text-danger");

        Assert.That(await errors.CountAsync(), Is.GreaterThan(0));

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await DeleteToyByNameAsync(toyName);
    }

    [Test]
    [Category("TC_Teacher_Create_Cancel")]
    public async Task TC_Create_Toy_Cancel()
    {
        string toyName = $"ToyCancelTest {DateTime.Now.Ticks}";

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("#Name").FillAsync(toyName);
        await _page.Locator("#LocationCode").FillAsync("C1");

        await _page.Locator("a:has-text('Cancel')").ClickAsync();

        var toyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName });

        Assert.That(await toyCard.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    [Category("TC_Teacher_Edit_Cancel")]
    public async Task TC_Edit_Toy_Cancel()
    {
        string toyName = $"ToyEditCancelTest {DateTime.Now.Ticks}";

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("#Name").FillAsync(toyName);
        await _page.Locator("#LocationCode").FillAsync("D2");

        await _page.RunAndWaitForNavigationAsync(async () =>
        {
            await _page.Locator("button[type='submit']").ClickAsync();
        });

        var toyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName })
            .First;

        await toyCard.WaitForAsync();

        await toyCard.Locator("a:has-text('Update')").ClickAsync();

        await _page.Locator("#Name").FillAsync("Cancelled");

        await _page.Locator("a:has-text('Cancel')").ClickAsync();

        var originalCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName });

        Assert.That(await originalCard.CountAsync(), Is.GreaterThan(0));

        await DeleteToyByNameAsync(toyName);
    }

    [Test]
    [Category("TC_Teacher_Delete_Cancelled")]
    public async Task TC_Teacher_Delete_Cancelled()
    {
        string toyName = $"ToyDeleteTest {DateTime.Now.Ticks}";

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("#Name").FillAsync(toyName);
        await _page.Locator("#LocationCode").FillAsync("D2");

        await _page.RunAndWaitForNavigationAsync(async () =>
        {
            await _page.Locator("button[type='submit']").ClickAsync();
        });

        var toyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName })
            .First;

        await toyCard.WaitForAsync();

        await toyCard.Locator("a:has-text('Delete')").ClickAsync();

        await _page.Locator("a:has-text('Cancel')").ClickAsync();

        Assert.That(await toyCard.CountAsync(), Is.GreaterThan(0));

        await DeleteToyByNameAsync(toyName);
    }

    [Test]
    [Category("TC_Teacher_Create_All_Fields_Success")]
    public async Task TC_Teacher_Create_All_Fields_Success()
    {
        string toyName = $"AllFieldsToy {DateTime.Now.Ticks}";

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("#Name").FillAsync(toyName);
        await _page.Locator("#LocationCode").FillAsync("A5");

        await _page.SelectOptionAsync("#Material", "Wood");
        await _page.SelectOptionAsync("#Category", "Cognitive");

        await _page.Locator("#Description").FillAsync("Test description");
        await _page.Locator("#AdditionalInformation").FillAsync("Additional test info");

        await _page.RunAndWaitForNavigationAsync(async () =>
        {
            await _page.Locator("button[type='submit']").ClickAsync();
        });

        var toyCard = _page
            .Locator(".toy-card", new PageLocatorOptions { HasTextString = toyName })
            .First;

        await toyCard.WaitForAsync();

        await DeleteToyByNameAsync(toyName);
    }

    [Test]
    [Category("TC_Teacher_Create_Required_Field_Failures")]
    public async Task TC_Teacher_Create_Required_Field_Failures()
    {
        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Index");

        await _page.Locator("a.btn-secondary:has-text('Add Toy')").ClickAsync();

        await _page.Locator("#LocationCode").FillAsync("A1");

        await _page.Locator("button[type='submit']").ClickAsync();

        var errors = _page.Locator("span.text-danger");

        Assert.That(await errors.CountAsync(), Is.GreaterThan(0));

        await _page.GotoAsync($"{GlobalSetup.TeacherBaseUrl}/Toy/Create");

        await _page.Locator("#Name").FillAsync("MissingLocation");

        await _page.Locator("button[type='submit']").ClickAsync();

        errors = _page.Locator("span.text-danger");

        Assert.That(await errors.CountAsync(), Is.GreaterThan(0));
    }
}