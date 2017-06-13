using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using EVE_Killboard_Analyser.Helper.AnalysisProvider;

namespace EVE_Killboard_Analyser.Models
{
    public class FavouriteShip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public int CharacterID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public string Ship { get; set; }

        public int Order { get; set; }
    }

    public sealed class CharacterV1DataEntry
    {
        public CharacterV1DataEntry(int characterId, IList<string> favouriteShips, double averageAttackerCount,
                               IEnumerable<string> tags, IList<string> associatedAlliances, IList<string> associatedCorporations )
        {
            CharacterID = characterId;
            int i = 0;
            FavouriteShips = new List<FavouriteShip>();
            foreach (var curShip in favouriteShips)
            {
                FavouriteShips.Add(new FavouriteShip {CharacterID = characterId, Ship = curShip, Order = i++});
            }
            AverageAttackerCount = averageAttackerCount;
            AssociatedAlliances = associatedAlliances.Select(s => new AssociatedAlliance {CharacterID = characterId, AllianceName = s}).ToList();
            AssociatedCorporations = associatedCorporations.Select(s => new AssociatedCorporation { CharacterID = characterId, CorporationName = s }).ToList(); ;
            Tags = tags.Select(s=>new CharacterTag(){CharacterId = characterId, Tag = s}).ToList();
        }

        public CharacterV1DataEntry(){}

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CharacterID { get; set; }

        public IList<FavouriteShip> FavouriteShips { get; set; }

        public double AverageAttackerCount { get; set; }
        public IList<AssociatedAlliance> AssociatedAlliances { get; set; }
        public IList<AssociatedCorporation> AssociatedCorporations { get; set; }

        public IList<CharacterTag> Tags { get; set; }
        
        public CharacterStatistics Statistics { get; set; }

        [NotMapped]
        public LastSeen LastSeen { get; set; }
    }

    public class AssociatedAlliance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public int CharacterID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public string AllianceName { get; set; }
    }

    public class AssociatedCorporation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public int CharacterID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public string CorporationName { get; set; }
    }
}