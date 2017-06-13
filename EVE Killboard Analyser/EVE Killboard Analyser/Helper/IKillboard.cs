using System.Collections.Generic;
using System.Web;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper
{
    public interface IKillboard
    {
        IList<Kill> GetKills(int characterId);
        IList<Kill> GetLosses(int characterId);
        CharacterStatistics GetStatistics(int characterId);
    }
}
