using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using EveLocalChatAnalyser.Ui.PosMapper;

namespace EveLocalChatAnalyser.Ui
{
    public class MoonItemGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as CollectionViewGroup;
            if (x == null)
            {
                return null;
            }
            return x.Items.Cast<MoonItemViewModel.AggregatedItem>().Sum(a => a.Amount);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
