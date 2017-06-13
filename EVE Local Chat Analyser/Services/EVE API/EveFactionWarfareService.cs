using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public class FactionWarfareOccupancy
    {
        public long OwningFactionId { get; set; }
        public long SolarSystemId { get; set; }
        public long OccupyingFactionId { get; set; }
        public string OccupyingFaction { get; set; }
        public bool IsContested { get; set; }
    }

    public class EveFactionWarfareService
    {
        //TODO theoretisch status von alten updaten, wenn neue daten da sind
        public async Task<IDictionary<long, FactionWarfareOccupancy>> GetFactionWarfareOccupancyBySystemId()
        {
            return await ApiCache.GetCurrentData("factionWarfare/v1", GetFactionWarefeStatsSync,x=>x);
        }

        private static CachedData<IDictionary<long, FactionWarfareOccupancy>> GetFactionWarefeStatsSync()
        {
            //TODO kann man auch verallgemeinern
            var doc =
                XDocumentWebRequester.RequestCcpApiXmlDocument(
                    "https://api.eveonline.com/map/FacWarSystems.xml.aspx");

            var fwStats =
                doc.Descendants("row")
                   .Select(
                       x =>
                       new FactionWarfareOccupancy
                       {
                           OwningFactionId = x.GetLongAttributeValue("owningFactionID"),
                           SolarSystemId = x.GetLongAttributeValue("solarSystemID"),
                           OccupyingFactionId = x.GetLongAttributeValue("occupyingFactionID"),
                           OccupyingFaction = x.GetAttributeValue("occupyingFactionName"),
                           IsContested = x.GetBoolAttributeValue("contested")
                       })
                   .ToList();

            var cachedUntil = DateTime.ParseExact(doc.Descendants("cachedUntil").First().Value, "yyyy-MM-dd HH:mm:ss",
                                                  CultureInfo.InvariantCulture);

            return new CachedData<IDictionary<long,FactionWarfareOccupancy> >
            {
                Value = fwStats.ToDictionary(x => x.SolarSystemId, y => y),
                CachedUntil = cachedUntil
            };
        }
    }
}
