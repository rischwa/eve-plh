using System.Collections.Generic;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper
{
    public interface ITagCreator
    {
        IList<string> TagsFromCollection(int characterId, IList<Kill> kills);
    }
}