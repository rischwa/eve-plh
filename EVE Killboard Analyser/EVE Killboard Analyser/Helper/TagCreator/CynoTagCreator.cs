using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.TagCreator
{
    public class CynoTagCreator : ITagCreator
    {
        private const int CAPSULE_ID = 670;
        private const int RECENT_COUNT = 20;

        private static bool IsDedicatedCynoChar(int recentLossesCount, int recentLossesWithCynoCount)
        {
            return recentLossesCount > 0 && recentLossesWithCynoCount >= .5*recentLossesCount;
        }

        private static IList<string> GetCynoTags(int characterId, IEnumerable<Kill> kills)
        {
            var losses = kills.AsQueryable().Where(kill => kill.Victim.CharacterID == characterId && kill.Victim.ShipTypeID != CAPSULE_ID).OrderByDescending(kill => kill.KillTime).Include(x => x.Items);

            var recentLosses = losses.Take(RECENT_COUNT).ToList();

            Func<Kill, bool> cynoLossPredicate = kill => kill.Items.Any(item => item.Flag != 5 && (item.TypeID == 28646 || item.TypeID == 21096));

            var recentCynoLosses = recentLosses.Count(cynoLossPredicate);
            if (IsDedicatedCynoChar(recentLosses.Count, recentCynoLosses))
            {
               return new List<string>{"Cynochar"};
            }

            var startDate = DateTime.Now.AddYears(-1);
            var countingLosses = losses.Where(x =>
                x.KillTime > startDate && !NOOB_AND_T1_FRIG_IDS.Contains(x.Victim.ShipTypeID)).Include(a => a.Items).ToList();
            var usesCombatCyno = countingLosses.Any(x => x.Items.Any(item => item.Flag != 5 && (item.TypeID == 28646 || item.TypeID == 21096)));

            if (!usesCombatCyno)
            {
                return new List<string>();
            }

            var last = countingLosses.Where(cynoLossPredicate).OrderByDescending(x => x.KillTime).First().KillTime;

            return new List<string>{string.Format("Occasional cyno-bait (last: {0})", last.ToString("yyyy-MM-dd"))};
        }

        private static readonly int[] NOOB_AND_T1_FRIG_IDS = new int[] {CAPSULE_ID, 602, 603, 605, 582, 583, 584, 585, 586, 587, 598, 599, 3766, 597, 29248, 589, 590, 591, 2161, 607, 608, 609, 592, 593, 594, 596, 601, 606, 588 };

        public IList<string> TagsFromCollection(int characterId, IList<Kill> kills)
        {
            return GetCynoTags(characterId, kills);
        }
    }
}