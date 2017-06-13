using System.ComponentModel;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    [Description("none")]
    public class NoActivityIndexCalculator : IIndexCalculator
    {
        public int GetIndex(SolarSystemViewModel vm)
        {
            return 0;
        }
    }
}
