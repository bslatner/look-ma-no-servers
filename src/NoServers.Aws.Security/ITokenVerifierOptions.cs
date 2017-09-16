using System.Security.Cryptography.X509Certificates;

namespace NoServers.Aws.Security
{
    public interface ITokenVerifierOptions
    {
        string ValidIssuer { get; }
        string ValidAudience { get; }
        X509Certificate2 Certificate { get; }
    }
}