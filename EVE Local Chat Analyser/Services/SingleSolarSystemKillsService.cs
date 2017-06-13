using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Exceptions;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Utilities;
using log4net;
using Newtonsoft.Json;
using PLHLib;

namespace EveLocalChatAnalyser.Services
{
    public class SingleSolarSystemKillsService
    {
        private static readonly ILog LOG = LogManager.GetLogger("SingleSolarSystemKillsService");
        public async Task<IList<SolarSystemKills>> GetKillsForSystem(string systemName)
        {
            return await ApiCache.GetCurrentData(string.Format("{0}_{1}", typeof(SolarSystemKills).Name,systemName) , () => GetKills(systemName),x=>x, false);
         //   return await Task.Factory.StartNew(() => GetKills(systemName), TaskCreationOptions.LongRunning);
        }

        private CachedData<IList<SolarSystemKills>> GetKills(string newsystem)
        {
            try
            {
                var url = string.Format(RischwaNetService.BASE_URL + "/SingleSolarSystemKillsV1/{0}", newsystem);
                var response = WebUtilities.GetHttpGetResponseFrom(url);

                var message = JsonConvert.DeserializeObject<BaseResult<List<SolarSystemKills>>>(response);
                if (message == null || message.data == null || message.status != "success")
                {
                    throw new EveLocalChatAnalyserException(
                        string.Format(
                                      "Could not retrieve killboard info for solar systems {0}: {1}",
                                      newsystem,
                                      message != null ? message.message : "none"));
                }

                return new CachedData<IList<SolarSystemKills>>
                       {
                           CachedUntil = DateTime.UtcNow + new TimeSpan(0, 5, 0), //Cache 5 minutes
                           Value = message.data,
                           Id = newsystem
                       };

            }
            catch (WebException e)
            {
                LOG.Warn("Error fetching kills for solar system", e);
                throw;
            }
        }
    }
}
