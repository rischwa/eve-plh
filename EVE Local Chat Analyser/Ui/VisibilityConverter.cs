using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EveLocalChatAnalyser.Ui
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isValueInverted = ((bool?) parameter).GetValueOrDefault();
            var boolValue = ((bool?)value).GetValueOrDefault();

            var isVisible = isValueInverted ^ boolValue;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
