using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVE_Killboard_Analyser.Models
{
    public class KillboardRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CharacterID { get; set; }
        public DateTime LastAccess { get; set; }
    }
}