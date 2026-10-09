using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Identity.Web;

namespace TMS_API.Helpers
{
    public class TokenCredentialWrapper : TokenCredential
    {
        private readonly ITokenAcquisition _tokenAcquisition;

        public TokenCredentialWrapper(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = _tokenAcquisition
                .GetAccessTokenForUserAsync(requestContext.Scopes)
                .GetAwaiter()
                .GetResult();

            return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(30));
        }

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(requestContext.Scopes);
            return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(30));
        }
    }
}
