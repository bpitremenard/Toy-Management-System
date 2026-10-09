# ADR 5 — CI/CD Pipeline with Automated Testing and Branch-Based Deployment
**Status:** Accepted  

## Context
The team needed a reliable way to build, test, and deploy the application automatically. Manual deployment was error-prone and inefficient.

## Options considered
- **Option 1: Manual deployment**  
  - Advantage: Simple and requires no setup.  
  - Drawback: High risk of human error and inconsistent deployments.  

- **Option 2: Automated CI/CD pipeline with branch triggers**  
  - Advantage: Consistent, repeatable deployments with automated testing.  
  - Drawback: Requires setup and maintenance of pipeline configuration.  

## Decision
The team implemented a CI/CD pipeline with branch-based triggers, where builds automatically run tests and successful builds trigger deployment.

## Rationale
Automating the pipeline ensured that every code change was validated through tests before deployment. Branch-based triggers allowed controlled promotion of code between environments.

## Consequences
- **Easier:**  
  - Consistent deployments  
  - Early detection of issues through automated testing  

- **Harder:**  
  - Pipeline configuration and debugging failures  
  - Managing deployment environments  

- **Technical debt / risk:**  
  - Pipeline maintenance overhead  

Pipeline Build:

![Picture1.png](/.attachments/Picture1-1647dbee-b988-4db2-a800-5dabafb68673.png)

Release Run:

![Picture1.png](/.attachments/Picture1-cb87d454-c57f-4e4d-b862-5cab8f706b57.png)

Dev, Prod, Test Releases:

![Picture1.png](/.attachments/Picture1-fdec2011-a04d-48b4-94f1-0a827a71ae36.png)

Dev, Prod, Test Pipelines:

![Picture1.png](/.attachments/Picture1-26f32dc5-59f7-4e77-ad24-32d2ca3bdd12.png)