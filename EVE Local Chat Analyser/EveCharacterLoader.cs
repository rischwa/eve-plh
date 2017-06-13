#region

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using EveLocalChatAnalyser.Exceptions;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Utilities;

#endregion

namespace EveLocalChatAnalyser
{
    public class EveCharacterLoader
    {
        private const int MAX_PARALLEL_CHARACTER_LOADING_COUNT = 10;

        public static List<IEveCharacter> RetrieveCharactersByName(IList<string> unknownNames)
        {
            if (unknownNames.IsNullOrEmpty())
            {
                return new List<IEveCharacter>();
            }

            try
            {
                var charIds = RetrieveCharacterIds(unknownNames);
                return RetrieveCharacters(charIds);
            }
            catch (CcpApiException e)
            {
                throw new CharacterRetrievalException("Could not load characters", e);
            }
        }

        private static IEnumerable<string> ExtractCharacterIds(XContainer charactersDocument)
        {
            return from row in charactersDocument.Descendants("row")
                   let charId = row.Attribute("characterID")
                       .Value
                   where charId != "0"
                   select charId;
        }

        private static IEnumerable<string> RetrieveCharacterIds(IEnumerable<string> unknownNames)
        {
            return unknownNames.Split(100)
                .AsParallel()
                .WithDegreeOfParallelism(MAX_PARALLEL_CHARACTER_LOADING_COUNT)
                .Select(RetrieveCharacterIdsDocument)
                .SelectMany(ExtractCharacterIds)
                .ToArray();
        }

        private static XDocument RetrieveCharacterIdsDocument(IEnumerable<string> unknownNames)
        {
            const string REQUEST_URL = "https://api.eveonline.com/eve/CharacterID.xml.aspx";

            return XDocumentWebRequester.RequestCcpApiXmlDocumentPOST(
                                                                      REQUEST_URL,
                                                                      new Dictionary<string, string>
                                                                      {
                                                                          {"names", ToNamesUrlParameterValue(unknownNames)}
                                                                      });
        }

        private static List<IEveCharacter> RetrieveCharacters(IEnumerable<string> charIds)
        {
            return (from curId in charIds.AsParallel()
                        .WithDegreeOfParallelism(MAX_PARALLEL_CHARACTER_LOADING_COUNT)
                    let curCharacter = EveCharacterApiService.RetrieveCharacterById(curId)
                    orderby curCharacter.Name ascending
                    select curCharacter).Cast<IEveCharacter>()
                .ToList();
        }

        private static string ToNamesUrlParameterValue(IEnumerable<string> unknownNames)
        {
            return unknownNames.Aggregate("", (names, curName) => names + HttpUtility.UrlEncode(curName) + ",");
        }
    }
}
