# ADR 2 — Use Repository Pattern with Service Layer
**Status:** Accepted  

## Context
As the application grew, direct data access inside controllers became difficult to maintain and test. The team needed a structured way to separate concerns between data access, business logic, and presentation.

## Options considered
- **Option 1: Direct Entity Framework Core usage in controllers**  
  - Advantage: Faster to implement with fewer layers.  
  - Drawback: Poor separation of concerns and difficult unit testing.  

- **Option 2: Repository pattern with service layer and interfaces**  
  - Advantage: Clear separation of concerns and improved testability.  
  - Drawback: Increased complexity and more boilerplate code.  

## Decision
The team implemented a repository pattern combined with a service layer, with interfaces used for abstraction.

## Rationale
This approach allowed business logic to reside in services while repositories handled database interactions. Interfaces enable mocking during testing, improving test coverage and maintainability.

## Consequences
- **Easier:**  
  - Unit testing using mocks  
  - Maintaining and scaling business logic  

- **Harder:**  
  - Increased number of classes and interfaces  
  - More initial setup time  

- **Technical debt / risk:**  
  - Over-engineering for smaller features  

Controller Using Services:

![Picture1.png](/.attachments/Picture1-6fd987f3-407d-4bfe-9ad5-53797a958b31.png)

Interface for Service:

![Picture1.png](/.attachments/Picture1-b9e595ae-a0ec-4ac4-ad78-3d781a18cc2d.png)

Service Implementation:

![Picture1.png](/.attachments/Picture1-99005c10-6f62-47c9-8543-66cb85d91da9.png)