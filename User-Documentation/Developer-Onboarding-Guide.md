# Developer Onboarding Guide — Toy Management System (TMS)

**Team:** K50 Team 2 | **Course:** Development Project 2 | **Assignment:** A04 Part D

Use this guide on a clean machine (no previous clone) to set up and run the full TMS stack in a single working session. Every command is shown verbatim. Every configuration key is explained. No step may be skipped.

---

## Table of Contents
[[_TOC_]]

---

## 1 · Repository Access

### Repository URL

```
https://csazure.cegep-heritage.qc.ca:8080/devprojectsF2025/Prject%202%20-%20Toys%20tracking%20system/_git/Prject%202%20-%20Toys%20tracking%20system
```

### Branches

| Branch | Purpose |
|--------|---------|
| `F25_Prod` | **Production branch.** This is what is deployed. |
| `F25_Develop` | Default development branch. Base all new work here. |

Always branch off `F25_Develop` and merge to `F25_Develop` via pull request once approved.

### Requesting access

If you cannot see or clone the repository:

1. Open Azure DevOps → **Repos → Branches → Security** to confirm you lack permissions.
2. Message the product owner or course instructor with the following:
   - Your Azure DevOps email address
   - The repository URL above
   - Access level needed: **Read**, **Contribute**, and branch access to `F25_Prod`, `F25_Develop`, and your feature branch
3. Wait for confirmation before attempting to clone.

![image.png](/.attachments/image-36659724-6634-441f-b4a9-57effdd7d693.png)
---

## 2 · Prerequisites

Install every tool below. Do not substitute versions. If a tool is already installed, verify its version before continuing.

| Tool | Required version | Download |
|------|-----------------|----------|
| Git for Windows | 2.49.x or newer | https://git-scm.com |
| .NET SDK | 8.0.x | https://dotnet.microsoft.com/download |
| Visual Studio 2022 | 17.8+ with **ASP.NET and web development** workload | Visual Studio Installer |
| PowerShell 7 (pwsh) | 7.x | https://aka.ms/powershell |
| Node.js | 20.x LTS | https://nodejs.org |
| SQL Server | 2019 or 2022 | https://www.microsoft.com/sql-server |
| SSMS or Azure Data Studio | SSMS 20.x / ADS latest | https://aka.ms/ssms |

### Verify installations

Run all commands in PowerShell 7:

```powershell
git --version
dotnet --version
node --version
npm --version
pwsh --version
```

> ⚠️ Every command must print a version number. If any command fails, reinstall that tool before continuing.

### Verify Visual Studio workload

Open **Visual Studio Installer** → click **Modify** on Visual Studio 2022 → confirm **ASP.NET and web development** is checked → click **Modify** if it is not yet installed.

![Screenshot 2026-05-01 142528.png](/.attachments/Screenshot%202026-05-01%20142528-84b02afe-d290-438d-a846-b514e1a3b1dc.png)

![Screenshot 2026-05-01 142528.png](/.attachments/Screenshot%202026-05-01%20142528-ed7168ca-c54c-40cb-9056-0b2c4f6f2da0.png)
---

## 3 · Cloning and Project Structure

### Clone the repository

Run the following in PowerShell or the regular command prompt. Replace `<your-user>` with your Windows username.

```powershell
cd "C:\Users\<your-user>\source\repos"

git clone "https://csazure.cegep-heritage.qc.ca:8080/devprojectsF2025/Prject%202%20-%20Toys%20tracking%20system/_git/Prject%202%20-%20Toys%20tracking%20system"

cd "Prject 2 - Toys tracking system"
git checkout F25_Develop
```

> ⚠️ If you see a certificate error during clone, add `--config http.sslVerify=false` after the clone command. Ask your instructor before doing this on a non-school machine.

### Open the solution

```powershell
cd ToyManagementSystem
start ToyManagementSystem.sln   # opens Visual Studio automatically
```

### Top-level folder map

| Folder | Contents |
|--------|----------|
| `TMS_API` | ASP.NET Core Web API. Exposes all data endpoints. Runs on **port 5002**. |
| `TMSStudent` | ASP.NET Core MVC. Student-facing portal. Runs on **port 5000**. |
| `TMSTeacher` | ASP.NET Core MVC. Teacher/admin portal. Includes Gemini AI integration. Runs on **port 5001**. |
| `TMS_SharedLibrary` | Shared entities, `K40TmsDdContext`, and models used by all three projects above. |
| `TMS_TEST` | NUnit/xUnit unit tests and Playwright E2E tests. |

