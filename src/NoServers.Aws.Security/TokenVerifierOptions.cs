using System.Security.Cryptography.X509Certificates;

namespace NoServers.Aws.Security
{
    public class TokenVerifierOptions : ITokenVerifierOptions
    {
        public string ValidIssuer { get; }
        public string ValidAudience { get; }
        public X509Certificate2 Certificate { get; }

        public TokenVerifierOptions(string validIssuer, string validAudience, X509Certificate2 certificate)
        {
            ValidIssuer = validIssuer;
            ValidAudience = validAudience;
            Certificate = certificate;
        }
    }
}