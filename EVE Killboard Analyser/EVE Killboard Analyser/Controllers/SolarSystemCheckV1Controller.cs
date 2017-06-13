using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;
using EVE_Killboard_Analyser.Helper;
using EVE_Killboard_Analyser.Helper.TagCreator;
using PLHLib;
using log4net;

namespace EVE_Killboard_Analyser.Controllers
{
    public class SolarSystemCheckV1Controller : ApiController
    {
        private const int MAX_NUMBER_OF_KILLS_PER_SYSTEM = 40;
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (SolarSystemCheckV1Controller));
        private static readonly TimeSpan ONE_HOUR = new TimeSpan(1, 0, 0);

        public BaseResult<List<SolarSystemKills>> Get(string id)
        {
            //TODO helper method
            id = SingleSolarSystemKillsV1Controller.CleanupSystemName(id);
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
                        var kills = context.ExecuteSqlQuery<SolarSystemKills>(
                            "SELECT toSolarSystemID AS SolarSystemId, s2.solarSystemName AS SolarSystemName, s2.security AS SolarSystemSecurity FROM (eveuniversedata.dbo.mapSolarSystems AS s1 JOIN eveuniversedata.dbo.mapSolarSystemJumps ON s1.solarSystemID = fromSolarSystemID) JOIN eveuniversedata.dbo.mapSolarSystems AS s2 ON toSolarSystemID = s2.solarSystemID WHERE s1.solarSystemName = @originSolarSystemName",
                            new SqlParameter("originSolarSystemName", id));

                        var originSolarSystem =
                            GetSolarSystemKillBasic(id, context);

                        kills.Add(originSolarSystem);
                        var systemIds = kills.Select(k => k.SolarSystemId).ToList();

                        var result = GetKillsForSystems(context, startDateTime, systemIds, kills);

                        AddSmartbombPoddingCounts(result, context);

                        LOGGER.Debug(string.Format("SolarSystemInfo from {1} for system {0} retrieved in {2}s", id,
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
                LOGGER.Error(string.Format("Error getting solarsystementry for {0}", id), e);
                throw;
            }
        }

        public static void AddSmartbombPoddingCounts(BaseResult<List<SolarSystemKills>> result, DatabaseContext context)
        {
            var smartbombPoddingEntries = GetSmartbombPoddingCount(result.data.Select(x => x.SolarSystemId), context);
            foreach (var curEntry in smartbombPoddingEntries)
            {
                var resultEntry = result.data.First(x => x.SolarSystemId == curEntry.SolarSystemID);
                resultEntry.SmartbombPoddingCount = curEntry.SmartbombPoddingCountLast48Hours;
                resultEntry.SmartbombPoddingCountVeryRecent = curEntry.SmartbombPoddingCountLast3Hours;
            }
        }

        private class SmartbombPoddingCountEntry
        {
            public int SolarSystemID { get; set; }
            public int SmartbombPoddingCountLast48Hours { get; set; }
            public int SmartbombPoddingCountLast3Hours { get; set; }
        }

        private static readonly string SMARTBOMB_IDS = string.Join(",", Types.SMARTBOMB_IDS);

        private static IEnumerable<SmartbombPoddingCountEntry> GetSmartbombPoddingCount(IEnumerable<int> solarSystemIds, DatabaseContext context)
        {

            var ids = string.Join(",", solarSystemIds);
            if (string.IsNullOrEmpty(ids))
            {
                return new List<SmartbombPoddingCountEntry>();
            }

            var command = "SELECT SolarSystemID, COUNT(*) AS SmartbombPoddingCountLast48Hours FROM Kills WHERE  SolarSystemID IN (" + ids + ") AND KillTime > dateadd(dd,-2,getutcdate()) AND EXISTS(SELECT 1 FROM Victims WHERE Kills.KillID = Victims.KillID AND (Victims.ShipTypeID = 670 OR Victims.ShipTypeID = 33328)) AND EXISTS(SELECT 1 FROM Attackers WHERE Attackers.KillID = Kills.KillID AND WeaponTypeID IN (" + SMARTBOMB_IDS + ") ) GROUP BY SolarSystemID";

            var recentCommand = "SELECT SolarSystemID, COUNT(*) AS SmartbombPoddingCountLast3Hours FROM Kills WHERE  SolarSystemID IN (" + ids + ") AND KillTime > dateadd(hh,-3,getutcdate()) AND EXISTS(SELECT 1 FROM Victims WHERE Kills.KillID = Victims.KillID AND (Victims.ShipTypeID = 670 OR Victims.ShipTypeID = 33328)) AND EXISTS(SELECT 1 FROM Attackers WHERE Attackers.KillID = Kills.KillID AND WeaponTypeID IN (" + SMARTBOMB_IDS + ") ) GROUP BY SolarSystemID";

            //LOGGER.Debug(command);

            var counts48 = context.ExecuteSqlQuery<SmartbombPoddingCountEntry>(command);
            var counts3 = context.ExecuteSqlQuery<SmartbombPoddingCountEntry>(recentCommand);

            foreach (var curCount in counts48)
            {
                var smartbombPoddingCountEntry = counts3.FirstOrDefault(x => curCount.SolarSystemID == x.SolarSystemID);
                if (smartbombPoddingCountEntry!= null)
                {
                    curCount.SmartbombPoddingCountLast3Hours = smartbombPoddingCountEntry.SmartbombPoddingCountLast3Hours;
                }
            }

            return counts48;
        }

        public static SolarSystemKills GetSolarSystemKillBasic(string name, DatabaseContext context)
        {
            return context.ExecuteSqlQuery<SolarSystemKills>(
                "SELECT solarSystemID AS SolarSystemId, @name AS SolarSystemName, security AS SolarSystemSecurity FROM eveuniversedata.dbo.mapSolarSystems WHERE solarSystemName = @name",
                new SqlParameter("name", name)).First();
        }

        public static BaseResult<List<SolarSystemKills>> GetKillsForSystems(DatabaseContext context, DateTime startDateTime, List<int> systemIds,
                                                     IList<SolarSystemKills> kills)
        {
            var killsInTheLastHour =
                context.Kills.Where(
                    kill => kill.KillTime > startDateTime && systemIds.Contains(kill.SolarSystemID))
                       .AsQueryable()
                       .Include(kill => kill.Attackers.Select(a => a.AllianceData))
                       .Include(kill => kill.Attackers.Select(a => a.CharacterData))
                       .Include(kill => kill.Attackers.Select(a => a.CorporationData))
                       .Include(kill => kill.Attackers.Select(a => a.FactionData))
                       .Include(kill => kill.Victim.CharacterData)
                       .Include(kill => kill.Victim.CorporationData)
                       .Include(kill => kill.Victim.FactionData)
                       .Include(kill => kill.Victim.AllianceData)
                       .ToList()
                       .Select(kill => new KillResult(kill));
            var killsInTheLastHourBySystem = killsInTheLastHour.ToLookup(key => key.SolarSystemId,
                                                                         value => value);

            //TODO fuer wenig kills, ist die aktuelle loesung besser, fuer sehr viele kills, einzelne abfragen mit limits ...


            return new BaseResult<List<SolarSystemKills>>
                {
                    data = kills.Select((x) =>
                        {
                            var killResults = killsInTheLastHourBySystem[x.SolarSystemId].ToList();
                            x.KillCountFromTheLastHour = killResults.Count;
                            x.RecentKills =
                                killResults.OrderByDescending(y => y.KillTime)
                                           .Take(MAX_NUMBER_OF_KILLS_PER_SYSTEM)
                                           .ToList();
                            return x;
                        }).ToList()
                };
        }
    }
}