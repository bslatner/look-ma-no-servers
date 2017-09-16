using System.Collections.Generic;
using System.Linq;

namespace NoServers.Aws
{
    public static class HeaderHelper
    {
        public static Dictionary<string, string> AddContentType(this Dictionary<string, string> headers, string contentType)
        {
            headers.Add("Content-Type", contentType);
            return headers;
        }

        public static Dictionary<string, string> AddCorsOrigin(this Dictionary<string, string> headers)
        {
            headers.Add("Access-Control-Allow-Origin", "*");
            return headers;
        }

        public static Dictionary<string, string> AddCorsForOptionsVerb(this Dictionary<string, string> headers, IEnumerable<string> allowedMethods)
        {
            headers.AddCorsOrigin();
            headers.Add("X-Requested-With", "*");
            headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,x-requested-with");
            headers.Add("Access-Control-Allow-Methods", string.Join(",", allowedMethods));
            return headers;
        }
    }
}