#region

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using EveLocalChatAnalyser.Exceptions;

#endregion

namespace EveLocalChatAnalyser.Utilities
{
    internal static class XDocumentWebRequester
    {

        private static readonly string PLH_USER_AGENT;

        static XDocumentWebRequester ()
        {
            var version = typeof (XDocumentWebRequester).Assembly.GetName().Version;
            PLH_USER_AGENT = "EVE PLH/" + version.Major + "." + version.Minor;
        }

        private static XDocument RequestXmlDocument(string request)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(request);
            httpWebRequest.UserAgent = PLH_USER_AGENT;
            
            var response = (HttpWebResponse) httpWebRequest.GetResponse();

            var resStream = response.GetResponseStream();

            if (resStream == null)
            {
                throw new EveLocalChatAnalyserException(string.Format("Could not get response stream for {0}", request));
            }
            var doc = XDocument.Load(resStream);

            resStream.Close();
            response.Close();
            return doc;
        }

        internal static XDocument RequestCcpApiXmlDocument(string request)
        {
            var document = RequestXmlDocument(request);
            CheckForError(document);

            return document;
        }

        internal static XDocument RequestCcpApiXmlDocumentPOST(string url, IDictionary<string, string> parameters)
        {
            var document = RequestXmlDocumentPOST( url,  string.Join("&",parameters.Select(x=>x.Key + "=" + x.Value)));
            CheckForError(document);

            return document;
        }

        private static XDocument RequestXmlDocumentPOST(string url, string parameters)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

           
            var data = Encoding.ASCII.GetBytes(parameters);

            request.Method = "POST";
            request.UserAgent = PLH_USER_AGENT;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var resStream = response.GetResponseStream();

            if (resStream == null)
            {
                throw new EveLocalChatAnalyserException(string.Format("Could not get response stream for {0}", request));
            }
            var doc = XDocument.Load(resStream);

            resStream.Close();
            response.Close();
            return doc;
        }

        private static void CheckForError(XContainer document)
        {
            var firstOrDefault = document.Descendants("error").FirstOrDefault();
            if (firstOrDefault != null)
            {
                var errorCode = GetErrorCode(firstOrDefault);
                throw new CcpApiException(errorCode, firstOrDefault.Value);
            }
        }

        private static int GetErrorCode(XElement firstOrDefault)
        {
            int errorCode;
            var errorCodeAttribute = firstOrDefault.Attribute("code");
            if (errorCodeAttribute == null || !int.TryParse(errorCodeAttribute.Value, out errorCode))
            {
                errorCode = -1;
            }
            return errorCode;
        }
    }
}