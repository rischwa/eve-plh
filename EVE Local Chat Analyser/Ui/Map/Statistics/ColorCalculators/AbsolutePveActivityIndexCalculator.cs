using System.ComponentModel;
using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Map.Statistics.Palettes;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    [Description("pve activity")]
    public class AbsolutePveActivityIndexCalculator : IIndexCalculator
    {

        public int GetIndex(SolarSystemViewModel vm)
        {
            var killCount = vm.Kills.NpcKillCount;

            if (killCount > 200)
            {
                return 6;
            }
            if (killCount > 140)
            {
                return 5;
            }
            if (killCount > 100)
            {
                return 4;
            }
            if (killCount > 60)
            {
                return 3;
            }
            if (killCount > 25)
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