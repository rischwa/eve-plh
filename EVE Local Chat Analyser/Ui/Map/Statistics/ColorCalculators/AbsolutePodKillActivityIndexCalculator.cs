using System.ComponentModel;
using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Map.Statistics.Palettes;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    [Description("pod kill activity")]
    public class AbsolutePodKillActivityIndexCalculator : IIndexCalculator
    {

        public int GetIndex(SolarSystemViewModel vm)
        {
            if (vm.Killboard.SmartbombPoddingCountVeryRecent > 2)
            {
                return 6;
            }
            if (vm.Killboard.SmartbombPoddingCountVeryRecent > 0)
            {
                return 5;
            }
            if (vm.Killboard.SmartbombPoddingCount > 0)
            {
                return 3;
            }
            if (vm.Kills.PodKillCount > 2)
            {
                return 3;
            }

            return vm.Kills.PodKillCount > 0 ? 2 : 0;
        }
    }
}