using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public interface IPalette
    {
        //int ElementCount { get; }

        Brush GetBrush(int index);

        Brush this[int index] { get; }

         string Description{ get; }
    }
}