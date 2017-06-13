using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.Palettes
{
    public sealed class WhiteToBluePalette : BasePalette
    {
        internal static readonly SolidColorBrush[] BRUSHES = ConvertFromStringBecauseLazy(@"239,243,255
198,219,239
158,202,225
107,174,214
66,146,198
33,113,181
8,69,148");

        


        public override Brush GetBrush(int index)
        {
            return BRUSHES[index];
        }

        public override string Description => "white - blue";
    }
}
