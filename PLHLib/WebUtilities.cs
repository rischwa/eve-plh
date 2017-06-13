using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace PLHLib
{
    public static class WebUtilities
    {
        private static readonly string PLH_USER_AGENT;

        static WebUtilities()
        {
            PLH_USER_AGENT = "EVE PLH";
        }
            
        public static string GetHttpGetResponseFrom(string url, TimeSpan timeout = default(TimeSpan))
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = PLH_USER_AGENT;
            if (timeout != default(TimeSpan))
            {
                request.Timeout = (int)timeout.TotalMilliseconds;
            }

            using (var response = request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if (stream == null)
                {
                    return null;
                }
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public static T GetFromHttpJson<T>(string url, TimeSpan timeout = default(TimeSpan))
        {
            var httpResponse = GetHttpGetResponseFrom(url);
            return JsonConvert.DeserializeObject<T>(httpResponse);
        }
    }
}
