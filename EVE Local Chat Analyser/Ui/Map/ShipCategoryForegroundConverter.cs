using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class ShipCategoryForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Properties.Settings.Default.DScanShipAggregatorGroupBy != "Role")
            {
                return Brushes.SolidBlackBrush;
            }

            var x = value as CollectionViewGroup;
            return x == null || !x.Items.Any() ? Brushes.SolidBlackBrush : x.Items.Cast<AggregatedShipTypeViewModel>().First().FleetRoleForeground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
