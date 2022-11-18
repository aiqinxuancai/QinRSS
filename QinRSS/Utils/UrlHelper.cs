using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QinRSS.Utils
{
    internal class UrlHelper
    {
        public static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
        }

        public static string CombineUriToString(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri).ToString();
        }
    }
}
