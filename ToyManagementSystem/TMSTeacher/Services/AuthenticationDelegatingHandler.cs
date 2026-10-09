using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;

namespace TMSTeacher.Services
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationDelegatingHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationDelegatingHandler(
            ITokenAcquisition tokenAcquisition,
            IConfiguration configuration,
            ILogger<AuthenticationDelegatingHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("No authenticated user; sending request without token.");
                return await base.SendAsync(request, cancellationToken);
            }

            try
            {
                var apiClientId = _configuration["Api:ClientId"];
                var scope = $"api://{apiClientId}/User_Access";

                _logger.LogInformation($"Acquiring token for scope: {scope}");

                // Only pass scopes — do NOT pass ClaimsPrincipal
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
                    new[] { scope }
                );

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                _logger.LogInformation("Token added to request successfully.");
            }
            catch (MsalUiRequiredException msalEx)
            {
                _logger.LogWarning(msalEx, "Silent token acquisition failed. User may need to sign in again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error acquiring token: {ex.Message}");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}