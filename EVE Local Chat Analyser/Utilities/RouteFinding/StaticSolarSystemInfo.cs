namespace EveLocalChatAnalyser.Utilities.RouteFinding
{
    public class StaticSolarSystemInfo
    {
        protected bool Equals(StaticSolarSystemInfo other)
        {
            return Id == other.Id;
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
            if (obj.GetType() != typeof (StaticSolarSystemInfo))
            {
                return false;
            }
            return Equals((StaticSolarSystemInfo) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public double SecurityStatus { get; set; }
    }
}