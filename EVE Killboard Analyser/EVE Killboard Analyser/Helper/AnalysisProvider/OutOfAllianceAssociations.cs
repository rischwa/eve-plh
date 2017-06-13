using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.AnalysisProvider
{
    public class OutOfAllianceAssociations : IAnalysisProvider
    {
        private const int NEWEST_KILL_COUNT = 50;
        private const double ATTACKERS_PER_ALLIANCE_RATIO = 0.3;
        private const double KILLS_WITH_ALLIANCE_RATIO = 0.5;
        private const int MIN_KILL_COUNT = 10;
        public string FieldName
        {
            get { return "alliance associations"; }
        }

        public object GetValueFromCollection(int characterId, IList<Kill> kills)
        {
            var killsAsAttacker = (from curKill in kills
                                   where curKill.Attackers.Any(attacker => attacker.CharacterID == characterId)
                                   orderby curKill.KillTime descending
                                   select curKill).Take(NEWEST_KILL_COUNT).ToList();

            if (killsAsAttacker.Count < MIN_KILL_COUNT)
            {
                return new List<string>();
            }

            var killsAndAlliances = (from curKill in killsAsAttacker
                                     let ownAllianceID = curKill.Attackers.First(a => a.CharacterID == characterId).AllianceID
                                     let alliances = (from attackersByAlliance in
                                                          (
                                                              from attacker in curKill.Attackers
                                                              where !String.IsNullOrEmpty(attacker.AllianceName) && attacker.AllianceID != ownAllianceID
                                                              group attacker by attacker.AllianceName
                                                              into g
                                                              select g)
                                                      where
                                                          attackersByAlliance.Count() >
                                                          ATTACKERS_PER_ALLIANCE_RATIO*curKill.Attackers.Count()
                                                      select attackersByAlliance.Key)
                                     where alliances.Any()
                                     select new {Kill = curKill, Alliances = alliances}).ToList();

            var potentiallyAssociatedAlliances = killsAndAlliances.SelectMany(x => x.Alliances).Distinct();

            var associatedAlliances = from curAlliance in potentiallyAssociatedAlliances
                                      where
                                          killsAndAlliances.Count(x => x.Alliances.Contains(curAlliance)) >=
                                          KILLS_WITH_ALLIANCE_RATIO*killsAsAttacker.Count
                                      select curAlliance;

            return associatedAlliances.ToList();
        }
    }
}