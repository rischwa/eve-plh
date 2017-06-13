using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web;
using System.Web.Http;
using EVE_Killboard_Analyser.Helper;
using Ninject.Activation;
using PLHLib;
using log4net;

namespace EVE_Killboard_Analyser.Controllers
{
    public static class IpBlock
    {
        private static readonly List<int> BLOCKED_ACCESS = new List<int>();
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(IpBlock));
        public static bool IsBlocked(string clientIp, DatabaseContext context)
        {
            IPAddress ipAddress;
            if (IPAddress.TryParse(clientIp, out ipAddress))
            {
                var intAddress = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
                if (context.BlockedIps.Any(x => x.Ip == intAddress))
                {
                    if (BLOCKED_ACCESS.All(x => x != intAddress))
                    {
                        BLOCKED_ACCESS.Add(intAddress);
                        LOGGER.Info("Blocked access from " + clientIp);
                    }
                    return true;
                }
            }
            else
            {
                LOGGER.Debug("Ip not recognized: " + clientIp);
            }
            return false;
        }

        public static void ThrowIfBlocked(HttpRequestMessage request, DatabaseContext context)
        {
            if (IsBlocked(request.GetClientIp(), context))
            {
                throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.Forbidden,
                                                                            "Stop Doing This! Be polite and ask, if you want to access my API, it is not public!"));
            }
        }
    }

    public class SingleSolarSystemKillsV1Controller : ApiController
    {
        //TODO most of the stuff should go into a superclass shared with SolarSystemCheckV1Controller
        private const int MAX_NUMBER_OF_KILLS_PER_SYSTEM = 40;
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(SingleSolarSystemKillsV1Controller));
        private static readonly TimeSpan ONE_HOUR = new TimeSpan(1, 0, 0);
        public BaseResult<List<SolarSystemKills>> Get(string id)
        {
            id = CleanupSystemName(id);
            try
            {
                var startTime = DateTime.UtcNow;
                var startDateTime = DateTime.UtcNow - ONE_HOUR;
                using (var context = new DatabaseContext())
                {
                    var clientIp = Request.GetClientIp();

                    IpBlock.ThrowIfBlocked(Request, context);
                   
                    using (var scope = new TransactionScope(TransactionScopeOption.Required, new
                                                                                                 TransactionOptions
                    {
                        IsolationLevel = IsolationLevel.ReadUncommitted
                    }))
                    {
                        var kills = new List<SolarSystemKills>();

                        var originSolarSystem =
                            SolarSystemCheckV1Controller.GetSolarSystemKillBasic(id, context);

                        kills.Add(originSolarSystem);
                        var systemIds = kills.Select(k => k.SolarSystemId).ToList();

                        var result = SolarSystemCheckV1Controller.GetKillsForSystems(context, startDateTime, systemIds, kills);
                        
                        SolarSystemCheckV1Controller.AddSmartbombPoddingCounts(result, context);

                        LOGGER.Debug(string.Format("SingleSolarSystemKills from {1} for system {0} retrieved in {2}s", id,
                                                   clientIp,
                                                   (DateTime.UtcNow - startTime).TotalSeconds));

                        scope.Complete();

                        return result;
                    }
                }
            }

            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                    LOGGER.Error(string.Format("Error getting singlesolarsystemkills for {0}", id), e);
                throw;
            }
        }

        public static string CleanupSystemName(string id)
        {

            //New Caldari|Tash-Murkon Prime|Ardishapur Prime|Du Annes|Lower Debyl|Upper Debyl|Sarum Prime|Khanid Prime|Kador Prime|Serpentis Prime|Old Man Star|Kor-Azor Prime|Central Point|Promised Land|Dead End|New Eden
            switch (id)
            {
                case "Sarum":
                    return "Sarum Prime";
                case "Old":
                case "OldManStar":
                    return "Old Man Star";
                case "Tash-Murkon":
                    return "Tash-Murkon Prime";
                case "Ardishapur":
                    return "Ardishapur Prime";
                case "Du":
                    return "Du Annes";
                case "Lower":
                    return "Lower Debyl";
                case "Upper":
                    return "Upper Debyl";
                case "Khanid":
                    return "Khanid Prime";
                case "Kador":
                    return "Kador Prime";
                case "Serpentis":
                    return "Serpentis Prime";
                case "Kor-Azor":
                    return "Kor-Azor Prime";
                case "Central":
                    return "Central Point";
                case "Promised" :
                    return "Promised Land";
                case "Dead" :
                    return "Dead End";
                default:
                    return id;
            }
        }
    }
}
