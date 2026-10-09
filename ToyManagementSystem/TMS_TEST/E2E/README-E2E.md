# Toy Management System — Playwright E2E Tests

End-to-end tests for the TMS ASP.NET Core MVC apps (Student & Teacher) **and** its REST API,
using Playwright and NUnit.

---

## Prerequisites

- .NET 8 SDK
- PowerShell (for the Playwright browser installer)
- The **Student app** running on **port 5000** before executing any tests

---

## 1. Install Playwright browsers

After building the project for the first time, install the required browser binaries:

```powershell
cd TMS_TEST
pwsh bin/Debug/net8.0/playwright.ps1 install
```

---

## 2. Fill in credentials

The test suite needs your Azure AD account credentials and MFA settings.
There are **two placeholder files** you must fill in (they are git-ignored):

### File 1 — `TMS_TEST/E2E/playwright.env`

```env
TEST_USERNAME=YOUR_USERNAME@cegep-heritage.qc.ca   ← replace
TEST_PASSWORD=YOUR_PASSWORD                          ← replace
MFA_TYPE=push                                        ← "push" or "phone_call"
PHONE_OPTION_TEXT=YOUR_PHONE_LAST_DIGITS            ← replace (phone_call only)
STUDENT_BASE_URL=http://localhost:5000
HEADLESS=false
```

### File 2 — `TMS_TEST/appsettings.Test.json`

```json
{
  "STUDENT_BASE_URL": "http://localhost:5000",
  "Headless": "false",
  "AzureAd": {
    "TestUsername": "YOUR_USERNAME@cegep-heritage.qc.ca",
    "TestPassword": "YOUR_PASSWORD",
    "MfaType": "push",
    "PhoneOptionText": "YOUR_PHONE_LAST_DIGITS"
  }
}
```

> Environment variables always take precedence over `appsettings.Test.json`.

---

## 3. Role-based testing workflow

Tests are separated into two categories — **Teacher** and **Student**.
You can run them separately or together.

### Option 1: Run both roles simultaneously

**To run all tests (both Teacher and Student):**
1. Go to **Azure Portal → App registrations → your app → App roles assignments**
2. Assign yourself **both** Teacher and Student roles
3. Set `TEST_ROLE=Both` in your env or `appsettings.Test.json`
4. Run all tests:
   ```bash
   cd TMS_TEST
   dotnet test
   ```

This will authenticate for both roles during setup (requires 2 MFA approvals), then run all tests.

### Option 2: Run each role separately

**To run Teacher tests only:**
1. Assign yourself the **Teacher** role (remove Student if previously assigned)
2. Set `TEST_ROLE=Teacher` in your env or `appsettings.Test.json`
3. Run the Teacher tests:
   ```bash
   cd TMS_TEST
   dotnet test --filter "TestCategory=Teacher"
   ```

**To run Student tests only:**
1. Assign yourself the **Student** role (remove Teacher if previously assigned)
2. Set `TEST_ROLE=Student` in your env or `appsettings.Test.json`
3. Run the Student tests:
   ```bash
   cd TMS_TEST
   dotnet test --filter "TestCategory=Student"
   ```

### No manual deletion needed — smart auth state

Each role has its own auth-state file:
- `auth-state-Teacher.json` — created automatically on first Teacher test run
- `auth-state-Student.json` — created automatically on first Student test run

`GlobalSetup` decodes the cached JWT token and checks the `roles` claim on every run.
If the role no longer matches (you switched roles in Azure AD), the stale file is
**deleted automatically** and a fresh MFA login is triggered — no manual cleanup required.

---

## 4. Choose your MFA flow

### Push notification (`MFA_TYPE=push`)

Flow: Email → Password → **Approve notification in Microsoft Authenticator** → KMSI "No"

Set `MFA_TYPE=push` (default). When the browser window opens, approve the push in your app.

### Phone call (`MFA_TYPE=phone_call`)

Flow: Email → Password → **Call +X…** → pick up phone + press `#` → KMSI "No"

1. Set `MFA_TYPE=phone_call`
2. Set `PHONE_OPTION_TEXT` to the last digits shown on screen (e.g. `XXXXXXXX46`)
3. Pick up the phone and press `#` when called

---

## 5. Load credentials and start the apps

### Load credentials (Windows PowerShell)

```powershell
Get-Content TMS_TEST/E2E/playwright.env | ForEach-Object {
    if ($_ -match '^\s*([^#][^=]*)=(.*)$') {
        [System.Environment]::SetEnvironmentVariable($Matches[1].Trim(), $Matches[2].Trim())
    }
}
```

### Start the Student app

```powershell
cd TMSStudent
dotnet run --urls "http://localhost:5000"
```

---

## 6. Run the tests

```bash
cd TMS_TEST

# Run all authentication tests
dotnet test

# Run specific test case
dotnet test --filter "TestCategory=TC-001"

# Run multiple test cases
dotnet test --filter "TestCategory=TC-001|TestCategory=TC-002"
```

---

## Project structure

| File | Purpose |
|------|---------|
| `AuthenticationTests.cs` | 8 authentication tests (TC-001 to TC-008) |
| `appsettings.Test.json` | JSON config — **fill in credentials here** |
| `playwright.env` | Env-var template — **fill in credentials here** |

---

## Test Coverage

### Authentication Tests (TC-001 to TC-008)

| Test Case | Description | Type |
|-----------|-------------|------|
| TC-001 | Unauthenticated - Navigate to protected page redirects to login | Regression |
| TC-002 | Log in with valid credentials - authenticates and redirects to home | Critical-Path |
| TC-003 | Log in with invalid password - error displayed | Negative |
| TC-004 | Log in with non-existent username - error displayed | Negative |
| TC-005 | Log out - session ends and redirects to login/home | Regression |
| TC-006 | Session reuse - authenticated page accessible without re-login | Regression |
| TC-007 | SQL injection input - should be rejected | Edge Case |
| TC-008 | Log out and press back button - no access to protected pages | Edge Case |

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| MFA timeout | You have 90 seconds to approve; increase `Timeout` in test if needed |
| Login fails | Check `TEST_USERNAME` / `TEST_PASSWORD` are correct |
| Phone call option not found | Verify `PHONE_OPTION_TEXT` matches the digits shown on-screen exactly |
| Playwright browsers not installed | Run `pwsh bin/Debug/net8.0/playwright.ps1 install` |
| Student app not responding | Ensure Student app is running on port 5000 |

---

## Running against Test Server

For final validation, update your configuration to point to the test server:

```json
{
  "STUDENT_BASE_URL": "https://your-student-test-server-url",
  "HEADLESS": "false"
}
```
