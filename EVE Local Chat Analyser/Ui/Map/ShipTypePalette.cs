using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map
{
    public static class ShipTypePalette
    {
        public static readonly SolidColorBrush CAPITALS =
            new SolidColorBrush(new Color {R = 254, G = 67, B = 101, A = 255});

        public static readonly SolidColorBrush DPS = new SolidColorBrush(new Color {R = 252, G = 157, B = 154, A = 255});

        public static readonly SolidColorBrush TACKLE =
            new SolidColorBrush(new Color {R = 249, G = 205, B = 13, A = 255});

        public static readonly SolidColorBrush EWAR = new SolidColorBrush(new Color {R = 200, G = 200, B = 169, A = 255});

        public static readonly SolidColorBrush LOGISTICS =
            new SolidColorBrush(new Color {R = 131, G = 175, B = 155, A = 255});

        public static readonly SolidColorBrush FANCY = new SolidColorBrush(new Color {R = 73, G = 10, B = 61, A = 255});

        public static readonly SolidColorBrush CIVILIAN =
            new SolidColorBrush(new Color {R = 255, G = 255, B = 255, A = 255});

        static ShipTypePalette()
        {
            CAPITALS.Freeze();
            DPS.Freeze();
            TACKLE.Freeze();
            EWAR.Freeze();
            LOGISTICS.Freeze();
            CIVILIAN.Freeze();
        }
    }
}