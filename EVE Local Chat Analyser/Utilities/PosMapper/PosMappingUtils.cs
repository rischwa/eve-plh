using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    internal static class PosMappingUtils
    {

        private static readonly long POINT_FIVE_AU_IN_KM = (long) UnitConverter.AUToKm(0.05);

        public static bool IsStargate(this IDScanItem x)
        {
            return x.Type.StartsWith("Stargate") || x.Type.StartsWith("Sternentor");//TODO german?
        }
        public static bool IsTower(this IDScanItem x)
        {
            return x.Type.Contains("Control Tower") || x.Type.Contains("Kontrollturm");
        }

        public static bool IsMoon(this IDScanItem arg)
        {
            return arg.Type == "Moon" || arg.Type == "Mond";
        }

        public static bool IsForceField(this IDScanItem arg)
        {
            return arg.Type == "Force Field"||  arg.Type == "Kraftfeld";
        }

        public static bool IsShip(this IDScanItem arg)
        {
            return ShipTypes.Instance.IsShipTypeName(arg.Type);
        }

        public static long GetLowerBoundInKm(Distance distance)
        {
            if (distance.AUValue < 0.1)
            {
                return Math.Max(distance.KmValue - 6000, 1000);
            }

            if (distance.KmValue < UnitConverter.AUToKm(0.15))
            {
               //the cut to 0.1 AU instead of km seems to be 10,000,000 km -> thank you Blue Katelo for finding out
                return 10000000;
            }

            return distance.KmValue - POINT_FIVE_AU_IN_KM;
        }

        public static long GetUpperBoundInKm(Distance distance)
        {
            if (distance.AUValue < 0.1)
            {
                return distance.KmValue + 6000;
            }

            return distance.KmValue + POINT_FIVE_AU_IN_KM;
        }

        public static List<IGrouping<long, IDScanItem>> GetMoonGroups(IEnumerable<IDScanItem> itemsAtNode)
        {
            return itemsAtNode.Where(IsMoon)
                              .GroupBy(x => x.Distance.KmValue)
                              .OrderBy(x => x.Key)
                              .ToList();
        }
    }
}