using EveLocalChatAnalyser.Utilities.RouteFinding;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public class KillNode : PriorityQueueNode
    {
        public readonly KillResult Kill;

        public StargateLocation StargateLocation { get; }


        public KillNode(KillResult kill, StargateLocation stargateLocation)
        {
            Kill = kill;
            StargateLocation = stargateLocation;
        }

        protected bool Equals(KillNode other)
        {
            return Equals(Kill.Victim.KillID, other.Kill.Victim.KillID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((KillNode) obj);
        }

        public override int GetHashCode()
        {
            return Kill?.Victim?.KillID.GetHashCode() ?? 0;
        }
    }
}