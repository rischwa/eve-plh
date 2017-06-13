using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators
{
    [Description("has recent [3h(/) or 1h(X)] pod kills")]
    public class HasRecentPodKillTriStateCalculator : ITriStateCalculator
    {
        public int GetState(SolarSystemViewModel vm)
        {
            return vm.Killboard.SmartbombPoddingCountVeryRecent == 0 ? 0 : 2;
        }
        
    }
}
