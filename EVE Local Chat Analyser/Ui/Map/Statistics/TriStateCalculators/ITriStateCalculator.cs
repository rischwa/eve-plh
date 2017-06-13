using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators
{
    public interface ITriStateCalculator
    {
        int GetState(SolarSystemViewModel vm);
        
    }
}