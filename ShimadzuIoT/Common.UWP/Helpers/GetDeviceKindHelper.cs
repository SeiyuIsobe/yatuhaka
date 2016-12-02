using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public static class GetDeviceKindHelper
    {
        public static string GetDeviceKind(string input)
        {
            Regex rgx = new Regex(@"_DK\w*_");

            MatchCollection matches = rgx.Matches(input);
            if (matches.Count > 0)
            {
                // 前後の区切り文字_を消して、DK以降の文字列を返す
                return matches[0].Value.Replace("_", "").Substring(2);
            }

            return string.Empty;
        }
    }
}
