using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class ShipCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as CollectionViewGroup;
            if (x == null)
            {
                return null;
            }

            return x.Items.Cast<AggregatedShipTypeViewModel>().Sum(z => z.Count);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
