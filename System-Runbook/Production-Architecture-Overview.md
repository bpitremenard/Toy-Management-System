# Production Architecture Overview

## Architecture Diagram

![image.png](/.attachments/image-9584d3e6-63ad-4985-abd8-701de7a863b1.png)

---

## Component Descriptions

### Web Applications — TMS Student & TMS Teacher

The system consists of two ASP.NET Core 8.0 web applications built using the Model-View-Controller pattern.

The Student application provides students with functionality to browse available toys, request loans, and manage their borrowed items.

The Teacher application offers toy management capabilities including inventory management, loan approvals, reporting, and system administration.

The applications utilize Entity Framework Core for data access and follow dependency injection patterns for maintainable code architecture.

---

### API Gateway — TMS API

The RESTful API serves as the central data access layer for both web applications. It is built on ASP.NET Core 8.0 with JWT Bearer authentication.

It exposes endpoints for CRUD operations on toys, students, teachers, loans, and notifications while implementing comprehensive business logic validation.

The API uses AutoMapper for object-to-object mapping between domain models and DTOs, and Swagger/OpenAPI for interactive API documentation.

Security is enforced through Azure AD integration with role-based authorization, and all operations are logged using Serilog for audit trails.

The API follows repository pattern architecture with separate repositories for each entity type.

---

### Azure SQL Database

The system's primary data store is an Azure SQL Database named `K40_TMS` hosted on Microsoft's cloud platform.

The database contains six main tables:

- Users
- Students
- Teachers
- Toys
- ToyLoans
- Notifications

The database uses foreign key relationships to ensure data integrity. It also includes triggers for automated routing and enforces referential integrity through cascading rules.

Connection strings are securely stored in Azure Key Vault and accessed through managed identities for enhanced security.

---

### Azure Key Vault

Azure Key Vault serves as the centralized secrets management solution for the entire production system.

It stores sensitive configuration including:

- Database connection strings
- API keys
- Client secrets
- Other credentials needed at runtime

The system uses Azure Managed Identity for authentication to Key Vault. This removes the need to store credentials directly in application code or configuration files.

Both development and production environments have separate Key Vault instances:

- `cs-app-keyvault`
- `cs-app-prodkeyvault`

This helps ensure proper environment isolation.

The Key Vault integration provides automatic secret rotation capabilities and audit logging of all access attempts.

---

### Azure Active Directory

Azure AD provides the identity and access management foundation for the Toy Management System.

All user authentication flows through Azure AD using OAuth 2.0 and OpenID Connect protocols. The system is configured to use the `cegepheritagecs.onmicrosoft.com` tenant.

Users are authenticated through JWT tokens issued by Azure AD. The API validates token signatures and claims on each request.

The system uses Microsoft Graph API to retrieve user profile information, group memberships, and organizational hierarchy data.

Azure AD integration enables single sign-on and centralized user management across all applications.

---

### Hosting Environment — Azure App Service

All application components are deployed to Azure App Service.

The system uses separate App Service instances for:

- Development: `csdev`
- Testing: `cstest`
- Production: `csprod`

Azure App Service handles:

- Application deployment
- Load balancing
- SSL termination
- Health monitoring
- Application logging
- Monitoring dashboards
- Built-in CI/CD integration

Managed identities are configured at the App Service level to allow secure access to other Azure resources without storing credentials.

---

### Microsoft Graph API

The Microsoft Graph API serves as the external service integration point for retrieving organizational data from Microsoft 365.

The system uses Graph API to fetch:

- User details
- Department information
- Organizational hierarchy
- Group membership information

Integration is handled through the `Microsoft.Graph` NuGet package with authentication using Azure AD application permissions.

This integration enables features such as automatic user provisioning and role assignment based on group memberships.

---

## Operational Notes

If the production system has issues, the main components to check are:

1. Web Applications — TMS Student and TMS Teacher
2. TMS API
3. Azure SQL Database
4. Azure Key Vault
5. Azure Active Directory
6. Azure App Service
7. Microsoft Graph API

If users can log in but data does not load, check the API and database first.

If users cannot log in, check Azure Active Directory and account permissions first.

If configuration or secret-related errors appear, check Azure Key Vault and Managed Identity access.