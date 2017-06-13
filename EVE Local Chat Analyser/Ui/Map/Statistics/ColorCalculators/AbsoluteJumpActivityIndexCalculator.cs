using System.ComponentModel;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    [Description("number of recent jumps")]
    public class AbsoluteJumpActivityIndexCalculator : IIndexCalculator
    {
        public int GetIndex(SolarSystemViewModel vm)
        {
            if (vm.JumpCount > 150)
            {
                return 6;
            }

            if (vm.JumpCount > 110)
            {
                return 5;
            }

            if (vm.JumpCount > 70)
            {
                return 4;
            }
            if (vm.JumpCount > 40)
            {
                return 3;
            }
            if (vm.JumpCount > 20)
            {
                return 2;
            }
            if (vm.JumpCount > 0)
            {
                return 1;
            }
            return 0;
        }
    }
}