using System.Collections.Generic;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public interface IGateCampDifferenceDetector
    {
        event GateCampAdded GateCampAdded;

        event GateCampRemoved GateCampRemoved;

        event GateCampIndexChanged GateCampIndexChanged;

        void SetNextStatus(IReadOnlyCollection<GateCamp> gateCamps);
    }
}