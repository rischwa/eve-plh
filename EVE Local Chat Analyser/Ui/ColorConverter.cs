using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is System.Drawing.Color))
            {
                return null;
            }
            var nb = ToMediaColor((System.Drawing.Color) value);
            return new SolidColorBrush(nb);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static Color ToMediaColor(System.Drawing.Color drawingColor)
        {
            return Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        public static System.Drawing.Color ToDrawingColor(Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }
    }
}