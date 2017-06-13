using System.Collections.Generic;
using System.Linq;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.TagCreator
{
    public class OffGridBoosterTagCreator : ITagCreator
    {
        private static int[] _offgridBoosterModules;

        public IList<string> TagsFromCollection(int characterId, IList<Kill> kills)
        {
            //TODO bedeutung mit dem flag != 5 rausfinden und kapseln!!
            var losses = kills.Where(x => x.Victim.CharacterID == characterId).ToList();

            var boosterLosses = losses.Where(x => x.Items.Any(i => i.Flag != 5 && OffgridBoosterModules.Contains(i.TypeID))).ToList();
            var nonCommandDestroyerBoosterLossesCount = boosterLosses.Count(x => !Types.IsCommandDestroyer(x.Victim.ShipTypeID));
            var hasBoosterLosses = boosterLosses.Any();

            if (hasBoosterLosses)
            {
                var lastBoosterLoss = boosterLosses.OrderByDescending(x => x.KillTime).First();
                var count = kills.OrderByDescending(x => x.KillTime).Where(x=>x.Victim.CharacterID!=characterId).TakeWhile(x => x.KillTime > lastBoosterLoss.KillTime).Count();
                return (count < 30 && nonCommandDestroyerBoosterLossesCount > (boosterLosses.Count/2) ) ? new List<string> { "Offgrid Booster" } : new List<string> { "Possible Booster" };
            }
            return new List<string>();
        }

        public static int[] OffgridBoosterModules{get
        {
            if (_offgridBoosterModules != null)
            {
                return _offgridBoosterModules;
            }
            using (var context = new DatabaseContext())
            {
                _offgridBoosterModules =
                    context.ExecuteSqlQuery<int>("SELECT typeID FROM evedb.dbo.invTypes WHERE groupID = 316").ToArray();
            }
            return _offgridBoosterModules;
        }}
    }
}