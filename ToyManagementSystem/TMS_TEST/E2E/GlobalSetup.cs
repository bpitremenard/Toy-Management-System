using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework;
using System.Text;
using System.Text.Json;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]

namespace TMS_TEST.E2E;

/// <summary>
/// One-time global setup that authenticates with Azure AD and saves the browser
/// storage state to auth-state-{Role}.json so that all authenticated tests can reuse it.
///
/// Each role gets its own auth-state file:
///   auth-state-Teacher.json   → created when TEST_ROLE=Teacher
///   auth-state-Student.json → created when TEST_ROLE=Student
///
/// Smart reuse: if auth-state-{Role}.json already exists, GlobalSetup tries to
/// reuse it by loading the session and decoding the cached token to verify that
/// it still carries the expected role. If the role no longer matches (e.g. the
/// user switched roles in Azure AD), the stale file is deleted automatically
/// and a full re-authentication (MFA) is triggered — no manual deletion needed.
///
/// Supports two MFA flows controlled by the MFA_TYPE config key:
///   MFA_TYPE=push        → Approve push notification in Microsoft Authenticator
///   MFA_TYPE=phone_call  → Phone call; pick up and press #
/// </summary>
[SetUpFixture]
public class GlobalSetup
{
    public static string  StudentBaseUrl { get; private set; } = string.Empty;
    public static string  TeacherBaseUrl { get; private set; } = string.Empty;
    public static string  ApiBaseUrl     { get; private set; } = string.Empty;
    public static string  TestRole       { get; private set; } = "Teacher";
    public static string  AuthStatePath  => $"auth-state-{TestRole}.json";

