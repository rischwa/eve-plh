using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Map.Statistics.Palettes;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    public class IndexColorConverter
    {
        private readonly IIndexCalculator _indexCalculator;
        private readonly IPalette _palette;

        public IndexColorConverter(IIndexCalculator indexCalculator, IPalette palette)
        {
            _indexCalculator = indexCalculator;
            _palette = palette;
        }

        public Brush GetBrush(SolarSystemViewModel vm)
        {
            return _palette[_indexCalculator.GetIndex(vm)];
        }
    }
}