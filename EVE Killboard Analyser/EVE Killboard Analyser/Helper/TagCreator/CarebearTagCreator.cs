using System.Collections.Generic;
using System.Linq;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.TagCreator
{
    public class CarebearTagCreator : ITagCreator
    {
        public const string CAREBEAR = "Carebear";

        public IList<string> TagsFromCollection(int characterId, IList<Kill> kills)
        {
            var killCount = kills.Count(kill => kill.Attackers.Any(attacker => attacker.CharacterID == characterId));
            return killCount < 25 ? new[] { CAREBEAR } : new string[0];
        }
    }
}