> ![Screenshot 2026-05-01 134022.png](/.attachments/Screenshot%202026-05-01%20134022-503d02d5-5152-4c4d-bd8c-a52ee6a3d56f.png)

---

## 4 · Environment Configuration

Three projects require secrets that are **never committed to the repository**: `TMS_API`, `TMSStudent`, and `TMSTeacher`. Each stores its secrets in a local user-secrets file managed by the .NET Secret Manager. You must also authenticate with Azure CLI so the app can reach Azure Key Vault at startup.

---

### Step 1 — Authenticate with Azure
If running on a personal machine, ensure your connection to the VPN.

### Step 2 — Set the environment selector

The application reads this variable to decide which `appsettings` file to load. Set it in every terminal session before starting any project.

```powershell
$env:TMS_ENV = "Development"
# Development  →  appsettings.Development.json
# Test         →  appsettings.Test.json
# Production   →  appsettings.Production.json
```
---

### Step 3 — Set secrets for TMS_API
In order to add your user secrets: 
Right click on the project you are trying to configure the user secrets
Click on the `Manage user secrets`, this will bring you to an empty page, you have to fill with the user secrets value:
![image.png](/.attachments/image-4092f98c-7318-4b7a-abfd-1316730b59f0.png)

`TMS_API` requires two secrets. Obtain both values from your team lead or product owner.

| Key | What it controls | Where to get the value |
|-----|-----------------|------------------------|
| `ConnectionStrings:EaglesConnection` | SQL Server connection string. Points to the `K40_TMS_Initial` database you will create in Section 5. | Ask team lead. Use the local DB string from Section 5 for development. |
| `AzureAdTms:ClientSecret` | Azure App Registration client secret used for Entra ID (Azure AD) authentication across the whole app. | Ask product owner. **Do not generate a new one.** |

![image.png](/.attachments/image-d7625034-d697-472e-8f1e-8d4ca7f488d9.png)
---
### Step 4 — Set secrets for TMSStudent

| Key | What it controls | Where to get the value |
|-----|-----------------|------------------------|
| `APIUrl` | Base URL the Student portal uses when calling `TMS_API`. | Always `http://localhost:5002/` for local development. |
| `AzureAdTms:ClientSecret` | Same App Registration secret as `TMS_API`. | Same value as the `AzureAdTms:ClientSecret` used for `TMS_API` — ask the product owner. |

![image.png](/.attachments/image-ca086fdd-064d-4d48-a044-96b0c417eb4e.png)
---

### Step 5 — Set secrets for TMSTeacher

`TMSTeacher` needs four secrets, including the Gemini AI key.

| Key | What it controls | Where to get the value |
|-----|-----------------|------------------------|
| `APIUrl` | Base URL for API calls. | Always `http://localhost:5002/` for local development. |
| `Gemini:ApiKey` | Google Gemini API key enabling the AI toy-suggestion feature. | Request from the feature owner through a secure channel. **Do not commit.** |
| `AzureAdTms:ClientSecret` | Same App Registration secret shared across all three projects. | Same value as the `AzureAdTms:ClientSecret` used for `TMS_API` — ask the product owner. |
| `ConnectionStrings:EaglesConnection` | SQL Server connection string. | Same value as the `ConnectionStrings:EaglesConnection` used for `TMS_API`. |

![image.png](/.attachments/image-12200a4e-3bfc-42e6-aae6-8cd8eda6ba7d.png)
---

### Complete secrets reference

| Project | Key | What it controls | Source | How to obtain |
|---------|-----|-----------------|--------|---------------|
| `TMS_API` | `ConnectionStrings:EaglesConnection` | SQL Server DB connection | Team lead / Key Vault | Ask the team lead. Use the local DB string from Section 5 for development. |
| `TMS_API` | `AzureAdTms:ClientSecret` | Entra ID authentication for the API | Azure App Registration | Ask the product owner. Never generate a new one. |
| `TMSStudent` | `APIUrl` | Endpoint the Student portal calls | Local setting | Always `http://localhost:5002/` for local development. |
| `TMSStudent` | `AzureAdTms:ClientSecret` | Entra ID authentication for the Student portal | Azure App Registration | Same value as the `AzureAdTms:ClientSecret` used for `TMS_API`. |
| `TMSTeacher` | `APIUrl` | Endpoint the Teacher portal calls | Local setting | Always `http://localhost:5002/` for local development. |
| `TMSTeacher` | `Gemini:ApiKey` | AI toy-suggestion feature | Google / feature owner | Request from the feature owner through a secure channel. |
| `TMSTeacher` | `AzureAdTms:ClientSecret` | Entra ID authentication for the Teacher portal | Azure App Registration | Same value as the `AzureAdTms:ClientSecret` used for `TMS_API`. |
| `TMSTeacher` | `ConnectionStrings:EaglesConnection` | SQL Server DB connection | Team lead / Key Vault | Same value as the `ConnectionStrings:EaglesConnection` used for `TMS_API`. |


