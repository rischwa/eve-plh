using System.ComponentModel;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators
{
    [Description("has station(/) station with rep facilities(X)")]
    public class StationTriStateCalculator : ITriStateCalculator
    {
        public int GetState(SolarSystemViewModel vm)
        {
            if (!vm.Stations.Any())
            {
                return 0;
            }
            return vm.Stations.Any(x => x.HasRepairFacility) ? 2 : 1;
        }
        
    }
}