using System;
using System.Globalization;
using System.Text.RegularExpressions;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui.Models
{
    public class Distance
    {
        private readonly long _distanceInKm;
        private readonly bool _hasDistance;

        public Distance() {}

        public Distance(string distance)
        {
            if (distance == null ||
                (distance != "-" && !distance.EndsWith("km") && !distance.EndsWith("AU") && !distance.EndsWith("AE") && !distance.EndsWith("m")))
            {
                throw new ArgumentException();
            }
            _hasDistance = distance != "-";
            if (!_hasDistance)
            {
                return;
            }

            string distanceValue;
            if (distance.TryStripFromEnd(" km", out distanceValue))
            {
                _distanceInKm = int.Parse(NormalizeNumber(distanceValue), CultureInfo.InvariantCulture);
                return;
            }
            if (distance.TryStripFromEnd(" m", out distanceValue))
            {
                _distanceInKm = int.Parse(NormalizeNumber(distanceValue), CultureInfo.InvariantCulture) / 1000;
                return;
            }
            if (distance.TryStripFromEnd(" AU", out distanceValue) || distance.TryStripFromEnd(" AE", out distanceValue))
            {
                var distanceInAU = distanceValue.ToDoubleNormalized();
                _distanceInKm = (long) UnitConverter.AUToKm(distanceInAU);
                return;
            }

            throw new ArgumentException();
        }

        private static string NormalizeNumber(string distanceValue)
        {
            return Regex.Replace(distanceValue, "[\\s\\.,']", "");
        }

        public bool HasValue
        {
            get { return _hasDistance; }
        }

        public long KmValue
        {
            get { return _distanceInKm; }
        }

        public double AUValue
        {
            get
            {
                //use round to remove rounding errors from km conversion, precision in dscan is 1 fractional digit for AU values
                return Math.Round(UnitConverter.KmToAU(_distanceInKm), 2);
            }
        }

        public override string ToString()
        {
            if (!_hasDistance)
            {
                return "-";
            }
            return AUValue < 0.1 ? string.Format("{0} km", KmValue) : string.Format("{0} AU", AUValue.ToString("0.##"));
        }
    }
}