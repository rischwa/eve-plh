using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PLHLib
{


    public class Attacker : Character
    {
        [Column(Order = 1), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(Order = 0), Key, ForeignKey("Kill")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long KillID { get; set; }

        [JsonIgnore]
        public virtual Kill Kill { get; set; }

        public double SecurityStatus { get; set; }
        public int DamageDone { get; set; }
        [JsonConverter(typeof(NumericBoolConverter))]
        public bool FinalBlow { get; set; }
        public int WeaponTypeID { get; set; }
        public int ShipTypeID { get; set; }
    }
}