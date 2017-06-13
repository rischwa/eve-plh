using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PLHLib
{

    [Table("CorporationData")]
    public class CorporationData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CorporationID { get; set; }

        public string CorporationName { get; set; }
    }

    [Table("AllianceData")]
    public class AllianceData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AllianceID { get; set; }

        public string AllianceName { get; set; }
    }

    [Table("FactionData")]
    public class FactionData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FactionID { get; set; }

        public string FactionName { get; set; }
    }

    [Table("CharacterData")]
    public class CharacterData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CharacterID { get; set; }

        public string CharacterName { get; set; }
    }

    public class Character
    {
        [Index(IsClustered = false, IsUnique = false), ForeignKey("CharacterData")]
        public int CharacterID { get; set; }
        
        public virtual CharacterData CharacterData { get; set; }

        [NotMapped]
        public string CharacterName
        {
            get { return ExternalCharacterName ?? CharacterData.CharacterName; }
            set { ExternalCharacterName = value; }
        }

        [NotMapped]
        public string ExternalCharacterName { get; set; }

        [ForeignKey("CorporationData")]
        public int CorporationID { get; set; }

        public virtual CorporationData CorporationData { get; set; }

        [NotMapped]
        public string CorporationName
        {
            get { return ExternalCorporationName ?? CorporationData.CorporationName; }
            set { ExternalCorporationName = value; }
        }

        [NotMapped]
        public string ExternalCorporationName { get; set; }

        [ForeignKey("AllianceData")]
        public int AllianceID { get; set; }

        public virtual AllianceData AllianceData { get; set; }

        [NotMapped]
        public string AllianceName
        {
            get { return ExternalAllianceName ?? AllianceData.AllianceName; }
            set { ExternalAllianceName = value; }
        }

        [NotMapped]
        public string ExternalAllianceName { get; set; }

        [ForeignKey("FactionData")]
        public int FactionID { get; set; }
        
        public virtual FactionData FactionData { get; set; }

        //TODO external crap rausziehen und einen eigenen json view machen
        [NotMapped]
        public string FactionName
        {
            get { return ExternalFactionName ?? FactionData.FactionName; }
            set { ExternalFactionName = value; }
        }

        [NotMapped]
        public string ExternalFactionName { get; set; }
    }

    public class Victim : Character
    {
        [Key,ForeignKey("Kill")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long KillID { get; set; }

        [Required]
        [JsonIgnore]
        public virtual Kill Kill { get; set; }

        public int ShipTypeID { get; set; }
        public int DamageTaken { get; set; }
        
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
    }

   
}