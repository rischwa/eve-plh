using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public class ConquerableStation
    {
        public long SolarSystemId { get; set; }
        public string StationName { get; set; }
    }

    class EveConquerableStationListService
    {

        public async Task<Dictionary<long, Station[]>> GetConquerableStationsBySystemId()
        {
            return (await ApiCache.GetCurrentData("conquerableStations/v1", GetConquerableStationsBySystemIdSync, s=>s.GroupBy(x => x.SolarSystemId).ToDictionary(x => x.Key, y => y.Select(a => new Station { Name = a.StationName }).ToArray())).ConfigureAwait(false));
        }

        private static CachedData<List<ConquerableStation>> GetConquerableStationsBySystemIdSync()
        {
            //TODO kann man auch verallgemeinern
            var doc =
                XDocumentWebRequester.RequestCcpApiXmlDocument(
                    "https://api.eveonline.com/eve/ConquerableStationList.xml.aspx");

            var fwStats =
                doc.Descendants("row")
                   .Select(
                       x =>
                       new ConquerableStation
                       {
                           SolarSystemId = x.GetLongAttributeValue("solarSystemID"),
                           StationName = x.GetAttributeValue("stationName")
                       });

            var cachedUntil = DateTime.ParseExact(doc.Descendants("cachedUntil").First().Value, "yyyy-MM-dd HH:mm:ss",
                                                  CultureInfo.InvariantCulture);

            return new CachedData<List<ConquerableStation>>
            {
                Value = fwStats.ToList(),//new Dictionary<long, List<object>>(fwStats.GroupBy(x => x.SolarSystemId).ToDictionary(x => x.Key, y => y.Select(a => new Station { Name = a.StationName }).Cast<object>().ToList())),
                CachedUntil = cachedUntil
            };
        }
    }
}
