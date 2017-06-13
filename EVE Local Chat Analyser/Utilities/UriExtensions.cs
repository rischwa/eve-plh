using System;
using Newtonsoft.Json;
using PLHLib;

namespace EveLocalChatAnalyser.Utilities
{
    public static class UriExtensions
    {
        public static string GetHttpResponse(this Uri uri)
        {
            return WebUtilities.GetHttpGetResponseFrom(uri.ToString());
        }

        public static T GetJsonResponse<T>(this Uri uri)
        {
            return JsonConvert.DeserializeObject<T>(uri.GetHttpResponse());
        }
    }
}