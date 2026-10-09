# Restart Procedures

This section explains how to restart each production component of the Toy Management System.

Restart should only be done when the system is not responding, deployment has completed but changes are not visible, or logs show repeated application startup/runtime errors.

---

## Before Restarting

Before restarting, check:

1. Whether the issue affects only one user or all users.
2. Whether the Student app, Teacher app, or API is affected.
3. Whether the production URLs load.
4. Whether recent deployment changes were made.
5. Whether logs show repeated errors.

---

# Restart Student Web Application

## Steps

1. Open the Azure Portal or production hosting console.
2. Navigate to the Student Web Application resource.
3. Confirm you are in the production resource.
4. Click **Restart**.
5. Wait for the restart to complete.
6. Open the Student production URL:
   `https://team2app-csprodwww.cegep-heritage.qc.ca/Home/Login`
7. Confirm that the login page loads.

## Expected Result

- Student application restarts successfully.
- Student login page loads.
- Students can sign in again.

> Insert screenshot here: Student app restart button  
> Insert screenshot here: Student app production login page after restart

---

# Restart Teacher Web Application

## Steps

1. Open the Azure Portal or production hosting console.
2. Navigate to the Teacher Web Application resource.
3. Confirm you are in the production resource.
4. Click **Restart**.
5. Wait for the restart to complete.
6. Open the Teacher production URL:
   `https://team2app-csprodwww.cegep-heritage.qc.ca/Teacher/Home/Login`
7. Confirm that the login page loads.

## Expected Result

- Teacher application restarts successfully.
- Teacher login page loads.
- Teachers can sign in again.

> Insert screenshot here: Teacher app restart button  
> Insert screenshot here: Teacher app production login page after restart

---

# Restart API

## Steps

1. Open the Azure Portal or production hosting console.
2. Navigate to the TMS API production resource.
3. Confirm you are in the production resource.
4. Click **Restart**.
5. Wait for the restart to complete.
6. Open the API health check or Swagger page if available:
   `https://csprod-tms-api.azurewebsites.net/swagger`
7. Confirm that the page loads without server errors.

## Expected Result

- API restarts successfully.
- Student and Teacher applications can load data.
- API health check or Swagger page loads.

> Insert screenshot here: API restart button  
> Insert screenshot here: API Swagger page after restart

---

# Restart Database / Database Recovery

The database should not normally be restarted by the application support team. If the database is unavailable, check the Azure SQL Database status first.

## Steps

1. Open Azure Portal.
2. Navigate to the production Azure SQL Database.
3. Check the database status.
4. Check recent errors or service health alerts.
5. If data must be restored, use the application’s Backup and Restore feature if appropriate.
6. If the database appears unavailable or corrupted, escalate to the instructor or system administrator.

## Expected Result

- Database status is online.
- Application can load toy and report data again.

> Insert screenshot here: Azure SQL Database status page

---

# Restart After Deployment

After restarting any component, verify:

1. Student URL loads.
2. Teacher URL loads.
3. A user can log in.
4. Toy list loads.
5. Reports page loads.
6. No new errors appear in logs.