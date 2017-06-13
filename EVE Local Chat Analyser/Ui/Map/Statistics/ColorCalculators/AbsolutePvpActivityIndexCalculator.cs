using System.ComponentModel;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    [Description("pvp activity")]
    public class AbsolutePvpActivityIndexCalculator : IIndexCalculator
    {
        

        public int GetIndex(SolarSystemViewModel vm)
        {
            var killCount = vm.Kills.PodKillCount + vm.Kills.ShipKillCount;
            if (killCount > 100)
            {
                return 6;
            }
            if (killCount > 50)
            {
                return 5;
            }
            if (killCount > 25)
            {
                return 4;
            }
            if (killCount > 10)
            {
                return 3;
            }
            if (killCount > 3)
            {
                return 2;
            }
            if (killCount > 0)
            {
                return 1;
            }
            return 0;
        }
    }
}