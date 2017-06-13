using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace EveLocalChatAnalyser.Ui
{
    public class HistoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var entry = value as LocalChatHistory.Entry;
            if (entry == null)
            {
                return null;
            }

            return string.Format("System: {0}   Time: {1}   #Chars:{2}", entry.System ?? "unknown",
                                 entry.TimeStamp.ToString("HH:mm"), entry.Characters.Count(character => character.LocalChangeStatus != LocalChangeStatus.Exited));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}