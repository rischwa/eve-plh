using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class ShipCategoryBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as CollectionViewGroup;
            if (x == null)
            {
                return Brushes.SolidWhiteBrush;
            }

            if (Properties.Settings.Default.DScanShipAggregatorGroupBy != "Role")
            {
                var count = x.Items.Cast<AggregatedShipTypeViewModel>().Sum(z => z.Count);
                if (count < 6)
                {
                    return Brushes.SolidLightGreenBrush;
                }
                return count < 11 ? Brushes.SolidOrangeBrush : Brushes.SolidLightCoralBrush;
            }

            return x == null || !x.Items.Any() ? Brushes.SolidWhiteBrush : x.Items.Cast<AggregatedShipTypeViewModel>().First().FleetRoleBackground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}