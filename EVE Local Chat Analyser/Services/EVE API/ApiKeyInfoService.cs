using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public static class ApiKeyInfoService
    {
        public static string GetCharacterId(string keyIdStr, string vCode)
        {
            CheckApiInfoFormat(keyIdStr, vCode);

            var keyId = int.Parse(keyIdStr);
            return GetCharacterId(vCode, keyId);
        }

        private static void CheckApiInfoFormat(string keyIdStr, string vCode)
        {
            int keyId;
            if (!int.TryParse(keyIdStr, out keyId) || vCode == null || vCode.Length != 64)
            {
                throw new Exception("Invalid key id or verification code");
            }
        }

        private static string GetCharacterId(string vCode, int keyId)
        {
            var doc = GetApiInfoDocument(vCode, keyId);
            CheckApiKeyType(doc);
            CheckIfApiKeyHasExpired(doc);

            return
                doc.Elements("eveapi").Elements("result")
                   .Elements("key")
                   .Elements("rowset")
                   .Elements("row")
                   .First()
                   .Attribute("characterID")
                   .Value;
        }

        private static void CheckApiKeyType(XDocument doc)
        {
            var keyElement = doc.Descendants("result").Descendants("key").First();
            var typeValue = keyElement.Attribute("type").Value;
            if (typeValue != "Character"/* && typeValue != "Account"*/)
            {
                throw new Exception("You need to supply a Character api key (not one for account/all or a corporation)");
            }
        }

        private static XDocument GetApiInfoDocument(string vCode, int keyId)
        {
            var url = string.Format("https://api.eveonline.com/account/APIKeyInfo.xml.aspx?keyId={0}&vCode={1}",
                                    keyId, vCode);
            return XDocumentWebRequester.RequestCcpApiXmlDocument(url);
        }

        private static void CheckIfApiKeyHasExpired(XDocument doc)
        {
            var expires = GetExpireDateTime(doc);
            var currentTime = GetCurrentDateTime(doc);

            if (currentTime > expires)
            {
                throw new Exception("Your API key has expired");
            }
        }

        private static DateTime GetCurrentDateTime(XDocument doc)
        {
            var currentTimeStr = doc.Descendants("currentTime").First().Value;
            return ParseApiDateTime(currentTimeStr);
        }

        private static DateTime GetExpireDateTime(XDocument doc)
        {
            var element = doc.Descendants("result").Descendants("key").First();
            var expiresStr = element.Attributes("expires").First().Value;

            return string.IsNullOrEmpty(expiresStr) ? DateTime.MaxValue : ParseApiDateTime(expiresStr);
        }

        private static DateTime ParseApiDateTime(string expiresStr)
        {
            return DateTime.ParseExact(expiresStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}