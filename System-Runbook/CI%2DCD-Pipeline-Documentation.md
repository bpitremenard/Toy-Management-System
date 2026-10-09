# CI/CD Pipeline Documentation

This section explains the CI/CD pipeline for the Toy Management System across the development, test, and production environments.

---

# Development Environment

## Trigger

- Push to the `F25_Develop` branch

## Database Name

`K50_TMS`

## Stages & Steps

### 1. Build Stage

- Run: `dotnet restore`
- Run: `dotnet build`

**Success:** Build completes without errors.

### 2. Test Stage

- Run: `dotnet test`

**Success:** All tests pass.

### 3. Publish Artifacts

- Package application
- Store artifacts

**Success:** Artifacts are available.

## Environment Configuration

- Uses development secrets
- No approval required

## Deployment Target

- Azure App Service (Dev)

| Application | URL |
|---|---|
| Student | https://team2app-csdevwww.cegep-heritage.qc.ca/Home/Login |
| Teacher | https://team2app-csdevwww.cegep-heritage.qc.ca/Teacher/Home/Login |

---

# Test Environment

## Trigger

- Push to the `F25_Test` branch

## Database Name

`K50_TMS_Test`

## Stages & Steps

### 1. Build Stage

- Same as development

**Success:** Build completes without errors.

### 2. Test Stage

- Same as development

**Success:** All tests pass.

### 3. Deploy to Test

- Deploy using Azure Pipeline

**Success:** App is accessible using the test URLs.

## Environment Configuration

- Uses test database

## Deployment Target

- Azure App Service (Test)

| Application | URL |
|---|---|
| Student | https://team2app-cstestwww.cegep-heritage.qc.ca/Home/Login |
| Teacher | https://team2app-cstestwww.cegep-heritage.qc.ca/Teacher/Home/Login |

---

# Production Environment

## Trigger

- Push to the `F25_Prod` branch

## Database Name

`K50_TMS`

## Stages & Steps

### 1. Build Validation

- Validate artifact

**Success:** Artifact is valid and ready for deployment.

### 2. Approval Gate

- Manual approval required

**Success:** Deployment is approved before production release.

### 3. Deploy to Production

- Deploy to Azure App Service

**Success:** Deployment completes with no errors.

## Environment Configuration

- Uses production Key Vault secrets
- Restricted access

## Deployment Target

- Azure App Service (Production)

| Application | URL |
|---|---|
| Student | https://team2app-csprodwww.cegep-heritage.qc.ca/Home/Login |
| Teacher | https://team2app-csprodwww.cegep-heritage.qc.ca/Teacher/Home/Login |

---

# Required Screenshots

Add screenshots for:

- Development pipeline run with all stages green
- Test pipeline run with all stages green
- Production pipeline run with all stages green
- YAML pipeline file open in Azure DevOps

Current pipeline run screenshot:

![image.png](/.attachments/image-abde2147-4392-449f-a712-413ccdcab1b7.png)

---

# YAML Pipeline File

```yaml
variables:
- name: BuildParameters.RestoreBuildProjects
  value: '**/*.csproj'

trigger:
  branches:
    include:
    - refs/heads/F25_Develop

name: $(date:yyyyMMdd)$(rev:.r)

resources:
  repositories:
  - repository: self
    type: git
    ref: refs/heads/F25_Develop

jobs:
- job: Job_1
  displayName: Agent job 1
  pool:
    name: CSDEVWWW_T2

  steps:
  - checkout: self

  - task: DotNetCoreCLI@2
    displayName: Restore
    inputs:
      command: restore
      projects: $(BuildParameters.RestoreBuildProjects)

  - task: DotNetCoreCLI@2
    displayName: Build StudentApp
    inputs:
      projects: ToyManagementSystem/TMSStudent/TMSStudent.csproj
      arguments: --configuration $(BuildConfiguration)

  - task: DotNetCoreCLI@2
    displayName: Build TeacherApp
    inputs:
      projects: ToyManagementSystem/TMSTeacher/TMSTeacher.csproj
      arguments: --configuration $(BuildConfiguration)

  - task: DotNetCoreCLI@2
    displayName: Build Api
    inputs:
      projects: ToyManagementSystem/TMS_API/TMS_API.csproj
      arguments: --configuration $(BuildConfiguration)

  - task: VSTest@2
    displayName: TMS Tests
    enabled: False
    inputs:
      testAssemblyVer2: >-
        **\TMS_TEST*test*.dll
        !**\*TestAdapter.dll
        !**\obj\**
      runTestsInIsolation: true
      codeCoverageEnabled: true

  - task: DotNetCoreCLI@2
    displayName: Run TMS Tests
    inputs:
      command: test
      projects: '**/TMS_TEST/*.csproj'
      arguments: '--configuration $(BuildConfiguration) --filter "FullyQualifiedName!~TMS_TEST.E2E" -m:1 '

  - task: DotNetCoreCLI@2
    displayName: Publish Student App
    inputs:
      command: publish
      publishWebProjects: false
      projects: ToyManagementSystem/TMSStudent/TMSStudent.csproj
      arguments: --configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)\student
      zipAfterPublish: false
      modifyOutputPath: false

  - task: DotNetCoreCLI@2
    displayName: Publish Teacher App
    inputs:
      command: publish
      publishWebProjects: false
      projects: ToyManagementSystem/TMSTeacher/TMSTeacher.csproj
      arguments: --configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)\teacher
      zipAfterPublish: false
      modifyOutputPath: false

  - task: DotNetCoreCLI@2
    displayName: Publish Teacher Api
    inputs:
      command: publish
      publishWebProjects: false
      projects: ToyManagementSystem/TMS_API/TMS_API.csproj
      arguments: --configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)\api
      zipAfterPublish: false
      modifyOutputPath: false

  - task: PowerShell@2
    displayName: Modify Web.Config
    inputs:
      targetType: inline
      script: >-
        $snippet = @'
                <modules runAllManagedModulesForAllRequests="true">
                  <remove name="WebDAVModule"/>
                </modules>
        '@

        $files = @(
          "$(Build.ArtifactStagingDirectory)\api\web.config",
          "$(Build.ArtifactStagingDirectory)\teacher\web.config",
          "$(Build.ArtifactStagingDirectory)\student\web.config"
        )

        foreach ($file in $files) {
          if (-not (Test-Path $file)) {
            Write-Host "Skipping $file (not found)"
            continue
          }

          $content = Get-Content $file -Raw

          if ($content -match 'WebDAVModule') {
            Write-Host "Snippet already present in $file, skipping"
            continue
          }

          if ($content -match '<system\.webServer') {
            $content = $content -replace '(<system\.webServer[^>]*>)', "`$1`r`n$snippet"
          }
          else {
            $block = @"
          <system.webServer>
        $snippet
          </system.webServer>
        "@
            $content = $content -replace '</configuration>', "$block`r`n</configuration>"
          }

          Set-Content -Path $file -Value $content -Encoding UTF8
          Write-Host "Patched $file"
        }

  - task: PublishBuildArtifacts@1
    displayName: Publish Artifact
    condition: succeededOrFailed()
    inputs:
      PathtoPublish: $(build.artifactstagingdirectory)
      TargetPath: '\\my\share\$(Build.DefinitionName)\$(Build.BuildNumber)'
```