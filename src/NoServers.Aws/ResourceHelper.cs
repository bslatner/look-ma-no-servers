using System.IO;
using System.Reflection;

namespace NoServers.Aws
{
    public static class ResourceHelper
    {
        public static string GetResourceAsString(Assembly assembly, string name)
        {
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null) return null;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}