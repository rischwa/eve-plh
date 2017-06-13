using System.Collections.Generic;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public class GateCampMessageModel
    {
        public GateCampMessageModel()
        {
        }

        public GateCampMessageModel(GateCamp gateCamp)
        {
            GateCampIndex = gateCamp.GateCampIndex;
            StargateLocations = gateCamp.StargateLocations;
        }

        public double GateCampIndex { get; set; }

        public List<StargateLocation> StargateLocations { get; set; }

    }
}