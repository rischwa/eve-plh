using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLHLib
{
    public class Kill
    {
       

        private sealed class KillIDEqualityComparer : IEqualityComparer<Kill>
        {
            public bool Equals(Kill x, Kill y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.KillID == y.KillID;
            }

            public int GetHashCode(Kill obj)
            {
                return obj.KillID.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<Kill> KILL_ID_COMPARER_INSTANCE = new KillIDEqualityComparer();

        public static IEqualityComparer<Kill> KillIDComparer
        {
            get { return KILL_ID_COMPARER_INSTANCE; }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long KillID { get; set; }

        public int SolarSystemID { get; set; }

        [Index(IsClustered = false, IsUnique = false)]
        public DateTime KillTime { get; set; }
        
        public int MoonID { get; set; }

        [InverseProperty("Kill")]
        public virtual ICollection<Attacker> Attackers { get; set; }

        [Required]
        [InverseProperty("Kill")]
        public virtual Victim Victim { get; set; }

        [InverseProperty("Kill")]
        public virtual ICollection<Item> Items { get; set; }

       
    }


    public class Position
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
    }
}