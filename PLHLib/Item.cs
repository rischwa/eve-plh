using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PLHLib
{
    public class Item
    {
        [Column(Order = 0), Key, ForeignKey("Kill")]
        public long KillID { get; set; }
        [JsonIgnore]
        public virtual Kill Kill { get; set; }

        [Column(Order = 1), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int TypeID { get; set; }
        public int Flag { get; set; }
        public int QtyDropped { get; set; }
        public int QtyDestroyed { get; set; }
        public int Singleton { get; set; }
    }
}