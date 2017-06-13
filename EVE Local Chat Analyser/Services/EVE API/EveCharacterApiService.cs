#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using EveLocalChatAnalyser.Exceptions;
using EveLocalChatAnalyser.Utilities;

#endregion


namespace EveLocalChatAnalyser.Services.EVE_API
{
    internal class EveCharacterApiService
    {
        private readonly string _charId;

        private readonly XDocument _characterDocument;

        public class EveCharacterPositions
        {
            //TODO remove only for old versions
        }

        private EveCharacterApiService(string charId)
        {
            _charId = charId;
            _characterDocument = RetrieveCharacterXDocument();
        }

        private Age Age
        {
            get
            {
                var employmentHistory = _characterDocument.Descendants("rowset").First(IsEmploymentHistoryElement);
                //for extremely old chars it seems to be possible to ahve no employment history
                //see DrPRoberts - https://api.eveonline.com/eve/CharacterInfo.xml.aspx?characterID=151019785
                var employmentOnCharacterCreation = employmentHistory.Descendants("row").LastOrDefault();
                if (employmentOnCharacterCreation == null)
                {
                    return new Age(new DateTime(2003,04,1));
                }
                var firstEmploymentDate = GetStartDate(employmentOnCharacterCreation);

                return new Age(firstEmploymentDate);
            }
        }

        private string Alliance
        {
            get
            {
                var allianceElements = _characterDocument.Descendants("alliance").ToList();
                return !allianceElements.Any() ? null : allianceElements.First().Value;
            }
        }

        private double SecurityStatus
        {
            get { return Double.Parse(_characterDocument.Descendants("securityStatus").First().Value, CultureInfo.InvariantCulture); }
        }

        private string Corporation
        {
            get
            {
                var corporationElements = _characterDocument.Descendants("corporation").ToList();
                return !corporationElements.Any() ? null : corporationElements.First().Value;
            }
        }

        private string Name
        {
            get { return _characterDocument.Descendants("characterName").First().Value; }
        }

        public static EveCharacter RetrieveCharacterById(string charId)
        {
            try
            {
                var loader = new EveCharacterApiService(charId);

                var character = new EveCharacter(charId, loader.Name, loader.SecurityStatus, loader.Age, loader.Corporation, loader.Alliance, GetKnownPositions(long.Parse(charId)));
                character.PropertyChanged += CharacterOnPropertyChanged;
                
                return character;
            }
            catch (Exception e)
            {

                throw new CharacterRetrievalException(
                    string.Format("Could not retrieve character information for character with id: {0}", charId), e);
            }
        }

        private static void CharacterOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var character = sender as IEveCharacter;
            if (character == null)
            {
                return;
            }

            if (propertyChangedEventArgs.PropertyName != "KnownPositions")
            {
                return;
            }

            using (var session = App.CreateStorageEngine())
            {
                var charId = long.Parse(character.Id);

                var collection = session.GetCollection<Services.EVE_API.EveCharacterPositions>(typeof(Services.EVE_API.EveCharacterPositions).Name);
                //store the last 5 positions
                collection.Upsert(new Services.EVE_API.EveCharacterPositions { CharacterId = charId, Positions = character.KnownPositions.Take(5).ToList() });
            }
        }

        private static IEnumerable<CharacterPosition> GetKnownPositions(long charId)
        {
            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<Services.EVE_API.EveCharacterPositions>(typeof(EveCharacterPositions).Name);
                var positions = collection.FindById(charId);
                return positions != null ? positions.Positions : new List<CharacterPosition>();
            }
        }

        private DateTime GetStartDate(XElement employmentOnCharacterCreation)
        {
            const string DATE_FORMAT = "yyyy-MM-dd";
            var startDate = employmentOnCharacterCreation.Attribute("startDate")
                                                         .Value.Substring(0, DATE_FORMAT.Length);
            return DateTime.ParseExact(startDate, DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        private static bool IsEmploymentHistoryElement(XElement element)
        {
            return element.Attribute("name").Value == "employmentHistory";
        }

        private XDocument RetrieveCharacterXDocument()
        {
            var requestUrl = "https://api.eveonline.com/eve/CharacterInfo.xml.aspx?characterID=" + _charId;

            return XDocumentWebRequester.RequestCcpApiXmlDocument(requestUrl);
        }
    }
}