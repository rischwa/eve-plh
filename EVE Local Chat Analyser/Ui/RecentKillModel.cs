using System.Collections.Generic;
using System.Linq;
using PLHLib;

namespace EveLocalChatAnalyser.Ui
{
    public class RecentKillModel
    {
        public RecentKillModel(KillResult killResult)
        {
            AttackerCount = killResult.Attackers.Count;
            var victim = killResult.Victim;
            //TODO Coalition
            //TODO mainwindow topzeile nicht ueberziehbar von dem resizer machen (rausziehen?)
            VictimShip = ShipTypes.Instance[victim.ShipTypeID];
            Identification = string.IsNullOrEmpty(victim.AllianceName)
                                 ? (string.IsNullOrEmpty(victim.CorporationName) ? victim.CharacterName : victim.CorporationName)
                                 : victim.AllianceName;
            Attackers = killResult.Attackers.Select(x => new AttackerModel(x));
            VictimName = killResult.Victim.CharacterName;
            VictimCorporation = killResult.Victim.CorporationName;
            VictimAlliance = killResult.Victim.AllianceName;
            KillTime = killResult.KillTime.ToString("HH:mm");
        }

        public string VictimAlliance { get; set; }

        public string KillTime { get; set; }

        public string VictimCorporation { get; set; }

        public string Identification { get; set; }

        public string VictimShip { get; set; }

        public int AttackerCount { get; set; }

        public IEnumerable<AttackerModel> Attackers { get; set; }
        public Victim Victim { get; set; }
        public string VictimName { get; set; }
    }
}