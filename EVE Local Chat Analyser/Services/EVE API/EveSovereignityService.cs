using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public struct SovEntry
    {
        private static readonly IDictionary<long, string> FACTIONS = new Dictionary<long, string>
            {
                {0, ""},
                {500001, "Caldari State"},
                {500002, "Minmatar Republic"},
                {500003, "Amarr Empire"},
                {500004, "Gallente Federation"},
                {500005, "Jove Empire"},
                {500006, "CONCORD Assembly"},
                {500007, "Ammatar Mandate"},
                {500008, "Khanid Kingdom"},
                {500009, "The Syndicate"},
                {500010, "Guristas Pirates"},
                {500011, "Angel Cartel"},
                {500012, "Blood Raider Covenant"},
                {500013, "The InterBus"},
                {500014, "ORE"},
                {500015, "Thukker Tribe"},
                {500016, "Servant Sisters of EVE"},
                {500017, "The Society of Conscious Thought"},
                {500018, "Mordu's Legion Command"},
                {500019, "Sansha's Nation"},
                {500020, "Serpentis"},
                {500021, "Unknown"}
            };

        public long AllianceID;
        public long FactionID;
        public long SolarSystemID;

        public string Faction
        {
            get { return FACTIONS[FactionID]; }
        }

        public bool IsNpcSov { get { return AllianceID == 0; } }
    }

    public class EveSovereignityService
    {
        public async Task<IDictionary<long, SovEntry>> GetSovereignityBySystemId()
        {
            return await ApiCache.GetCurrentData("sovereignity/v1", GetSovereignitySync,x=>x);
        }

        private static CachedData<IDictionary<long, SovEntry>> GetSovereignitySync()
        {
            var doc =
                XDocumentWebRequester.RequestCcpApiXmlDocument(
                    "https://api.eveonline.com/map/Sovereignty.xml.aspx");

            var sovEntries =
                doc.Descendants("row")
                   .Select(
                       x =>
                       new SovEntry
                       {
                           SolarSystemID = x.GetLongAttributeValue("solarSystemID"),
                           AllianceID = x.GetLongAttributeValue("allianceID"),
                           FactionID = x.GetLongAttributeValue("factionID")
                       })
                   .ToList();

            var cachedUntil = DateTime.ParseExact(doc.Descendants("cachedUntil").First().Value, "yyyy-MM-dd HH:mm:ss",
                                                  CultureInfo.InvariantCulture);

            return new CachedData<IDictionary<long, SovEntry>>
            {
                Value = sovEntries.ToDictionary(x=>x.SolarSystemID, x=>x),
                CachedUntil = cachedUntil
            };
        }
    }
}