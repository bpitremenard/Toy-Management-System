# Log Reference

The Toy Management System uses ASP.NET Core 8's built-in ILogger framework across all three application components: TMS_API, TMSTeacher, and TMSStudent. Logs are emitted to stdout and captured by IIS on the production server.

---

## Log Storage Locations

### Primary — IIS stdout log files

**Path:**  
`C:\inetpub\wwwroot\<AppName>\logs\stdout_<date>_<pid>.log`

**Enabled via:**  
`stdoutLogEnabled="true"` in `web.config`

**File details:**
- One log file per application instance restart
- Each file is timestamped

---

### Secondary — Windows Application Event Log

**Source:**  
ASP.NET Core / TMS_API / TMSTeacher / TMSStudent

Critical and Error level messages are forwarded here by the EventLog provider.

---

## How to Access Logs

### IIS stdout log files

Connect to the CSDEVWWW server using Remote Desktop or a UNC path, then navigate to the application log folder:

```text
\\CSDEVWWW\C$\inetpub\wwwroot\TMS_API\logs\
\\CSDEVWWW\C$\inetpub\wwwroot\TMSTeacher\logs\
\\CSDEVWWW\C$\inetpub\wwwroot\TMSStudent\logs\
```

To view a live log from an admin PowerShell session:

```powershell
Get-Content -Path .\stdout_<timestamp>.log -Wait -Tail 100
```

---

### Windows Event Log

Open Event Viewer on the CSDEVWWW server and go to:

```text
Windows Logs > Application
```

Filter by `.NET Runtime`, or run:

```powershell
Get-EventLog -LogName Application -Source "*TMS*" -Newest 50
```

---

### Azure DevOps Pipeline Logs

Build and deploy logs are available in Azure DevOps under:

```text
Pipelines > Runs
```

Each run includes the restore, build, test, publish, and deployment output.

---

## Normal Log Entry Examples

### Normal Entry #1 — Scheduled overdue check starting

```text
[06:00:00 INF] Running scheduled overdue toy check at 1/15/2026 6:00:00 AM
Source: TMS_API.Services.BackgroundSchedulerService
```

**Meaning:**  
The nightly overdue toy check started successfully.

**Action required:**  
None.

---

### Normal Entry #2 — No overdue loans found

```text
[06:00:01 INF] No overdue toy loans found
Source: TMS_API.Services.OverdueToyService
```

**Meaning:**  
The overdue check completed and no overdue loans were found.

**Action required:**  
None.

---

## Error Log Entry Examples

### Error Entry #1 — Database connection failure during overdue check

```text
[06:00:01 ERR] Error occurred while checking overdue toys
Source: TMS_API.Services.OverdueToyService
Exception: Microsoft.Data.SqlClient.SqlException: Connection timed out
```

**Meaning:**  
The database was unreachable when the overdue check ran.

**Action required:**  
Check the database status, connection string, firewall rules, and Key Vault secrets.

---

### Error Entry #2 — Null reference in notification pipeline

```text
[06:00:00 ERR] Error occurred during scheduled overdue toy check
Source: TMS_API.Services.BackgroundSchedulerService
Exception: System.NullReferenceException: Object reference not set to an instance of an object.
```

**Meaning:**  
A required record is missing or incomplete, such as a student without a linked user account.

**Action required:**  
Check the ToyLoans table and confirm the Student–User relationship is intact.

##How the log file is formatted
![image.png](/.attachments/image-7f7accc7-a5f1-43f0-b556-d2129995d9db.png)