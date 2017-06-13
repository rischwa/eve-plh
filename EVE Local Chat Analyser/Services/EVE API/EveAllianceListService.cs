using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public class Alliance
    {
        public string Name { get; set; }
        public long Id { get; set; }

        public string Ticker { get; set; }
    }

    public class EveAllianceListService
    {
        public async Task<IDictionary<long, Alliance>> GetAlliancesByAllianceId()
        {
            return await ApiCache.GetCurrentData("allianceInfo/v1", GetAlliancesSync, x=>x);
        }

        private static CachedData<IDictionary<long, Alliance>> GetAlliancesSync()
        {
            //without version=1 member corps are also included
            var doc =
                XDocumentWebRequester.RequestCcpApiXmlDocument(
                    "https://api.eveonline.com/eve/AllianceList.xml.aspx?version=1");

            var alliances =
                doc.Descendants("row")
                   .Select(
                       x =>
                       new Alliance
                       {
                           Id = x.GetLongAttributeValue("allianceID"),
                           Name = x.GetAttributeValue("name"),
                           Ticker = x.GetAttributeValue("shortName")
                       })
                   .ToList();

            var cachedUntil = DateTime.ParseExact(doc.Descendants("cachedUntil").First().Value, "yyyy-MM-dd HH:mm:ss",
                                                  CultureInfo.InvariantCulture);

            return new CachedData<IDictionary<long, Alliance>>
            {
                Value = alliances.ToDictionary(x=>x.Id, y=>y),
                CachedUntil = cachedUntil
            };
        }
    }
}