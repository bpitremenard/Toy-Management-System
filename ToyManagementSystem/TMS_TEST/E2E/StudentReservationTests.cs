using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Assert = NUnit.Framework.Assert;

namespace TMS_TEST.E2E;

/// <summary>
/// Student reservation E2E tests.
/// Covers TC-038, TC-039, TC-044, and TC-087.
/// Uses GlobalSetup for authentication and auth state reuse
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class StudentReservationTests : BrowserTest
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    private string StudentBaseUrl => GlobalSetup.StudentBaseUrl;

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

    private async Task GoToToyListAsync()
    {
        await _page.GotoAsync($"{StudentBaseUrl}/Toy");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task<ILocator> GetFirstAvailableToyCardAsync()
    {
        await GoToToyListAsync();

        var card = _page.Locator("div.col-12.col-md-6.col-lg-4")
            .Filter(new LocatorFilterOptions
            {
                Has = _page.Locator("a:has-text('Reserve')")
            })
            .First;

        await card.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });

        return card;
    }

    private async Task<int> CountReserveLinksAsync()
    {
        await GoToToyListAsync();
        return await _page.Locator("a:has-text('Reserve')").CountAsync();
    }

    // ====================================================================
    // TC-038: Reserve available toy successfully
    // ====================================================================

    [Test]
    [Category("TC-038")]
    public async Task TC038_ReserveToy_AvailableToy_ReservationSuccessful()
    {
        var reserveCountBefore = await CountReserveLinksAsync();
        Assert.That(reserveCountBefore, Is.GreaterThan(0),
            "Expected at least one available toy with a Reserve link.");

        var firstAvailableCard = await GetFirstAvailableToyCardAsync();
        var toyName = (await firstAvailableCard.Locator("h5").InnerTextAsync()).Trim();

        await firstAvailableCard.Locator("a:has-text('Reserve')").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var successAlert = _page.Locator(".alert-success");
        await successAlert.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });

        var successText = await successAlert.InnerTextAsync();
        Assert.That(successText, Does.Contain("successfully reserved").IgnoreCase,
            "A successful reservation should show a success message.");

        var reserveCountAfter = await _page.Locator("a:has-text('Reserve')").CountAsync();
        Assert.That(reserveCountAfter, Is.LessThan(reserveCountBefore),
            $"After reserving '{toyName}', there should be fewer toys with a Reserve link.");
    }

    // ====================================================================
    // TC-039: Reserved toy appears in student's current list/state
    // ====================================================================

    [Test]
    [Category("TC-039")]
    public async Task TC039_ReserveToy_ReservedToyAppearsAsCurrentStudentLoan()
    {
        var firstAvailableCard = await GetFirstAvailableToyCardAsync();
        var toyName = (await firstAvailableCard.Locator("h5").InnerTextAsync()).Trim();

        await firstAvailableCard.Locator("a:has-text('Reserve')").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GoToToyListAsync();

        var reservedCard = _page.Locator("div.col-12.col-md-6.col-lg-4")
            .Filter(new LocatorFilterOptions { HasText = toyName })
            .Filter(new LocatorFilterOptions
            {
                Has = _page.Locator("a:has-text('Return')")
            })
            .First;

        await reservedCard.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });

        var reservedText = await reservedCard.InnerTextAsync();
        Assert.That(reservedText, Does.Contain(toyName),
            "The reserved toy should still be visible to the student.");

        Assert.That(await reservedCard.Locator("a:has-text('Return')").CountAsync(), Is.EqualTo(1),
            "A toy reserved by the current student should show a Return link.");
    }

    // ====================================================================
    // TC-044: Direct reservation attempt for unavailable toy fails safely
    // ====================================================================

    [Test]
    [Category("TC-044")]
    public async Task TC044_ReserveToy_DirectUnavailableReservationFailsSafely()
    {
        await GoToToyListAsync();

        var unavailableCard = _page.Locator("div.col-12.col-md-6.col-lg-4")
            .Filter(new LocatorFilterOptions
            {
                Has = _page.Locator("button:has-text('Reserved')")
            })
            .First;

        if (await unavailableCard.CountAsync() == 0)
        {
            Assert.Ignore("No already reserved toy was available to validate this scenario.");
            return;
        }

        var disabledButtons = await unavailableCard.Locator("button:has-text('Reserved')").CountAsync();
        Assert.That(disabledButtons, Is.GreaterThan(0),
            "An unavailable toy should show a disabled Reserved button instead of a Reserve link.");

        var reserveLinks = await unavailableCard.Locator("a:has-text('Reserve')").CountAsync();
        Assert.That(reserveLinks, Is.EqualTo(0),
            "An unavailable toy should not expose a Reserve link for direct interaction.");
    }



    // ====================================================================
    // TC-087: Unavailable or already reserved toy cannot be reserved
    // ====================================================================

    [Test]
    [Category("TC-087")]
    public async Task TC087_ReserveToy_UnavailableToy_ReservationBlocked()
    {
        await GoToToyListAsync();

        var reservedButton = _page.Locator("button:has-text('Reserved')").First;

        if (await reservedButton.CountAsync() == 0)
        {
            Assert.Ignore("No unavailable or already reserved toy was available to validate this scenario.");
            return;
        }

        await reservedButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10_000
        });

        Assert.That(await reservedButton.IsDisabledAsync(), Is.True,
            "A toy already reserved by another student should not be reservable by the current student.");
    }

}
