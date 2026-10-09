# TMS Environment Configuration Reference

Inventory date: 2026-04-22. Sources reviewed: repo `appsettings*.json`, `Program.cs`, test setup, `F2025-azure-pipeline.yml`, and exported Azure DevOps build/release JSON files for Dev, Test, and Prod. Sensitive values are intentionally not printed here.

## Environment Selection

| Key | Purpose | Type | Required | Source | Sensitivity |
|---|---|---|---|---|---|
| `TMS_ENV` | Forces loading `appsettings.Test.json`, `appsettings.Development.json`, or `appsettings.Production.json`. Accepted values: `Test`, `Development`/`Dev`, `Production`/`Prod`. | string enum | Apps deployed outside of the Development environment must have it specified | IIS app-pool environment variable set by Test/Prod release scripts; read by API, Teacher, Student `Program.cs` | Non-secret |
| `WEBSITE_HOSTNAME` | Fallback environment detection when `TMS_ENV` is not set. | string | No | Azure/IIS host environment | Non-secret |
| `WEBSITE_SITE_NAME` | Fallback environment detection when `TMS_ENV` and `WEBSITE_HOSTNAME` are not set. | string | No | Azure/IIS host environment | Non-secret |
| `HOSTNAME` | Final fallback environment detection. | string | No | Host environment | Non-secret |

Dev release script does not currently set `TMS_ENV` since the Development Environment is run automatically; Test sets `TMS_ENV=Test`; Prod sets `TMS_ENV=Production`.

## Application Settings

| Key | Purpose | Type | Required | Source | Sensitivity |
|---|---|---|---|---|---|
| `ConnectionStrings:EaglesConnection` | SQL Server connection used by API EF Core and Teacher backup controller. | connection string | Yes for API and backup features | API and Teacher appsettings; overridden by Key Vault | Secret |
| `AzureAdTms:Instance` | Microsoft identity authority base URL. | URL | Yes | API, Teacher, Student appsettings | Non-secret |
| `AzureAdTms:Domain` | Azure AD tenant domain for Microsoft.Identity.Web binding. | string | Yes for web app auth | API, Teacher, Student appsettings | Non-secret |
| `AzureAdTms:TenantId` | Azure AD tenant ID used to build authority and issuer. | GUID | Yes | API, Teacher, Student appsettings | Public identifier |
| `AzureAdTms:ClientId` | Entra app/client ID; API audience and OAuth client. | GUID | Yes | API, Teacher, Student appsettings | Public identifier |
| `AzureAdTms:ClientSecret` | Confidential app secret for Microsoft.Identity.Web flows where needed. | string | Required if the deployed auth flow needs confidential client credentials | API, Teacher, Student appsettings; overridden by Key Vault | Secret; must be Key Vault or secret store |
| `AzureAdTms:CallbackPath` | OIDC sign-in callback path. | path | Yes for web apps | API, Teacher, Student appsettings | Non-secret |
| `AzureAdTms:SignedOutCallbackPath` | OIDC sign-out callback path. | path | Yes for web apps | API, Teacher, Student appsettings | Non-secret |
| `AzureAdTms:Audience` | API audience value; present in config, while code validates `ClientId` and `api://ClientId`. | string | No based on current code | API appsettings | Public identifier |
| `Api:ClientId` | Builds delegated API scope `api://{ClientId}/User_Access` for outbound API calls. | GUID | Yes for Teacher/Student API calls | Teacher and Student appsettings | Public identifier |
| `Api:BaseUrl` | Base URL for HTTP clients calling the API. Dev: `https://team2app-csdevwww.cegep-heritage.qc.ca/api/`; Test: `https://team2app-cstestwww.cegep-heritage.qc.ca/api/`; Prod: `https://team2app-csprodwww.cegep-heritage.qc.ca/api/`. | URL | Yes in deployed apps | Teacher and Student appsettings | Non-secret |
| `Gemini:ApiKey` | Google Gemini API key for AI toy field suggestions. | string | Yes only when AI suggestion feature is used | Teacher appsettings; overridden by Key Vault | Secret |
| `Gemini:Model` | Gemini model used for AI toy field suggestions. | string | No | Teacher appsettings | Non-secret |
| `Graph:BaseUrl` | Microsoft Graph base URL; present in config. | URL | No based on current code | Teacher appsettings | Non-secret |
| `Graph:Scopes` | Microsoft Graph scopes; present in config. | string array | No based on current code | Teacher appsettings | Non-secret |
| `DownstreamApis:MicrosoftGraph:BaseUrl` | Graph endpoint consumed by `.AddMicrosoftGraph(...)`. | URL | Yes if Graph integration is active | Teacher appsettings | Non-secret |
| `DownstreamApis:MicrosoftGraph:Scopes` | Graph scopes consumed by `.AddMicrosoftGraph(...)`; currently `User.Read`, `User.Read.All`. | string array | Yes if Graph integration is active | Teacher appsettings | Non-secret |
| `KeyVaultName` | Azure Key Vault name loaded as `https://{KeyVaultName}.vault.azure.net/`. Dev: `cs-app-keyvault`; Test: `cs-app-testkeyvault`; Prod: `cs-app-prodkeyvault`. | string | Recommended in deployed apps | API, Teacher, Student appsettings | Non-secret |
| `ManagedIdentity:ClientId` | User-assigned managed identity client ID for Key Vault access. Dev: `83ec3e48-a25c-4b19-b424-c20db577199f`; Test: `9f75bc5d-161c-4582-9cac-341c30051fad`; Prod: `b8be574d-aeef-4c6a-b8f1-4a5dfc89acc2`. | GUID | Required when using user-assigned managed identity | API, Teacher, Student appsettings | Public identifier |
| `TeacherBaseUrl` | Teacher app base URL; present in Student config. | URL | No based on current code | Student appsettings | Non-secret |
| `Logging:LogLevel:Default` | ASP.NET Core default log level. | string enum | No | API, Teacher, Student appsettings | Non-secret |
| `Logging:LogLevel:Microsoft.AspNetCore` | ASP.NET Core framework log level override. | string enum | No | API, Teacher, Student appsettings | Non-secret |
| `AllowedHosts` | ASP.NET Core host filtering. | string/list | No for local, recommended in deployment | API, Teacher, Student appsettings | Non-secret |

