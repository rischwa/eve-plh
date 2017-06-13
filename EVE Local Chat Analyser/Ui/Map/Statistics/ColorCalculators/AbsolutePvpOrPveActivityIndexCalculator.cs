using System;
using System.ComponentModel;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    [Description("pve or pvp activity")]
    public class AbsolutePvpOrPveActivityIndexCalculator : IIndexCalculator
    {
        private readonly AbsolutePvpActivityIndexCalculator _pvpActivityIndexCalculator = new AbsolutePvpActivityIndexCalculator();
        private readonly AbsolutePveActivityIndexCalculator _pveActivityIndexCalculator = new AbsolutePveActivityIndexCalculator();

        public int GetIndex(SolarSystemViewModel vm)
        {
            return Math.Max(_pvpActivityIndexCalculator.GetIndex(vm), _pveActivityIndexCalculator.GetIndex(vm));
        }
    }
}