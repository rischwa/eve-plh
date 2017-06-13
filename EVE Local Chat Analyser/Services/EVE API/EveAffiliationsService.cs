using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public class Affiliation
    {
        public long CharacterID { get; set; }
        public string FactionName { get; set; }
        public int FactionID { get; set; }
    }

    internal static class EveAffiliationsService
    {
        public static void LoadAffiliations(IList<IEveCharacter> characters)
        {
            foreach (var curAffiliation in GetAffiliations(characters.Select(c => c.Id).ToArray()))
            {
                characters.First(c => c.Id == curAffiliation.CharacterID.ToString(CultureInfo.InvariantCulture))
                          .SetFaction(curAffiliation.FactionID, curAffiliation.FactionName);
            }
        }

        public static List<Affiliation> GetAffiliations(params string[] characterIds)
        {
            if (characterIds.Length == 0)
            {
                return new List<Affiliation>();
            }
            var document = RequestCcpApiXmlDocument(characterIds);
            return document.Descendants("row")
                           .Select(
                               row =>
                               new Affiliation
                                   {
                                       CharacterID = long.Parse(row.Attribute("characterID").Value),
                                       FactionID = int.Parse(row.Attribute("factionID").Value),
                                       FactionName = row.Attribute("factionName").Value
                                   }).ToList();
        }

        private static XDocument RequestCcpApiXmlDocument(string[] characterIds)
        {
            var requestUrl = "https://api.eveonline.com/eve/CharacterAffiliation.xml.aspx?ids=" +
                             string.Join(",", characterIds);

            return XDocumentWebRequester.RequestCcpApiXmlDocument(requestUrl);
        }
    }
}