    [OneTimeSetUp]
    public async Task RunBeforeAllTests()
    {
        // ----------------------------------------------------------------
        // 1. Load configuration — env vars always take precedence over JSON
        // ----------------------------------------------------------------
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // TEST_ROLE determines which auth-state file is used and which role is verified
        TestRole = config["TEST_ROLE"] ?? config["TestRole"] ?? "Teacher";

        var username = config["TEST_USERNAME"]
                       ?? config["AzureAd:TestUsername"]
                       ?? throw new InvalidOperationException(
                           "Set TEST_USERNAME env var or AzureAd:TestUsername in appsettings.Test.json.");

        var password = config["TEST_PASSWORD"]
                       ?? config["AzureAd:TestPassword"]
                       ?? throw new InvalidOperationException(
                           "Set TEST_PASSWORD env var or AzureAd:TestPassword in appsettings.Test.json.");

        var mfaType         = (config["MFA_TYPE"] ?? config["AzureAd:MfaType"] ?? "push").ToLowerInvariant();
        var phoneOptionText = config["PHONE_OPTION_TEXT"] ?? config["AzureAd:PhoneOptionText"] ?? "YOUR_PHONE_LAST_DIGITS";

        StudentBaseUrl = config["STUDENT_BASE_URL"] ?? config["StudentBaseUrl"]
                        ?? throw new InvalidOperationException(
                            "Set STUDENT_BASE_URL env var or StudentBaseUrl in appsettings.Test.json.");
        TeacherBaseUrl = config["TEACHER_BASE_URL"] ?? config["TeacherBaseUrl"]
                        ?? throw new InvalidOperationException(
                            "Set TEACHER_BASE_URL env var or TeacherBaseUrl in appsettings.Test.json.");
        ApiBaseUrl = config["API_BASE_URL"] ?? config["ApiBaseUrl"] ?? config["APIUrl"]
                     ?? throw new InvalidOperationException(
                         "Set API_BASE_URL env var or ApiBaseUrl/APIUrl in appsettings.Test.json.");

        var headless = string.Equals(
            config["HEADLESS"] ?? config["Headless"] ?? "false",
            "true", StringComparison.OrdinalIgnoreCase);

        // ----------------------------------------------------------------
        // 2. Launch browser — headed by default so human can approve MFA
        // ----------------------------------------------------------------
        var playwright = await Playwright.CreateAsync();
        var browser    = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless,
            SlowMo   = headless ? 0 : 80
        });

        // ----------------------------------------------------------------
        // 3. Smart reuse: try to load the existing auth state for this role.
        //    Decodes the JWT to verify the role claim still matches.
        //    If the role no longer matches (user switched roles in Azure AD),
        //    the stale file is deleted and a fresh login is performed below.
        // ----------------------------------------------------------------
        var existingToken = await TryReuseExistingStateAsync(browser, StudentBaseUrl);
        if (existingToken != null && TokenHasRole(existingToken, TestRole))
        {
            Console.WriteLine($"[GlobalSetup] Reusing auth-state for role '{TestRole}' — skipping MFA.");
            return;
        }

        if (File.Exists(AuthStatePath))
        {
            File.Delete(AuthStatePath);
            Console.WriteLine($"[GlobalSetup] Stale auth state for role '{TestRole}' deleted — re-authenticating.");
        }

        // ----------------------------------------------------------------
        // 4. Full authentication — browser login + MFA
        // ----------------------------------------------------------------
        var context = await browser.NewContextAsync();
        var page    = await context.NewPageAsync();

        // Trigger the Azure AD redirect by hitting a protected route
        var baseUrl = TestRole.Equals("Student", StringComparison.OrdinalIgnoreCase) ? StudentBaseUrl : TeacherBaseUrl;
        await page.GotoAsync($"{baseUrl}/Toy");
        await page.WaitForURLAsync(
            url => url.Contains("login.microsoftonline.com"),
            new PageWaitForURLOptions { Timeout = 30_000 });

        // Step 1: Enter email
        await page.FillAsync("input[type='email']", username);
        await page.ClickAsync("input[type='submit'], button[type='submit']");

        // Step 2: Enter password
        await page.WaitForSelectorAsync("input[type='password']",
            new PageWaitForSelectorOptions { Timeout = 15_000 });
        await page.FillAsync("input[type='password']", password);
        await page.ClickAsync("input[type='submit'], button[type='submit']");

        // Step 3: MFA challenge
        if (mfaType == "phone_call")
            await HandlePhoneCallMfaAsync(page, phoneOptionText);
        else
            await HandlePushMfaAsync(page);

        // Step 4: "Stay signed in?" (KMSI) — click No if it appears
        try
        {
            await page.WaitForSelectorAsync("#idBtn_Back",
                new PageWaitForSelectorOptions { Timeout = 15_000 });
            await page.ClickAsync("#idBtn_Back");
        }
        catch (TimeoutException)
        {
            // KMSI prompt did not appear — that is fine
        }

        // ----------------------------------------------------------------
        // 5. Wait until we are back in the app
        // ----------------------------------------------------------------
        await page.WaitForURLAsync(
            url => url.StartsWith(baseUrl),
            new PageWaitForURLOptions { Timeout = 30_000 });

        // ----------------------------------------------------------------
        // 6. Save the authenticated session for this role
        // ----------------------------------------------------------------
        await context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = AuthStatePath
        });
        await context.CloseAsync();
    }

    // ====================================================================
    // Try to reuse the existing auth-state file for this role.
    // Returns null if the file does not exist or the session has expired.
    // ====================================================================
    private static async Task<string?> TryReuseExistingStateAsync(IBrowser browser, string baseUrl)
    {
        if (!File.Exists(AuthStatePath)) return null;

        try
        {
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                StorageStatePath = AuthStatePath
            });
            try
            {
                var page = await context.NewPageAsync();
                var response = await page.GotoAsync($"{baseUrl}/Home/Index",
                    new PageGotoOptions { Timeout = 10_000 });

                if (response is null || !response.Ok) return null;

                // If we can navigate to a protected page, the session is valid
                return "valid";
            }
            finally
            {
                await context.CloseAsync();
            }
        }
        catch
        {
            return null;
        }
    }

    // ====================================================================
    // Decode the JWT payload and check whether the "roles" claim contains
    // the expected role. No external library — pure base64url decode.
    // ====================================================================
    private static bool TokenHasRole(string token, string expectedRole)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return false;

            // Convert base64url to standard base64
            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("roles", out var rolesElement))
                return false;

            return rolesElement.EnumerateArray()
                .Any(r => string.Equals(r.GetString(), expectedRole, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    // ====================================================================
    // MFA: Push notification
    // ====================================================================
    private static async Task HandlePushMfaAsync(IPage page)
    {
        Console.WriteLine(
            "[GlobalSetup] MFA=push  →  Please approve the notification in your Microsoft Authenticator app...");

        await page.WaitForURLAsync(
            url => url.Contains("login.microsoftonline.com/common/SAS/ProcessAuth"),
            new PageWaitForURLOptions { Timeout = 90_000 });
    }

    // ====================================================================
    // MFA: Phone call
    // ====================================================================
    private static async Task HandlePhoneCallMfaAsync(IPage page, string phoneOptionText)
    {
        try
        {
            await page.WaitForSelectorAsync(
                "a:has-text('Sign in another way'), a:has-text('sign in another way')",
                new PageWaitForSelectorOptions { Timeout = 5_000 });
            await page.ClickAsync(
                "a:has-text('Sign in another way'), a:has-text('sign in another way')");
        }
        catch (TimeoutException)
        {
            // Already on the identity-selection page — no link to click
        }

        var callOption = page.Locator("[data-value*='Voice']")
            .Filter(new LocatorFilterOptions { HasText = phoneOptionText });

        await callOption.WaitForAsync(new LocatorWaitForOptions { Timeout = 15_000 });
        await callOption.First.ClickAsync();

        Console.WriteLine(
            $"[GlobalSetup] MFA=phone_call  →  Calling ...{phoneOptionText}. Please pick up and press #...");

        await page.WaitForURLAsync(
            url => url.Contains("login.microsoftonline.com/common/SAS/ProcessAuth"),
            new PageWaitForURLOptions { Timeout = 90_000 });
    }
}
