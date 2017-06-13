using System;
using System.IO;
using System.Net;
using EveLocalChatAnalyser.Exceptions;
using Newtonsoft.Json;

namespace EveLocalChatAnalyser.Services
{
    public struct MessageOfTheDay
    {
        public String Text { get; set; }
        public int MessageNumber { get; set; }
    }

    internal static class MotdService
    {
        public static MessageOfTheDay MessageOfTheDay
        {
            get
            {
                var httpWebRequest =
                    (HttpWebRequest) WebRequest.Create(RischwaNetService.BASE_URL + "/MessageOfTheDay");

                var response = (HttpWebResponse) httpWebRequest.GetResponse();

                var resStream = response.GetResponseStream();

                if (resStream == null)
                {
                    response.Close();
                    throw new EveLocalChatAnalyserException("Could not load message of the day");
                }

                var reader = new StreamReader(resStream);
                var result = reader.ReadToEnd();
                resStream.Close();
                response.Close();
                return JsonConvert.DeserializeObject<MessageOfTheDay>(result);
            }
        }
    }
}