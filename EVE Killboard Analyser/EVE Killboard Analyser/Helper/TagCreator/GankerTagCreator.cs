using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.TagCreator
{
    public class GankerTagCreator : ITagCreator
    {
        private const int REQUIRED_NUMBER_OF_TANKS_TO_BE_GANKER = 4;

     
        public IList<string> TagsFromCollection(int characterId, IList<Kill> kills)
        {
            var tmpKills = kills;
            var killsAsAttacker = (from curKill in tmpKills
                                   where curKill.Attackers.Any(attacker => attacker.CharacterID == characterId)
                                   select curKill).ToList();
            
            if (killsAsAttacker.Count < REQUIRED_NUMBER_OF_TANKS_TO_BE_GANKER)
            {
                return ToTags(isGanker: false);
            }

            var solarSystemIds = (from curKill in killsAsAttacker
                                  select curKill.SolarSystemID).ToList();

            IList<int> highsecSystemIds;
            IList<int> concordIds;
            using (var context = new DatabaseContext())
            {
                highsecSystemIds =
                    context.ExecuteSqlQuery<int>(
                        string.Format(
                            "select solarSystemID from eveuniversedata.dbo.mapSolarSystems where solarSystemID in ({0}) and security >= 0.5",
                            string.Join(",", solarSystemIds)));

                concordIds = context.ExecuteSqlQuery<int>("select typeID from evedb.dbo.invTypes where typeName like 'concord%'");
            }

            var possibleGanks = from curKill in killsAsAttacker
                                where highsecSystemIds.Contains(curKill.SolarSystemID)
                                select curKill;

            var ganks = from curGank in possibleGanks
                        where
                            kills.Any(
                                kill =>
                                kill.Victim.CharacterID == characterId &&
                                kill.SolarSystemID == curGank.SolarSystemID &&
                                kill.KillTime > curGank.KillTime &&
                                kill.KillTime < curGank.KillTime + new TimeSpan(0, 0, 2, 0) &&
                                kill.Attackers.Any(attacker => concordIds.Contains(attacker.ShipTypeID))
                            )
                        select curGank;

            var isGanker = ganks.Take(REQUIRED_NUMBER_OF_TANKS_TO_BE_GANKER).Count() ==
                           REQUIRED_NUMBER_OF_TANKS_TO_BE_GANKER;

            return ToTags(isGanker);
        }

        private static IList<string> ToTags(bool isGanker)
        {
            return isGanker ? new[] {"Ganker"} : new string[0];
        }

      
    }
}