using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public sealed class TransparentToGreenPalette : BasePalette
    {
        private static readonly Brush[] BRUSHES =
        {
            new SolidColorBrush(
                new Color
                {
                    R = 237,
                    G = 248,
                    B = 233,
                    A = 0
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 199,
                    G = 233,
                    B = 192,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 161,
                    G = 217,
                    B = 155,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 116,
                    G = 196,
                    B = 118,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 65,
                    G = 171,
                    B = 93,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 35,
                    G = 139,
                    B = 69,
                    A = 255
                }),
            new SolidColorBrush(
                new Color
                {
                    R = 0,
                    G = 90,
                    B = 50,
                    A = 255
                })
        };

        static TransparentToGreenPalette()
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

        public override string Description => "transparent - green";
    }
}