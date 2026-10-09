using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace TMSTeacher.Services
{
    public class GraphAPIService : IGraphAPIService
    {
        private readonly GraphServiceClient _appGraphClient;
        private readonly ILogger<GraphAPIService> _logger;

        public GraphAPIService(IConfiguration configuration, ILogger<GraphAPIService> logger)
        {
            _logger = logger;

            // Use app-level ClientSecretCredential instead of the delegated user token.
            // This works regardless of whether the logged-in teacher is a school or
            // personal (guest) account, because it authenticates as the application
            // itself using the Application permission User.Read.All.
            var tenantId = configuration["AzureAdTms:TenantId"]
                ?? throw new InvalidOperationException("AzureAdTms:TenantId is not configured.");
            var clientId = configuration["AzureAdTms:ClientId"]
                ?? throw new InvalidOperationException("AzureAdTms:ClientId is not configured.");
            var clientSecret = configuration["AzureAdTms:ClientSecret"]
                ?? throw new InvalidOperationException("AzureAdTms:ClientSecret is not configured.");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _appGraphClient = new GraphServiceClient(credential,
                new[] { "https://graph.microsoft.com/.default" });
        }

        public async Task<StudentGraphInfo?> GetStudentInfoByOidAsync(string oid)
        {
            try
            {
                var user = await _appGraphClient.Users[oid].GetAsync(config =>
                {
                    config.QueryParameters.Select =
                        new[] { "givenName", "surname", "userPrincipalName", "displayName" };
                });

                if (user == null)
                {
                    _logger.LogWarning("Graph returned null for OID {Oid}", oid);
                    return null;
                }

                return new StudentGraphInfo
                {
                    FirstName = user.GivenName,
                    LastName = user.Surname,
                    Email = user.UserPrincipalName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Graph info for OID {Oid}", oid);
                return null;
            }
        }
    }

    // Simple data carrier — add this to the same file or its own file in Services/
    public record StudentGraphInfo
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Email { get; init; }
    }
}