using System;
using System.Collections.Generic;
using System.Linq;

namespace EveLocalChatAnalyser
{
    public enum LastSeenType
    {
        Unknown = 0,
        Kill,
        Loss
    }

    public class LastSeen
    {
        public LastSeenType Type { get; set; }

        public String ShipName { get; set; }

        public DateTime Occurrence { get; set; }

        public String Weapon { get; set; }
    }

    public class KillboardInformation
    {
        public KillboardInformation()
        {
            AssociatedAlliances = new List<string>();
            AssociatedCorporations = new List<string>();
            FavouriteShips = new List<string>();
        }

        public IEnumerable<string> FavouriteShips { get; set; }
        public int CharacterID { get; set; }
        public double AverageAttackerCount { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public StatisticsEntry TotalsStatistics { get; set; }
        public IList<string> AssociatedAlliances { get; set; }
        public IList<string> AssociatedCorporations { get; set; }
        public LastSeen LastSeen { get; set; }
        
        public override string ToString()
        {
            
            var iskLost = FormatIsk(TotalsStatistics.IskLost);
            var iskDestroyed = FormatIsk(TotalsStatistics.IskDestroyed);
            int totalPoints = TotalsStatistics.PointsDestroyed + TotalsStatistics.PointsLost;
            var pointRatio = totalPoints == 0 ? "0%" :
                (TotalsStatistics.PointsDestroyed/
                 (double) totalPoints).ToString("0.0%");

            return string.Format("K/D: {3}/{4} (ISK: {5} / {6})\nPoint Ratio: {7}\nAvg. #chars on recent kills: {0}\n\nFavourite Ships: {1}{2}",
                                 AverageAttackerCount, string.Join(", ", FavouriteShips),
                                 Tags.Any() ? string.Format("\n\nTags:\t{0}", string.Join(",\n\t", Tags)) : "", TotalsStatistics.ShipsDestroyed, TotalsStatistics.ShipsLost, iskDestroyed, iskLost, pointRatio);
        }

        private static string FormatIsk(double iskValue)
        {
            const string MILLION_ISK_FORMAT = "0M";
            const string BILLION_ISK_FORMAT = "0.#B";
            const double _1_BILLION = 1000000000;
            const double _1_MILLION = 1000000;

            return iskValue > _1_BILLION ? (iskValue/_1_BILLION).ToString(BILLION_ISK_FORMAT) : (iskValue/_1_MILLION).ToString(MILLION_ISK_FORMAT);
        }
    }

    public class StatisticsEntry
    {
        public int ShipsDestroyed { get; set; }
        public int ShipsLost { get; set; }
        public double IskDestroyed { get; set; }
        public double IskLost { get; set; }
        public int PointsDestroyed { get; set; }
        public int PointsLost { get; set; }
    }
}