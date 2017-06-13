using System.Linq;
using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public abstract class BasePalette : IPalette
    {
        public abstract Brush GetBrush(int index);

        public  Brush this[int index] => GetBrush(index);

        public abstract string Description { get; }

        protected static SolidColorBrush[] ConvertFromStringBecauseLazy(string str)
        {
            return str.Split('\n')
                .Select(
                        x =>
                        {
                            var parts = x.Trim()
                                .Split(',');

                            var brush = new SolidColorBrush(
                                new Color
                                {
                                    R = byte.Parse(parts[0]),
                                    G = byte.Parse(parts[1]),
                                    B = byte.Parse(parts[2]),
                                    A = 255
                                });
                            brush.Freeze();

                            return brush;
                        })
                .ToArray();
        }
    }
}