using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PLHLib
{

    [JsonObject(MemberSerialization.OptOut)]
    public class SolarSystemKills
    {
        public int KillCountFromTheLastHour { get; set; }

        public int SolarSystemId { get; set; }
        public string SolarSystemName { get; set; }
        public double SolarSystemSecurity { get; set; }
        [JsonIgnore]
        public string SolarSystemSecurityFormatted{get { return SolarSystemSecurity.ToString("0.0"); }}

        [JsonIgnore]
        public bool HasSmartbombPodding { get { return SmartbombPoddingCount > 0; } }

        public List<KillResult> RecentKills { get; set; }
        public int SmartbombPoddingCount { get; set; }
        public int SmartbombPoddingCountVeryRecent { get; set; }
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class KillResult
    {

        public KillResult()
        {
            
        }

        public KillResult(Kill kill)
        {
            SolarSystemId = kill.SolarSystemID;
            KillTime = kill.KillTime;
            Victim = kill.Victim;
            Attackers = kill.Attackers.ToList();
            Position = new Position {X=kill.Victim.X,Y = kill.Victim.Y, Z=kill.Victim.Z};
        }

        public int SolarSystemId { get; set; }

        public DateTime KillTime { get; set; }

        public Victim Victim { get; set; }

        public List<Attacker> Attackers { get; set; }

        public Position Position { get; set; }
    }
}
