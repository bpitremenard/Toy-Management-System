# E3 — Health Check & Monitoring

## Production System Health Verification

This section explains how to verify that the Toy Management System is running correctly in production.

---

## Production URLs

Use the following production URLs to verify the live system:

| Application | Production URL |
|---|---|
| Student Web App | `https://team2app-csprodwww.cegep-heritage.qc.ca/Home/Login` |
| Teacher Web App | `https://team2app-csprodwww.cegep-heritage.qc.ca/Teacher/Home/Login` |

> Note: The API URL should be confirmed from the production deployment settings or Azure/IIS configuration before being listed here.

---

## Health Check Endpoints

### Student Web App Health Check

Open the Student Web App production URL:

```text
https://team2app-csprodwww.cegep-heritage.qc.ca/Home/Login
```

**Healthy Response Characteristics:**

- The page loads successfully
- The login page is displayed
- No application error message appears
- CSS and JavaScript load correctly
- The production URL is visible in the browser

---

### Teacher Web App Health Check

Open the Teacher Web App production URL:

```text
https://team2app-csprodwww.cegep-heritage.qc.ca/Teacher/Home/Login
```

**Healthy Response Characteristics:**

- The page loads successfully
- The teacher login page is displayed
- No application error message appears
- CSS and JavaScript load correctly
- The production URL is visible in the browser

---

### API Health Check

The API should be checked using the confirmed production API URL from the deployment configuration.

If Swagger is enabled in production, the API can be checked by opening the Swagger endpoint.

**Expected Swagger format:**

```text
<production-api-url>/swagger
```

**Healthy Response Characteristics:**

- Returns HTTP 200 OK status
- Displays Swagger UI with API documentation
- Shows available API endpoints
- No error messages or loading failures appear

> Insert screenshot here: API Swagger page or confirmed API health response.

---

## Database Connectivity Health Check

Since the applications use Entity Framework Core with Azure SQL Database, database health can be verified by completing actions that require saved data.

### Verification Methods

1. Log in to the Student or Teacher Web App.
2. Confirm that the toy list loads.
3. Confirm that reports load from the Teacher side.
4. Try a simple feature that requires database access, such as viewing toys or viewing student borrow history.

**Healthy Response Characteristics:**

- Toy information loads correctly
- Reports load correctly
- No timeout errors appear
- No database-related error messages appear

---

## Monitoring and Logging

### Application Logs

The system uses application logging to record normal activity and errors.

Logs should be checked when:

- The system does not load
- Users cannot log in
- Toys or reports do not appear
- A feature fails unexpectedly
- A deployment has recently occurred

### Azure App Service / Server Monitoring

The hosting environment provides monitoring information such as:

- HTTP server errors
- Requests and response times
- CPU usage
- Memory usage
- Application errors

### Verification Method

1. Check that the Student and Teacher production URLs load.
2. Check application logs for recent errors.
3. Verify that there are no repeated `Exception`, `ERR`, `Timeout`, or `Unauthorized` messages.
4. Confirm that data is loading from the database.

---

## Common Health Issues and Solutions

### Database Connectivity Issues

**Symptoms:**

- HTTP 500 errors
- Timeout exceptions
- Toys or reports do not load

**Checks:**

- Azure SQL Database status
- Connection string
- Key Vault secrets
- Network connectivity/firewall rules
- API/application logs

---

### Authentication Issues

**Symptoms:**

- Redirect loops
- Login fails
- 401 Unauthorized errors
- User logs in but cannot access the correct role

**Checks:**

- Azure AD app registration
- User account access
- Role assignment
- Client secrets
- Token validation configuration

---

### Application Performance Issues

**Symptoms:**

- Slow response times
- Pages take too long to load
- Requests timeout

**Checks:**

- App Service or server resource usage
- CPU and memory usage
- Database performance
- API response time

---

## Error Filtering in Logs

When checking logs, search for common error keywords.

```text
ERR
Error
Exception
Failed
Unable
Timeout
Unauthorized
SqlException
NullReferenceException
AuthenticationFailedException
CredentialUnavailableException
```

**Healthy Result:**

- No repeated errors appear in the newest logs
- Login pages load correctly
- Toy and report data loads correctly

**Unhealthy Result:**

- The same error appears repeatedly
- The application shows HTTP 500 errors
- Users cannot log in
- Data cannot be loaded or saved

