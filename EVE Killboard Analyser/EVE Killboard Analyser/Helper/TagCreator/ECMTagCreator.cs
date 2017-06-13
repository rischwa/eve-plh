using System;
using System.Collections.Generic;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.TagCreator
{
    public class ECMTagCreator : ITagCreator
    {
        private const int MIN_AMOUNT_OF_KILLS = 30;
        private const double ECM_RATIO = 0.1;
        private const int MAX_NUMBER_OF_ATTACKERS = 10;
        private const int POD_ID = 670;
        private const int IMPAIROR_ID = 596;
        private const int REAPER_ID = 588;
        private const int IBIS_ID = 601;
        private const int VELATOR_ID = 606;

        private static DateTime MinTime
        {
            get { return DateTime.UtcNow - new TimeSpan(90, 0, 0, 0); }
        }

     

        public IList<string> TagsFromCollection(int characterId, IList<Kill> kills)
        {
            return Tags(characterId, kills);
        }

        private static IList<string> Tags(int characterId, IEnumerable<Kill> kills)
        {
            var minTime = MinTime;
            //exclude pods and rookie ships (structures should be excluded too + t1 frigs + indus + miningships)
// ReSharper disable PossibleMultipleEnumeration
            var allCountingKills =
                kills.Count(
                    kill =>
                    kill.KillTime > minTime && kill.Victim.ShipTypeID != POD_ID && kill.Victim.ShipTypeID != IMPAIROR_ID &&
                    kill.Victim.ShipTypeID != REAPER_ID && kill.Victim.ShipTypeID != IBIS_ID && kill.Victim.ShipTypeID != VELATOR_ID &&
                    kill.Attackers.Any(attacker => attacker.CharacterID == characterId) &&
                    kill.Attackers.Count <= MAX_NUMBER_OF_ATTACKERS);
// ReSharper restore PossibleMultipleEnumeration
            if (allCountingKills < MIN_AMOUNT_OF_KILLS)
            {
                return new List<string>();
            }
            //falcon/kitsune/griffin/blackbird
// ReSharper disable PossibleMultipleEnumeration
            double killsWithECM = kills.Count(kill => kill.KillTime > minTime && kill.Victim.ShipTypeID != POD_ID &&
// ReSharper restore PossibleMultipleEnumeration
                                                      kill.Attackers.Any(attacker => attacker.CharacterID == characterId) &&
                                                      kill.Attackers.Any(
                                                          a => a.CharacterID != characterId &&
                                                          (a.ShipTypeID == 11957 || a.ShipTypeID == 11194 ||
                                                           a.ShipTypeID == 584 || a.ShipTypeID == 632)) &&
                                                      kill.Attackers.Count <= MAX_NUMBER_OF_ATTACKERS);

            return killsWithECM / allCountingKills > ECM_RATIO
                       ? new List<string> {"Likes to use ECM support/alts"}
                       : new List<string>();
        }
    }
}