using System.Collections.Generic;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public class GateCamp
    {
        public GateCamp()
        {
            StargateLocations = new List<StargateLocation>();
            Kills = new List<KillResult>();
        }

        public double GateCampIndex { get; set; }

        public List<StargateLocation> StargateLocations { get; set; }

        public List<KillResult> Kills { get; set; }

        public bool IsAtSameLocation(IEnumerable<StargateLocation> locations)
        {
            return StargateLocations.HasIntersection(locations);
        }
    }
}