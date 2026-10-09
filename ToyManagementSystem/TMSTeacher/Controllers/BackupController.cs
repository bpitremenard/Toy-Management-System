using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using TMS_SharedLibrary.Audit.Enums;
using TMSTeacher.Services;

[Authorize(Roles = "Teacher")]
public class BackupController : Controller
{
    private readonly string _backupFolder;
    private readonly string _connectionString;
    private readonly string _dbName;
    private readonly IUserAPIService _userAPIService;
    private static readonly Dictionary<string, string> _backupStatus = new();
    private static readonly Dictionary<string, string> _restoreStatus = new();

    public BackupController(IConfiguration configuration, IWebHostEnvironment env, IUserAPIService userAPIService)
    {
        _connectionString = configuration.GetConnectionString("EaglesConnection");
        _dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
        _backupFolder = Path.Combine(env.ContentRootPath, "App_Data", "Backups");
        Directory.CreateDirectory(_backupFolder);
        _userAPIService = userAPIService;
    }

    [HttpGet]
    public ActionResult Index() => View();

    [HttpGet]
    public ActionResult GetBackups()
    {
        var files = Directory.GetFiles(_backupFolder, "*.bacpac")
                             .Select(Path.GetFileName)
                             .OrderByDescending(f => f)
                             .ToList();
        return Json(files);
    }

    [HttpPost]
    public ActionResult CreateBackup()
    {
        try
        {
            string fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bacpac";
            string backupPath = Path.Combine(_backupFolder, fileName);
            string jobId = Guid.NewGuid().ToString();
            _backupStatus[jobId] = "running";

            var connectionString = _connectionString;
            var dbName = _dbName;
            var userAPIService = _userAPIService;

            Task.Run(async () =>
            {
                try
                {
                    var dacServices = new DacServices(connectionString);
                    dacServices.ExportBacpac(backupPath, dbName);

                    await userAPIService.LogAuditAsync(AuditAction.Create, "Backup", 0, $"Created backup: {fileName}");
                    _backupStatus[jobId] = "success:" + fileName;
                }
                catch (Exception ex)
                {
                    _backupStatus[jobId] = "error:" + ex.Message;
                }
            });

            return Json(new { success = true, jobId = jobId });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Backup failed: " + ex.Message });
        }
    }

    [HttpGet]
    public ActionResult BackupStatus(string jobId)
    {
        if (!_backupStatus.TryGetValue(jobId, out var status))
            return Json(new { status = "unknown" });

        if (status == "running")
            return Json(new { status = "running" });

        if (status.StartsWith("success:"))
        {
            string fileName = status.Substring("success:".Length);
            _backupStatus.Remove(jobId);
            return Json(new { status = "success", message = $"Backup created: {fileName}", fileName = fileName });
        }

        string error = status.Substring("error:".Length);
        _backupStatus.Remove(jobId);
        return Json(new { status = "error", message = "Backup failed: " + error });
    }

    [HttpPost]
    public ActionResult RestoreBackup(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName.Contains("..") ||
                fileName.Contains("/") ||
                fileName.Contains("\\"))
                return Json(new { success = false, message = "Invalid file name." });

            string backupPath = Path.Combine(_backupFolder, fileName);

            if (!System.IO.File.Exists(backupPath))
                return Json(new { success = false, message = "Backup file not found." });

            string jobId = Guid.NewGuid().ToString();
            _restoreStatus[jobId] = "running";

            var connectionString = _connectionString;
            var dbName = _dbName;
            var userAPIService = _userAPIService;

            Task.Run(async () =>
            {
                try
                {
                    // Step 1: Drop all tables so database is empty
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string dropAll = @"
                        DECLARE @sql NVARCHAR(MAX) = '';
                        SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
                        FROM sys.foreign_keys;
                        EXEC sp_executesql @sql;

                        SET @sql = '';
                        SELECT @sql += 'DROP TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + ';'
                        FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
                        EXEC sp_executesql @sql;";

                        using var cmd = new SqlCommand(dropAll, conn) { CommandTimeout = 120 };
                        cmd.ExecuteNonQuery();
                    }

                    // Step 2: Import into now-empty database
                    var bacpac = BacPackage.Load(backupPath);
                    var dacServices = new DacServices(connectionString);
                    var importOptions = new DacImportOptions { ImportContributors = "" };
                    dacServices.ImportBacpac(bacpac, dbName, importOptions);

                    await userAPIService.LogAuditAsync(AuditAction.Update, "Backup", 0, $"Restored backup: {fileName}");
                    _restoreStatus[jobId] = "success";
                }
                catch (Exception ex)
                {
                    _restoreStatus[jobId] = "error:" + ex.Message;
                }
            });

            return Json(new { success = true, jobId = jobId });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Restore failed: " + ex.Message });
        }
    }

    [HttpGet]
    public ActionResult RestoreStatus(string jobId)
    {
        if (!_restoreStatus.TryGetValue(jobId, out var status))
            return Json(new { status = "unknown" });

        if (status == "running")
            return Json(new { status = "running" });

        if (status == "success")
        {
            _restoreStatus.Remove(jobId);
            return Json(new { status = "success", message = "Database restored successfully." });
        }

        string error = status.Substring("error:".Length);
        _restoreStatus.Remove(jobId);
        return Json(new { status = "error", message = "Restore failed: " + error });
    }
}