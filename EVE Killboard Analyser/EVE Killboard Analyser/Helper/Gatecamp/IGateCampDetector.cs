using System.Collections.Generic;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public interface IGateCampDetector
    {
        IReadOnlyCollection<GateCamp> GateCamps { get; }

        void AddRange(IEnumerable<KillResult> kills);

        void DetectGateCamps();

        void AddKill(Kill kill);
    }
}