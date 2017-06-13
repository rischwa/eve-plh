using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public interface IStargateLocationRepository
    {
        bool TryGetStargateLocation(int solarSystemID, Position pos, out StargateLocation location);
    }
}