
## How to Create New Unit Tests

Below is an example using the Arrange / Act / Assert pattern:
```C
[Test]
public void AddToy_Should_Add_Toy_Successfully()
{
    // Arrange: Set up objects and dependencies
    var service = new ToyService();
    var toy = new Toy { Name = "Test Toy" };

    // Act: Call the method being tested
    service.AddToy(toy);

    // Assert: Verify the expected outcome
    Assert.IsTrue(service.GetAllToys().Any(t => t.Name == "Test Toy"));
}
```

###Steps to Add a New Unit Test
- Open the appropriate test file (e.g., ToyServiceTests.cs)
- Add a new [Test] method
- Follow Arrange / Act / Assert
- Use naming format: ```MethodName_Should_ExpectedBehavior```
- Run dotnet test to verify