
# How to Run the Tests
File Structure
```
Tests/
 ├── Controller/
 │    ├── ToyControllerTest.cs
 ├── Model/
 │    ├── ToyModelTest.cs
 ├── Service/
 │    ├── ToyServiceTest.cs
```
# Unit Tests

Run all unit tests using the .NET CLI:
```bash
dotnet test
```
###OR
- Run Tests through the IDE UI
# E2E Tests

Install Playwright browsers:
```bash
npx playwright install
```

Set required environment variables:
```bash
SET TestUsername=email@example.com
SET TestPassword=password
```
Run all E2E Tests
```bash
dotnet test --filter Category=E2E
```
###OR
- Download Packages through the UI
- Run Tests through the IDE UI