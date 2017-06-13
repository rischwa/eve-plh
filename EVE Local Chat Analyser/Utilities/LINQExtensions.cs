using System;
using System.Collections.Generic;
using System.Linq;

namespace EveLocalChatAnalyser.Utilities
{
    public static class LINQExtensions
    {
        public static readonly long MIN_AU_FOR_ROUNDING = (long) UnitConverter.AUToKm(0.1);
        public static readonly long AU_ROUNDING_DIFFERENCE_IN_KM = (long) UnitConverter.AUToKm(0.05);
        private static readonly long POINT_1_AU_IN_KM = (long) UnitConverter.AUToKm(0.1);

        public static long GetLowerBound(long distanceInKm)
        {
            return distanceInKm >= MIN_AU_FOR_ROUNDING
                       ? (distanceInKm/POINT_1_AU_IN_KM*POINT_1_AU_IN_KM) - AU_ROUNDING_DIFFERENCE_IN_KM
                       : Math.Max(DScanFinder.MIN_DISTANCE_IN_KM, distanceInKm - DScanFinder.KM_OFFSET);
        }

        public static long GetUpperBound(long distanceInKm)
        {
            return distanceInKm >= MIN_AU_FOR_ROUNDING
                       ? Math.Min(Int32.MaxValue,
                                  (distanceInKm/POINT_1_AU_IN_KM*POINT_1_AU_IN_KM) + AU_ROUNDING_DIFFERENCE_IN_KM)
                       : distanceInKm + DScanFinder.KM_OFFSET;
        }

        //public static long Median(this IEnumerable<long> source)
        //{
        //    if (!source.Any())
        //    {
        //        throw new InvalidOperationException("Cannot compute median for an empty set.");
        //    }

        //    var sortedList = (from number in source
        //                      orderby number
        //                      select number).ToList();

        //    int itemIndex = sortedList.Count() / 2;

        //    if (sortedList.Count() % 2 == 0)
        //    {
        //        // Even number of items. 
        //        return (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;
        //    }

        //    // Odd number of items. 
        //    return sortedList.ElementAt(itemIndex);
        //}
        public static long Median(this IList<DScanFinder.DScanGroup> source)
        {
            if (!source.Any())
            {
                throw new InvalidOperationException("Cannot compute median for an empty set.");
            }

            var sortedList = (from dScanItem in source
                              orderby dScanItem.Boundary.Upper
                              select dScanItem).ToList();

            var itemIndex = sortedList.Count()/2;

            var distanceInKm = sortedList.ElementAt(itemIndex).Boundary.Upper;
            if (sortedList.Count()%2 == 0)
            {
                return (long) ((distanceInKm + sortedList.ElementAt(itemIndex - 1).Boundary.Upper)/2.0);
            }

            return GetUpperBound(distanceInKm);
        }

        public static IEnumerable<T> Without<T>(this IEnumerable<T> first, IEnumerable<T> second) 
        {
            var x = second.ToList();
            foreach (var y in first)
            {
                if (!x.Contains(y))
                {
                    yield return y;
                }
                else
                {
                    x.Remove(y);
                }
            }
        }

        public static IEnumerable<T> Insersect<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var x = second.ToList();
            foreach (var y in first)
            {
                if (x.Contains(y))
                {
                    x.Remove(y);
                    yield return y;
                }
            }
        }
    }
}