using System;
using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public sealed class TransparentToBluePalette : BasePalette
    {
        private static readonly Brush[] BRUSHES = new Brush[7];

        static TransparentToBluePalette()
        {
            Array.Copy(WhiteToBluePalette.BRUSHES, BRUSHES, 7);
            BRUSHES[0] = Brushes.Transparent;
        }

        public override string Description => "transparent -> blue";

        public override Brush GetBrush(int index)
        {
            return BRUSHES[index];
        }
    }
}
