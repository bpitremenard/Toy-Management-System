# ADR 3 — Use Playwright and xUnit for Testing Across Layers
**Status:** Accepted  

## Context
The project required both unit testing and end-to-end validation to ensure reliability across different layers (controllers, services, UI). The team needed a consistent testing strategy.

## Options considered
- **Option 1: Only unit testing (xUnit)**  
  - Advantage: Fast execution and easy to maintain.  
  - Drawback: Does not validate UI or full workflows.  

- **Option 2: Combine xUnit with Playwright for end-to-end testing**  
  - Advantage: Covers both backend logic and user interactions.  
  - Drawback: Slower tests and more complex setup.  

## Decision
The team used xUnit for unit testing and Playwright for end-to-end testing, applying tests across all layers.

## Rationale
This combination ensured both correctness of individual components and validation of real user workflows. Playwright allowed simulation of real browser interactions, which was critical for validating the reservation flow.

## Consequences
- **Easier:**  
  - Detecting issues across the full application stack  
  - Ensuring UI workflows function correctly  

- **Harder:**  
  - Maintaining Playwright tests  
  - Longer execution times in pipelines  

- **Technical debt / risk:**  
  - Flaky UI tests due to timing or environment issues  

Xunit:

![Picture1.png](/.attachments/Picture1-3f1d2b61-c5bc-4bb5-b976-54c2043e65e3.png)

Playwright:

![Picture1.png](/.attachments/Picture1-3d380cc5-8b0f-44b1-8335-3b11242fcd19.png)