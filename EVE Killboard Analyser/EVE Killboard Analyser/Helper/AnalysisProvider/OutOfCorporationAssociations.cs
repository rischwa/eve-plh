using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.AnalysisProvider
{
    public class OutOfCorporationAssociations : IAnalysisProvider
    {
        private const int NEWEST_KILL_COUNT = 50;
        private const double ATTACKERS_PER_CORPORATION_RATIO = 0.3;
        private const double KILLS_WITH_CORPORATION_RATIO = 0.5;
        private const int MIN_KILL_COUNT = 10;

        public string FieldName
        {
            get { return "corporationAssociations"; }
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

            var killsAndCorporations = (from curKill in killsAsAttacker
                                        let ownCorporationID = curKill.Attackers.First(a => a.CharacterID == characterId).CorporationID
                                        let corporations = (from attackersByCorporation in
                                                                (
                                                                    from attacker in curKill.Attackers
                                                                    where String.IsNullOrEmpty(attacker.AllianceName) && attacker.CorporationID != ownCorporationID
                                                                    group attacker by attacker.CorporationName
                                                                        into g
                                                                        select g)
                                                            where
                                                                attackersByCorporation.Count() >
                                                                ATTACKERS_PER_CORPORATION_RATIO *
                                                                curKill.Attackers.Count()
                                                            select attackersByCorporation.Key)
                                        where corporations.Any()
                                        select new { Kill = curKill, Corporations = corporations }).ToList();

            var potentiallyAssociatedCorporations = killsAndCorporations.SelectMany(x => x.Corporations).Distinct();

            var associatedCorporations = from curCorporation in potentiallyAssociatedCorporations
                                         where
                                             killsAndCorporations.Count(x => x.Corporations.Contains(curCorporation)) >=
                                             KILLS_WITH_CORPORATION_RATIO * killsAsAttacker.Count
                                         select curCorporation;

            return associatedCorporations.ToList();
        }
    }
}