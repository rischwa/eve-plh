using System.Globalization;

namespace EveLocalChatAnalyser.Utilities
{
    internal static class StringUtils
    {
        public static double ToDoubleNormalized(this string value)
        {
            return double.Parse(value.Replace(',', '.').Replace("'", ""), CultureInfo.InvariantCulture);
        }

        public static string StripFromEnd(this string value, string stripFromEnd)
        {
            return value.Substring(0, stripFromEnd.Length);
        }

        public static bool TryStripFromEnd(this string value, string stripFromEnd, out string strippedString)
        {
            if (value == null || stripFromEnd == null || !value.EndsWith(stripFromEnd))
            {
                strippedString = value;
                return false;
            }

            strippedString = value.Substring(0, value.Length - stripFromEnd.Length);
            return true;
        }
    }
}