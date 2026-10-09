
##How to Add a New E2E Test
File Structure
```
Tests/
 ├── E2E/
 │    ├── TC_Add_Toy.cs
 │    ├── TC_Edit_Toy.cs
 │    ├── TC_Delete_Toy.cs
 │    └── BaseTest.cs
```
Example Test
```C
[Test]
[Category("E2E")]
public async Task TC_Add_Toy_Success()
{
    await _page.GotoAsync($"{BaseUrl}/Toy");

    await _page.ClickAsync("text=Add Toy");
    await _page.FillAsync("#Name", "E2E Toy");

    await _page.ClickAsync("button[type='submit']");

    var result = await _page.Locator("text=E2E Toy").IsVisibleAsync();

    Assert.IsTrue(result);
}
```
Key Concepts:
- Uses Playwright for browser automation
- _page comes from the base test class
- Authentication state is preloaded
- Tests simulate real user behavior

###Steps to Add a New E2E Test
- Create a new file (e.g., TC_NewFeature.cs)
- Add [Test] and [Category("E2E")]
- Copy an existing test as a template
- Simulate user actions (click, fill, submit)
- Add assertions to verify results
- Run tests to confirm it passes