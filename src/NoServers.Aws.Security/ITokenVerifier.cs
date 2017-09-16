using Microsoft.IdentityModel.Tokens;

namespace NoServers.Aws.Security
{
    public interface ITokenVerifier
    {
        string GetVerifiedUserFromToken(string tokenString);
    }
}