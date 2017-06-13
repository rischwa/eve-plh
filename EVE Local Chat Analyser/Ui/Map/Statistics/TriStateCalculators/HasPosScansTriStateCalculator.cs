using System.ComponentModel;
using EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities.PosMapper;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    [Description("POS scan of system stored (X)")]
    public class HasPosScansTriStateCalculator : ITriStateCalculator
    {
        //TODO needs to get update eventually
        public int GetState(SolarSystemViewModel vm)
        {
            var hasScan = App.GetFromCollection<MoonItemModel, bool>(c => c.Exists(x => x.Id.StartsWith(vm.Name)));
            return hasScan ? 2 : 0;
        }
        
    }
}