using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.AnalysisProvider
{
    public class AvgShipCountOnRecentKills : IAnalysisProvider
    {
        private const int NEWEST_KILL_COUNT_TO_CONSIDER = 20;

        public string FieldName
        {
            get { return "avg_ship_count"; }
        }

        public object GetValueFromCollection(int characterId, IList<Kill> kills)
        {
            var newestKills =
                kills.Where(kill => kill.Attackers.Any(attacker => attacker.CharacterID == characterId))
                     .OrderByDescending(kill => kill.KillTime)
                     .Take(NEWEST_KILL_COUNT_TO_CONSIDER).ToList();

            
            return newestKills.Any() ? Math.Round(newestKills.Average(kill => kill.Attackers.Count), 2) : 0.0;
        }
    }
}