> ⚠️ The `appsettings.json` files in the repository contain no secret values. All actual values are stored in user-secrets and are never committed.
---

## 5 · Database Setup

> ⚠️ The repository is configured **database-first** using `K40TmsDdContext`. There are no EF migrations committed. You must create the local database manually, then point the connection string at it.

### Step 1 — Create the local database

Open **SQL Server Management Studio**. Connect to your local SQL Server instance (typically `localhost` or `.\SQLEXPRESS`). Open a new Query window and run:

```sql
CREATE DATABASE K40_TMS_Initial;
GO
```

### Step 2 — Confirm the connection string

The connection string you set in Section 4 Step 4 must reference `K40_TMS_Initial`. For a default local SQL Server instance:

```
Server=localhost;Database=K40_TMS_Initial;Trusted_Connection=True;TrustServerCertificate=True;
```

If you are using a named instance (e.g. SQL Express), replace `localhost` with `.\SQLEXPRESS`.

### Step 3 — Verify no migrations are pending

Because no EF migrations are committed, this command is expected to report `No migrations were applied`:

```powershell
cd "C:\Users\<your-user>\source\repos\Prject 2 - Toys tracking system\ToyManagementSystem"

dotnet ef database update --project .\TMS_API\TMS_API.csproj --startup-project .\TMS_API\TMS_API.csproj
```

If `dotnet ef` is not found, install it first:

```powershell
dotnet tool install --global dotnet-ef
```

 ![image.png](/.attachments/image-18c7b315-99da-4f92-9a85-6cfe11741575.png)
---

## 6 · Running the Application

The full stack requires the API, Student application, and Teacher application to run at the same time. This can be done using Visual Studio multiple startup projects.

> ⚠️ All three projects must be running for the application to work end-to-end.

### Step 1 — Restore all dependencies (run once)

```powershell
cd "C:\Users\<your-user>\source\repos\Prject 2 - Toys tracking system\ToyManagementSystem"
dotnet restore ToyManagementSystem.sln
npm install
```

### Step 2 — Start all projects at the same time

Open Visual Studio, then right-click the solution in **Solution Explorer** and select **Configure Startup Projects**.

Set the following projects to **Start**:

- `TMS_API`
- `TMSStudent`
- `TMSTeacher`

Click **Apply**, then click **OK** or **Save**.

![Screenshot 2026-05-01 155653.png](/.attachments/Screenshot%202026-05-01%20155653-3429c9f4-736f-4351-9b7f-2f8c1dd9d88c.png)

![Screenshot 2026-05-01 155720.png](/.attachments/Screenshot%202026-05-01%20155720-10371230-b688-4771-8052-d82a7c569ea8.png)

✅ **Successful startup:** all three projects start without errors.  
The API runs on `http://localhost:5002`, the Student app runs on `http://localhost:5000`, and the Teacher app runs on `http://localhost:5001`.

### Port summary

| Service | Local URL | What to verify |
|---------|-----------|----------------|
| `TMS_API` | http://localhost:5002 | Swagger UI loads at `/swagger` |
| `TMSStudent` | http://localhost:5000 | Login page visible |
| `TMSTeacher` | http://localhost:5001 | Login page visible |

![image.png](/.attachments/image-b663ae51-68ad-415f-90fb-43aa7c555551.png)

![image.png](/.attachments/image-b4f368ae-1c3a-4aca-b32b-f4953b161218.png)
 
![image.png](/.attachments/image-fc94f262-23a4-40f7-91d7-a63785d256bf.png)

---

## 7 · Running the Tests

### A — Unit and integration tests
To run the tests, open **Test Explorer** in Visual Studio and select **Run All Tests**.

