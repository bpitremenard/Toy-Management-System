# ADR 4 — Use Azure Key Vault for Secrets Management
**Status:** Accepted  

## Context
The application required secure handling of sensitive data such as connection strings and API keys. Storing secrets in configuration files posed a security risk, especially in a team and CI/CD environment.

## Options considered
- **Option 1: Store secrets in appsettings.json**  
  - Advantage: Simple and easy to configure.  
  - Drawback: Security risk and not suitable for production.  

- **Option 2: Use Azure Key Vault with Managed Identity**  
  - Advantage: Secure, centralized secret management.  
  - Drawback: More complex setup and requires Azure configuration.  

## Decision
The team used Azure Key Vault to manage all sensitive configuration values.

## Rationale
Azure Key Vault provides secure storage, access control, and integration with Azure services. It eliminates the need to store secrets in code or configuration files and supports secure access through managed identities.

## Consequences
- **Easier:**  
  - Secure handling of secrets  
  - Managing secrets across environments  

- **Harder:**  
  - Initial setup and configuration  
  - Debugging issues related to access permissions  

- **Technical debt / risk:**  
  - Dependency on Azure infrastructure  

Appsettings not using sensitive information:

![Picture1.png](/.attachments/Picture1-557e9456-3628-4e0f-bae3-cc88b5b0985c.png)

User secrets:

![Picture1.png](/.attachments/Picture1-7457d31d-6891-4109-9846-64e957817d5d.png)

Keyvault values:

![Picture1.png](/.attachments/Picture1-99aad84a-f92a-4e32-a8c7-0a9b8bd26412.png)