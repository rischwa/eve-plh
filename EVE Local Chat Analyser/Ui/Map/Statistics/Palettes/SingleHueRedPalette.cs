using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public sealed class SingleHueRedPalette : BasePalette
    {
        private static readonly SolidColorBrush[] BRUSHES = ConvertFromStringBecauseLazy(@"254,229,217
252,187,161
252,146,114
251,106,74
239,59,44
203,24,29
153,0,13");
        public override Brush GetBrush(int index)
        {
            return BRUSHES[index];
        }

        public override string Description => "single hue red";
    }
}
