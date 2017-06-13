using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EVE_Killboard_Analyser.Helper;
using EVE_Killboard_Analyser.Helper.TagCreator;
using PLHLib;
using log4net;

namespace EVE_Killboard_Analyser.Controllers
{
    public static class CharacterLoader
    {
        private const int MAX_KILL_COUNT = 500;
        private static readonly ILog LOGGER = LogManager.GetLogger("CharacterLoader");
        private static readonly int[] INTERESTING_TYPE_IDS = new int[] { 28646, 21096 }.Concat(OffGridBoosterTagCreator.OffgridBoosterModules).ToArray();
        public static IEnumerable<Kill> GetKillsOfCharacter(int id, DatabaseContext context)
        {
            var start = DateTime.Now;
            var victimIds = context.Victims.Where(x => x.CharacterID == id).Select(x=>x.KillID);
            var allKillIds = victimIds.Union(context.Attackers.Where(x => x.CharacterID == id).Select(x=>x.KillID)).ToList();

            var kills = context.Kills.Where(x => allKillIds.Contains(x.KillID)).OrderByDescending(x=>x.KillTime).Take(MAX_KILL_COUNT).ToList();//.Include(x => x.Attackers).ToList();
            var latestKillIds = kills.Select(x => x.KillID).ToArray();
            var attackers = context.Attackers.Where(x => latestKillIds.Contains(x.KillID)).ToLookup(x => x.KillID, x => x);
            var victims = context.Victims.Where(x => latestKillIds.Contains(x.KillID)).ToDictionary(x=>x.KillID, x=>x);
            
            var items = context.Items.Where(x => victimIds.Contains(x.KillID) && x.Flag != 5 && INTERESTING_TYPE_IDS.Contains(x.TypeID)).GroupBy(x => x.KillID).ToDictionary(x => x.Key, x => x);
            foreach (var curKill in kills)
            {
                context.ObjectContext.Detach(curKill);
                curKill.Attackers = attackers[curKill.KillID].ToList();
                curKill.Victim = victims[curKill.KillID];
                IGrouping<long, Item> curItems;
                if (items.TryGetValue(curKill.KillID, out curItems))
                {
                    curKill.Items = curItems.ToList();
                }
                else
                {
                    curKill.Items = new List<Item>();
                }
            }

            LOGGER.Debug(string.Format("Retrieved {0} kills for {1} in {2}s", kills.Count, id, (DateTime.Now - start).TotalSeconds));
            return kills;
            
            
            //var ids = GetKillIdsOfCharacter(context, id);
            //var result = Queryable.Where(context.Kills, x=>ids.Contains(x.KillID)).Include(x=>x.Attackers).ToList();
            //foreach (var curResult in result)
            //{
            //    var x = curResult.Victim;
            //}

            //CharactersV1Controller.LOGGER.Debug(string.Format("Retrieved {0} kills for {1} in {2}s", result.Count, id, (DateTime.Now-start).TotalSeconds));

            //return result;
        }

        //private static IList<long> GetKillIdsOfCharacter(DatabaseContext context, int id)
        //{
        //    return context.ExecuteSqlQuery<long>(
        //        "SELECT Kills.KillID FROM Kills WHERE EXISTS(SELECT 1 FROM Attackers WHERE CharacterID=@charID AND Attackers.KillID = Kills.KillID) OR EXISTS(SELECT 1 FROM Victims WHERE  CharacterID=@charID AND Victims.KillID = Kills.KillID)", new SqlParameter("charID", id));
        //}
    }
}