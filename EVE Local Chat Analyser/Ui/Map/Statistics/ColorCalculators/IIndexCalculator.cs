using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    public interface IIndexCalculator
    {
        int GetIndex(SolarSystemViewModel vm);
    }
}