using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public sealed class YellowToRedPalette : BasePalette
    {
        private static readonly Brush[] BRUSHES =
        {
            new SolidColorBrush(
                new Color
                {
                    R = 255,
                    G = 255,
                    B = 178,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 254,
                    G = 217,
                    B = 118,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 254,
                    G = 178,
                    B = 76,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 253,
                    G = 141,
                    B = 60,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 252,
                    G = 78,
                    B = 42,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 227,
                    G = 26,
                    B = 28,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 177,
                    G = 0,
                    B = 38,
                    A = 255
                })
        };

        static YellowToRedPalette()
        {
            foreach (var curBrush in BRUSHES)
            {
                curBrush.Freeze();
            }
        }

        public override Brush GetBrush(int index)
        {
            return BRUSHES[index];
        }

        public override string Description => "yellow - red";
    }
}