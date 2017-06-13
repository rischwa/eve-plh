using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public static class EveStandingsApiService
    {
        public static Hashtable GetStandings(string characterId, long keyId, string vCode, out DateTime cachedUntil)
        {
            var doc = GetContactListDocument(characterId, keyId, vCode);

            var standings = GetStandings(doc);
            cachedUntil = GetCachedUntil(doc);

            AddStandingsForPersonalCorpAndAlliance(standings);

            return standings;
        }

        private static DateTime GetCachedUntil(XDocument doc)
        {
            return DateTime.Parse(doc.Descendants("cachedUntil").First().Value);
        }

        private static Hashtable GetStandings(XContainer doc)
        {
            var elements = (from curRow in GetRowsInDescendingPriorityOrder(doc)
                            select
                                new
                                    {
                                        Name = curRow.Attribute("contactName").Value,
                                        Standing = double.Parse(curRow.Attribute("standing").Value.Replace(",", "."), CultureInfo.InvariantCulture)
                                    });

            var entries = new Hashtable();
            foreach (var curElement in elements.Where(x => !entries.ContainsKey(x.Name)))
            {
                entries.Add(curElement.Name, curElement.Standing);
            }

            return entries;
        }

        private static IEnumerable<XElement> GetRowsInDescendingPriorityOrder(XContainer doc)
        {
            return doc.Descendants("result").Descendants("rowset").Where(x=>!x.Attribute("name").Value.Contains("Labels")).Descendants("row").Reverse();
        }

        private static void AddStandingsForPersonalCorpAndAlliance(Hashtable entries)
        {
            var eveChar = EveCharacterApiService.RetrieveCharacterById(ActiveProfile.Default.CharacterId);
            if (!entries.ContainsKey(eveChar.Corporation))
            {
                entries.Add(eveChar.Corporation, 10.0d);
            }

            if (eveChar.Alliance != null && !entries.ContainsKey(eveChar.Alliance))
            {
                entries.Add(eveChar.Alliance, 10.0d);
            }

            if (!entries.ContainsKey(eveChar.Name))
            {
                entries.Add(eveChar.Name, 10.0d);
            }
        }

        private static XDocument GetContactListDocument(string characterId, long keyId, string vCode)
        {
            return XDocumentWebRequester.RequestCcpApiXmlDocument(
                string.Format(
                    "https://api.eveonline.com/char/ContactList.xml.aspx?keyId={0}&vCode={1}&characterID={2}", keyId,
                    vCode, characterId));
        }
    }
}