using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EVE_Killboard_Analyser.Models
{
    public class SingleStatisticsEntry
    {
        public SingleStatisticsEntry()
        {

        }

        public SingleStatisticsEntry(SingleStatisticsEntry characterStatisticsJson)
        {
            IskDestroyed = characterStatisticsJson.IskDestroyed;
            PointsLost = characterStatisticsJson.PointsLost;
            ShipsLost = characterStatisticsJson.ShipsLost;
            PointsDestroyed = characterStatisticsJson.PointsDestroyed;
            ShipsDestroyed = characterStatisticsJson.ShipsDestroyed;
            IskLost = characterStatisticsJson.IskLost;
        }

        public double IskLost { get; set; }
        public double IskDestroyed { get; set; }
        public int PointsLost { get; set; }
        public int PointsDestroyed { get; set; }
        public int ShipsLost { get; set; }
        public int ShipsDestroyed { get; set; }
    }

    public class CharacterStatisticsJson : SingleStatisticsEntry
    {
        public Dictionary<int, SingleStatisticsEntry> Groups;

        //public SingleStatisticsEntry Totals
        //{
        //    get
        //    {
        //        return this;
        //    }
        //}

        public CharacterStatistics ToDBCharacterStatistics(int characterId)
        {
            return new CharacterStatistics
                {
                    CharacterID = characterId,
                   Totals =  new SingleStatisticsEntry(this),
                    Groups = Groups == null
                                 ? null
                                 : Groups.Select(
                                     pair => new SingleStatisticsForShipType(characterId, pair.Key, pair.Value))
                                         .ToList()
                };
        }
    }


    public class SingleStatisticsForShipType
    {
        public SingleStatisticsForShipType()
        {
        }

        public SingleStatisticsForShipType(int characterId, int shipTypeId, SingleStatisticsEntry entry)
        {
            CharacterID = characterId;
            ShipTypeId = shipTypeId;
            IskLost = entry.IskLost;
            IskDestroyed = entry.IskDestroyed;
            PointsLost = entry.PointsLost;
            PointsDestroyed = entry.PointsDestroyed;
            CountLost = entry.ShipsLost;
            CountDestroyed = entry.ShipsDestroyed;
        }

        public double IskLost { get; set; }
        public double IskDestroyed { get; set; }
        public int PointsLost { get; set; }
        public int PointsDestroyed { get; set; }
        public int CountLost { get; set; }
        public int CountDestroyed { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public int CharacterID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        public int ShipTypeId { get; set; }
    }

    public class CharacterStatistics
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CharacterID { get; set; }
        
        public virtual List<SingleStatisticsForShipType> Groups { get; set; }
        public SingleStatisticsEntry Totals { get; set; }
    }
}