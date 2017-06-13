using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators
{
    [Description("none")]
    public class NoTriStateCalculator : ITriStateCalculator
    {
        public int GetState(SolarSystemViewModel vm)
        {
            return 0;
        }
        
    }
}
