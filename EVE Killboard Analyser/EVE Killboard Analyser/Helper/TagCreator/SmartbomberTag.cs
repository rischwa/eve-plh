using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.TagCreator
{
    public class SmartbomberTag : ITagCreator
    {
        public IList<string> TagsFromDatabase(DatabaseContext context, int characterId)
        {
            return TagsFromCollection(characterId, context.Kills.Where(k =>
                    k.Attackers.Any(a => a.CharacterID == characterId)).AsQueryable().Include(t=>t.Attackers).ToList());
        }

        public IList<string> TagsFromCollection(int characterId, IList<Kill> kills)
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            var attackerKills =
                kills.Where(
                    k =>
                    k.Attackers.Any(a => a.CharacterID == characterId) &&
                    k.KillTime > threeMonthsAgo).ToList();

            var smartBombKills =
                attackerKills.Where(
                    k => k.Attackers.Any(a => a.CharacterID == characterId && a.WeaponTypeID.IsSmartBomb())).ToList();
            
            return (attackerKills.Count > 0 && (((double)smartBombKills.Count/(double) attackerKills.Count) > 0.2d))
                       ? new List<string> {"Likes to use smartbombs"}
                       : new List<string>();
        }
    }
}