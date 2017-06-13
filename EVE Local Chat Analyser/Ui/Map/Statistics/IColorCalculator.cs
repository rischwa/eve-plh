using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    public interface IColorCalculator
    {
        Brush GetBrush(SolarSystemViewModel vm);
    }
}