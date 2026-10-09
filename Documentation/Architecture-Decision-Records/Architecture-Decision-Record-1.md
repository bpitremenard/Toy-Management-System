# ADR 1 — Use Centralized Shared Library for Models
**Status:** Accepted  

## Context
During early development, both the Teacher and Student applications required access to the same domain models (e.g., Toy). Initially, models were duplicated across projects, which introduced inconsistencies and increased maintenance effort when changes were required. The team needed a way to ensure consistency and reduce duplication while keeping projects modular.

## Options considered
- **Option 1: Duplicate models in each project**  
  - Advantage: Simple to implement initially with no additional dependencies.  
  - Drawback: High risk of inconsistency and duplicated maintenance effort.  

- **Option 2: Create a shared class library for models**  
  - Advantage: Single source of truth for all domain models, and cleaner code base infrastructure.  
  - Drawback: Introduces dependency between projects and requires proper version control.  

## Decision
The team created a shared class library project to store all domain models and reused it across Teacher, Student, and API projects.

## Rationale
This approach ensured that all applications referenced the same model definitions, eliminating duplication and reducing the likelihood of mismatched properties or validation logic. It also simplified updates since changes only needed to be made in one place.

## Consequences
- **Easier:**  
  - Maintaining consistency across applications  
  - Updating models without duplicating effort  

- **Harder:**  
  - Managing dependencies between projects  
  - Coordinating changes that affect multiple layers  

- **Technical debt / risk:**  
  - Tight coupling between projects and the shared library  

Project Structure:

![Picture1.png](/.attachments/Picture1-0c90b690-bd6d-4c5c-8fa5-0e845ddafa08.png)

Controller using Shared Library:

![Picture1.png](/.attachments/Picture1-4f27c77c-9b3f-48b6-b33e-a5fad635d88c.png)