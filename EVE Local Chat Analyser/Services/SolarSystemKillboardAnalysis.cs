using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using EveLocalChatAnalyser.Exceptions;
using EveLocalChatAnalyser.Utilities;
using Newtonsoft.Json;
using PLHLib;

namespace EveLocalChatAnalyser.Services
{
    public static class SolarSystemKillboardAnalysis
    {
        public static async Task<List<SolarSystemKills>> GetInfoForAdjacentSystemsOf(string newsystem)
        {
            return await Task.Factory.StartNew(()=>RequestInfoForAdjacentSystemsOf(newsystem), TaskCreationOptions.LongRunning);
        }

        private static List<SolarSystemKills> RequestInfoForAdjacentSystemsOf(string newsystem)
        {
            var url = string.Format(RischwaNetService.BASE_URL + "/SolarSystemCheckV1/{0}", HttpUtility.UrlEncode(newsystem));
            var response = WebUtilities.GetHttpGetResponseFrom(url);

            var message = JsonConvert.DeserializeObject<BaseResult<List<SolarSystemKills>>>(response);
            if (message == null || message.data == null || message.status != "success")
            {
                throw new EveLocalChatAnalyserException(
                    string.Format("Could not retrieve killboard info for adjacent systems of {0}: {1}", newsystem,
                                  message != null ? message.message : "none"));
            }

            return message.data;
        }
    }
}
