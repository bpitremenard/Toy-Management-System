using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace TMSStudent.Controllers;

/// <summary>
/// Internal endpoint used by E2E tests to retrieve the current user's
/// delegated access token for the API. Only available in Development.
/// </summary>
[Authorize]
[ApiController]
[Route("internal")]
public class InternalController : ControllerBase
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public InternalController(
        ITokenAcquisition tokenAcquisition,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _tokenAcquisition = tokenAcquisition;
        _configuration    = configuration;
        _env              = env;
    }

    /// <summary>
    /// Returns the current user's Bearer token for the Toys API.
    /// Only accessible in Development environment.
    /// </summary>
    [HttpGet("token")]
    public async Task<IActionResult> GetApiToken()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var scope = _configuration["AzureAdTms:ApiScope"]
            ?? throw new InvalidOperationException(
                        "AzureAdTms:ApiScope is not configured in appsettings.");

        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { scope });
        return Ok(token);
    }
}
