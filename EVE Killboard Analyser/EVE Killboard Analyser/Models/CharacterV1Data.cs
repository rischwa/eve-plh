using System.Collections.Generic;
using System.Linq;
using EVE_Killboard_Analyser.Helper.AnalysisProvider;

namespace EVE_Killboard_Analyser.Models
{
    internal class CharacterV1Data
    {
        private static readonly int[] AWESOME_PEOPLE = new[] {92321722, 268946627, 626711141};

        public CharacterV1Data(CharacterV1DataEntry dataEntry)
        {
            Tags = dataEntry.Tags != null ? dataEntry.Tags.Select(tag => tag.Tag).ToList() : new List<string>();
            FavouriteShips = dataEntry.FavouriteShips != null
                                 ? dataEntry.FavouriteShips.Select(ship => ship.Ship).ToList()
                                 : new List<string>();
            AverageAttackerCount = dataEntry.AverageAttackerCount;
            CharacterID = dataEntry.CharacterID;
            TotalsStatistics = dataEntry.Statistics != null ? dataEntry.Statistics.Totals : new SingleStatisticsEntry();
            AssociatedAlliances = dataEntry.AssociatedAlliances == null ? new List<string>() : dataEntry.AssociatedAlliances.Select(alliance => alliance.AllianceName).ToList();
            AssociatedCorporations = dataEntry.AssociatedCorporations == null  ? new List<string>() : 
                dataEntry.AssociatedCorporations.Select(corporation => corporation.CorporationName).ToList();
            LastSeen = dataEntry.LastSeen;

            if (Tags.Contains("Carebear") &&
                (Tags.Any(
                    s =>
                    s.Contains("Supercarrier") || s.Contains("Titan") || s.Contains("Dreadnaught") ||
                    s.Contains("Cynochar"))))
            {
                Tags.Remove("Carebear");
            }

            if (AWESOME_PEOPLE.Contains(CharacterID) && !Tags.Contains("Awesome Guy"))
            {
                Tags.Add("Awesome Guy");
            }
        }

        public List<string> AssociatedCorporations { get; set; }

        public List<string> AssociatedAlliances { get; set; }

        public SingleStatisticsEntry TotalsStatistics { get; set; }

        public int CharacterID { get; set; }

        public double AverageAttackerCount { get; set; }

        public List<string> FavouriteShips { get; set; }

        public List<string> Tags { get; set; }

        public LastSeen LastSeen { get; set; }
    }
}