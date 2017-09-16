using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace NoServers.Aws.Security
{
    public class TokenVerifier : ITokenVerifier
    {
        private readonly ITokenVerifierOptions _Options;

        public TokenVerifier(ITokenVerifierOptions options)
        {
            _Options = options;
        }

        public string GetVerifiedUserFromToken(string tokenString)
        {
            var securityKey = new X509SecurityKey(_Options.Certificate);
            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _Options.ValidIssuer,
                ValidAudience = _Options.ValidAudience,
                IssuerSigningKey = securityKey,

                // Why is this necessary?
                // Something to do with .NET Core 1.0
                // See https://github.com/IdentityServer/IdentityServer3/issues/3040
                IssuerSigningKeyResolver = (token, token1, kid, parameters) => 
                    new List<X509SecurityKey> {securityKey}
            };

            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(
                tokenString, 
                validationParameters, 
                out var securityToken);
            var jwtSecurityToken = (JwtSecurityToken) securityToken;
            return jwtSecurityToken.Subject;
        }
    }
}
