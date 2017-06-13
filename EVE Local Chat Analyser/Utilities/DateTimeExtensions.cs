using System;

namespace EveLocalChatAnalyser.Utilities
{
    public static class DateTimeExtensions
    {
        private static readonly TimeSpan ONE_HOUR = new TimeSpan(1, 0, 0);
        private static readonly TimeSpan TWO_HOURS = new TimeSpan(2, 0, 0);
        private static readonly TimeSpan ONE_DAY = new TimeSpan(1, 0, 0, 0);
        private static readonly TimeSpan TWO_DAYS = new TimeSpan(2, 0, 0, 0);

        public static string GetTimeDifference(this DateTime occurrence)
        {
            if (occurrence == DateTime.MinValue)
            {
                return "unknown";
            }

            var difference = DateTime.UtcNow - occurrence;
            if (difference < ONE_HOUR)
            {
                return String.Format("{0} minutes ago", (int)Math.Round(difference.TotalMinutes));
            }
            if (difference < TWO_HOURS)
            {
                return "1 hour ago";
            } 
            if (difference < ONE_DAY)
            {
                return String.Format("{0} hours ago", (int)Math.Round(difference.TotalHours));
            }
            if (difference < TWO_DAYS)
            {
                return "1 day ago";
            }
            return String.Format("{0} days ago", (int)Math.Round(difference.TotalDays));
        }

        public static string ToStringDefault(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
