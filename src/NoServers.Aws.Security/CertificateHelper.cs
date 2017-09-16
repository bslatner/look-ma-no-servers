using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace NoServers.Aws.Security
{
    public static class CertificateHelper
    {
        public static X509Certificate2 GetCertificateFromResource(Assembly assembly, string resourceName)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            var mem = new MemoryStream();
            stream.CopyTo(mem);
            var bytes = mem.ToArray();
            return new X509Certificate2(bytes);
        }

        public static X509Certificate2 GetCertificateFromFile(string path)
        {
            using (var file = File.Open(path, FileMode.Open))
            {
                return GetCertificateFromStream(file);
            }
        }

        public static X509Certificate2 GetCertificateFromStream(Stream stream)
        {
            var mem = new MemoryStream();
            stream.CopyTo(mem);
            var bytes = mem.ToArray();
            return new X509Certificate2(bytes);
        }
    }
}