✅ All tests should show a green **Passed** count. Any **Failed** entry means a misconfiguration — re-check Section 4 before investigating test code.
There are 2 ⚠️ in the project currently that could not be resolved.

---

### B — E2E tests (Playwright)

Playwright tests automate a real browser. They require the Student app to be running on port 5000 and a valid Azure AD test account.

#### Step 1 — Install Playwright browsers (run once)

```powershell
cd "C:\Users\<your-user>\source\repos\Prject 2 - Toys tracking system\ToyManagementSystem\TMS_TEST"
ppwershell .\bin\Debug\net8.0\playwright.ps1 install
```

#### Step 2 — Create the E2E environment file

Create a new file at `TMS_TEST\E2E\playwright.env` with the following content. Obtain credentials from the test account owner.

```env
TEST_USERNAME=your_test_account@cegep-heritage.qc.ca
TEST_PASSWORD=REDACTED
MFA_TYPE=push
STUDENT_BASE_URL=http://localhost:5000
HEADLESS=false
TEST_ROLE=Teacher
```

> ⚠️ Do not commit `playwright.env`. It is already in `.gitignore`. Never paste credentials into test code.


#### Step 3 — Confirm the Student app is running 
If the Student app is already running and showing `Now listening on: http://localhost:5000`, skip this step.

Otherwise, start the application from Visual Studio and confirm that the Student app is running on port 5000.

#### Step 4 — Run all E2E tests

Running all the test will trigger the E2E Test. In the `appsettings.Test.json` you can modify the user from Teacher to Student to test the different roles.

## 8 · Common Issues and Solutions
---

### Issue 1 — Error while trying to start the project
![image (2).png](/.attachments/image%20(2)-f2d5dfdf-6929-41a5-9b36-4cc75bedeaf9.png)

**Symptom**

The application fails to start and the terminal shows an error similar to

MsalclientException: One client credential type either require : clientSecret, certificate

**Cause**

The local machine is not authenticated to Azure, or the account does not have the required Key Vault access policy assigned.

**Resolution**

```
 - Confirm that you have the right user secrets in your folder
 - Clean your solution
 - Build your solution
```

Then ask your team lead to assign you the **Reader** and **Key Vault Secrets User** RBAC roles on the vault. Restart the affected app after access is granted.
---

### Issue 2 — E2E login times out during MFA

**Symptom**

A Playwright test fails with a timeout error during Azure AD login. The browser opens the Microsoft login page but the test does not proceed past the MFA step.

**Cause**

Either the MFA approval was not given in time, `TEST_USERNAME` or `TEST_PASSWORD` in `playwright.env` is incorrect, or `MFA_TYPE` does not match the method configured on the test account or you did not enter correct information while trying to log in **(This is rare but it might happen)**.

**Resolution**

1. Open `TMS_TEST\E2E\playwright.env` and verify `TEST_USERNAME` and `TEST_PASSWORD` are exactly correct (no trailing spaces).
2. Confirm `MFA_TYPE` matches your test account's primary method: use `push` for the Authenticator app or `phone_call` for a voice call.
3. Re-run the test and approve the MFA push notification within 30 seconds.
4. If the test account does not exist, request a dedicated test account from the course instructor.
---

### Issue 3 — Extra Teacher added to the path when switching language

**Symptom**

After successfully connecting to TMS, on the teacher's portal, when switching from a language to another, you can see an extra Teacher added to the path.

```
Going from http://localhost:5001/Toy to http://localhost:5001/teacher/Toy
```

No browser page loads.

**Cause**
Unfortunately the cause of this issue, has not been found, as it was discovered in our last sprint. We made some investigation, but they were not successfull.

**Resolution**
```
Investigate more on the subject. We recommend checking the path in the controllers.
```
![image.png](/.attachments/image-d75685f8-64c3-41d8-ba7e-43d5c6ec2742.png)

## 9 · Validation Statement

The following team member followed this guide step-by-step on a **clean machine** (fresh clone, no prior project files) and confirmed that every step succeeded.

| Field | Value |
|-------|-------|
| **Validated by** | Barbara-Ann Pitre Menard |
| **Validation date** | 01/05/2026 |
| **Environment** | Clean machine / fresh clone — no prior project files |
| **Confirmation** | I followed this guide step-by-step and successfully cloned, configured, ran the full application, and executed unit and E2E tests locally. |