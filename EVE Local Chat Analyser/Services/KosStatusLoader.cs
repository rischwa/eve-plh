//#region

//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Web;
//using EveLocalChatAnalyser.Utilities;
//using HtmlAgilityPack;

//#endregion

//namespace EveLocalChatAnalyser.Services
//{
//    public class KosStatusLoader
//    {
//        private readonly IList<IEveCharacter> _newCharacters;

//        private KosStatusLoader(IList<IEveCharacter> newChars)
//        {
//            _newCharacters = newChars;
//        }

//        public static void SetKosStatusOn(IList<IEveCharacter> newChars)
//        {
//            if (newChars.IsNullOrEmpty())
//            {
//                return;
//            }

//            var kosChars = RetrieveKosCharacters(newChars);

//            MarkAsKos(kosChars);
//        }

//        private string BuildRequestUri()
//        {
//            const string BASE_URI = "http://fof.rennfeuer.org/ajax.php?mod=fof3";

//            var characterParameter = _newCharacters.Aggregate("&fofcheck=",
//                                                              (parameterString, curCharacter) =>
//                                                              parameterString +
//                                                              HttpUtility.UrlEncode(curCharacter.Name + "\n"));
//            return BASE_URI + characterParameter;
//        }

//        private static HtmlDocument DocumentFromHtmlFragment(string result)
//        {
//            var doc = new HtmlDocument();
//            doc.LoadHtml("<html><head></head><body>" + result + "</body></html>");
//            return doc;
//        }

//        private IEnumerable<IEveCharacter> ExtractKosCharacters(IEnumerable<HtmlNode> kosNodes)
//        {
//            return from curNode in kosNodes
//                   let curName = curNode.InnerText.Trim()
//                   join curChar in _newCharacters on curName equals curChar.Name
//                   select curChar;
//        }

//        private static void MarkAsKos(IEnumerable<IEveCharacter> kosChars)
//        {
//            foreach (var curChar in kosChars)
//            {
//                curChar.IsCvaKos = true;
//            }
//        }

//        private static IEnumerable<IEveCharacter> RetrieveKosCharacters(IList<IEveCharacter> newChars)
//        {
//            var kosChecker = new KosStatusLoader(newChars);
//            return kosChecker.RetrieveKosCharacters();
//        }

//        private IEnumerable<IEveCharacter> RetrieveKosCharacters()
//        {
//            var kosHtmlDocument = RetrieveKosHtml();

//            var kosNodes = kosHtmlDocument.DocumentNode.SelectNodes("//table[@class='box']/tr[@class='iskos']/td[@class='pilotname bold']");
//            return kosNodes == null ? new List<IEveCharacter>() : ExtractKosCharacters(kosNodes);
//        }

//        private HtmlDocument RetrieveKosHtml()
//        {
//            using (var wc = new WebClient())
//            {
//                wc.Headers[HttpRequestHeader.ContentType] = "application/character-www-form-urlencoded; charset=UTF-8";

//                var uri = BuildRequestUri();
//                var result = wc.UploadString(uri, "");

//                return DocumentFromHtmlFragment(result);
//            }
//        }
//    }
//}