### Key Vault Screenshots

Screenshots of the Azure Key Vault secret list for the development, test, and production environments.

**Development Key Vault**

![image.png](/.attachments/image-b1b1c075-11b1-4114-85e6-925a7882263e.png)

**Test Key Vault**

![image.png](/.attachments/image-c70d33a0-31ff-4824-a3a3-b9dd3cca3bb4.png)

**Production Key Vault**

![image.png](/.attachments/image-13f650a5-8430-4e41-94f5-09d7d0b9cf8a.png)

## E2E Test Settings

| Key | Purpose | Type | Required | Source | Sensitivity |
|---|---|---|---|---|---|
| `TEST_USERNAME` / `AzureAd:TestUsername` | Azure AD username for Playwright login. | string | Yes for E2E auth tests | Environment variable or `TMS_TEST/appsettings.Test.json` | Secret/PII |
| `TEST_PASSWORD` / `AzureAd:TestPassword` | Azure AD password for Playwright login. | string | Yes for E2E auth tests | Environment variable or `TMS_TEST/appsettings.Test.json` | Secret |
| `TEST_ROLE` / `TestRole` | Role to validate and name auth-state file; typically `Teacher` or `Student`. | string enum | No | Environment variable or test appsettings | Non-secret |
| `MFA_TYPE` / `AzureAd:MfaType` | MFA automation path: `push` or `phone_call`. | string enum | No | Environment variable or test appsettings | Non-secret |
| `PHONE_OPTION_TEXT` / `AzureAd:PhoneOptionText` | Phone MFA option text/last digits selector. | string | Required for phone-call MFA | Environment variable or test appsettings | Sensitive/PII |
| `STUDENT_BASE_URL` / `StudentBaseUrl` | Student app URL used by E2E tests. | URL | Yes for E2E tests | Environment variable or test appsettings | Non-secret |
| `TEACHER_BASE_URL` / `TeacherBaseUrl` | Teacher app URL used by E2E tests. | URL | Yes for E2E tests | Environment variable or test appsettings | Non-secret |
| `API_BASE_URL` / `ApiBaseUrl` / `APIUrl` | API URL used by E2E tests. | URL | Yes for E2E tests | Environment variable or test appsettings | Non-secret |
| `HEADLESS` / `Headless` | Controls Playwright browser visibility. | boolean string | No | Environment variable or test appsettings | Non-secret |

## Pipeline Variables And Release Constants

| Key | Purpose | Type | Required | Source | Sensitivity |
|---|---|---|---|---|---|
| `BuildConfiguration` | Passed to `dotnet build`, `dotnet test`, and `dotnet publish`; current value `Release`. | string enum | Yes | Dev/Test/Prod build definitions | Non-secret |
| `BuildPlatform` | Classic pipeline variable; current value `any cpu`. Not directly used by active dotnet tasks. | string | No based on current tasks | Dev/Test/Prod build definitions | Non-secret |
| `system.debug` | Enables verbose Azure DevOps task logging when `true`; current value `false`. | boolean string | No | Dev/Test/Prod build definitions | Non-secret |
| `BuildParameters.RestoreBuildProjects` | Restore project glob: `**/*.csproj`. | glob | Yes for YAML build | `F2025-azure-pipeline.yml` | Non-secret |
| `BuildParameters.TestProjects` | Test project glob: `**/*[Tt]ests/*.csproj`. | glob | Yes for YAML build | `F2025-azure-pipeline.yml` | Non-secret |
| `Parameters.RestoreBuildProjects` | Restore project glob referenced by exported classic build tasks. | glob | Yes for classic restore task | Classic build task input | Non-secret |
| `Build.ArtifactStagingDirectory` / `build.artifactstagingdirectory` | Output root for published `student`, `teacher`, and `api` artifacts. | path | Yes | Azure DevOps predefined variable | Non-secret |
| `Build.DefinitionName` | Used in fallback artifact share path. | string | No for container artifacts | Azure DevOps predefined variable | Non-secret |
| `Build.BuildNumber` | Used in fallback artifact share path. | string | No for container artifacts | Azure DevOps predefined variable | Non-secret |
| `System.DefaultWorkingDirectory` | Release package root for IIS deployment tasks. | path | Yes | Azure DevOps predefined variable | Non-secret |
| Release script constants: `siteName`, `basePath`, `port`, `poolRoot`, `poolTeacher`, `poolApi`, `teacherAlias`, `apiAlias` | IIS layout. Current values: site `ToyManagementSystem`, base path `D:\inetpub\wwwroot`, port `92`, pools `studentTmsAppPool`, `teacherTmsAppPool`, `tmsAppPoolApi`, aliases `teacher`, `api`. | strings/int | Yes for release script | Inline PowerShell in release definitions | Non-secret |