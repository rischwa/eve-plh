using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public class KillsBySystem
    {
        public long SolarSystemId { get; set; }
        public int ShipKillCount { get; set; }
        public int PodKillCount { get; set; }
        public int NpcKillCount { get; set; }
    }

    public class EveKillsService
    {
        // /map/Kills.xml.aspx

        public async Task<IDictionary<long, KillsBySystem>> GetKillsBySystemId()
        {
            return await ApiCache.GetCurrentData("killsBySystem/v1", GetKillsBySystem,x=>x);
        }

        private static CachedData<IDictionary<long, KillsBySystem>> GetKillsBySystem()
        {
            //TODO kann man auch verallgemeinern
            var doc =
                XDocumentWebRequester.RequestCcpApiXmlDocument(
                    "https://api.eveonline.com/map/Kills.xml.aspx");

            var killsBySystem =
                doc.Descendants("row")
                   .Select(
                       x =>
                       new KillsBySystem()
                       {
                           ShipKillCount = x.GetIntAttributeValue("shipKills"),
                           SolarSystemId = x.GetLongAttributeValue("solarSystemID"),
                           NpcKillCount = x.GetIntAttributeValue("factionKills"),
                           PodKillCount = x.GetIntAttributeValue("podKills")
                       })
                   .ToList();

            var cachedUntil = DateTime.ParseExact(doc.Descendants("cachedUntil").First().Value, "yyyy-MM-dd HH:mm:ss",
                                                  CultureInfo.InvariantCulture);

            return new CachedData<IDictionary<long, KillsBySystem>>
            {
                Value = killsBySystem.ToDictionary(x=>x.SolarSystemId, x=>x),
                CachedUntil = cachedUntil
            };
        }
    }
}
