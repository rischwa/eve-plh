using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public class EveJumpsService
    {

        public async Task<IDictionary<long, int>> GetJumpsBySystemId()
        {
            return await ApiCache.GetCurrentData("jumpsBySystem/v1", GetJumpsBySystemIdInternal,x=>x);
        }

        private static CachedData<IDictionary<long, int>> GetJumpsBySystemIdInternal()
        {
            var doc =
                XDocumentWebRequester.RequestCcpApiXmlDocument(
                    "https://api.eveonline.com/map/Jumps.xml.aspx");

            var jumpsBySystem =
                doc.Descendants("row").ToDictionary(x => x.GetLongAttributeValue("solarSystemID"), x=>x.GetIntAttributeValue("shipJumps"));

            var cachedUntil = DateTime.ParseExact(doc.Descendants("cachedUntil").First().Value, "yyyy-MM-dd HH:mm:ss",
                                                  CultureInfo.InvariantCulture);

            return new CachedData<IDictionary<long, int>>
            {
                Value = jumpsBySystem,
                CachedUntil = cachedUntil
            };
        }
    }
}
