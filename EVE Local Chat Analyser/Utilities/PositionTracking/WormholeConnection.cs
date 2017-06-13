using System;
using Humanizer;
using LiteDB;

namespace EveLocalChatAnalyser.Utilities.PositionTracking
{
    public interface IWormholeConnection
    {
        string FirstSystem { get; }

        string FirstToSecondSignature { get; }

        DateTime MaxEndOfLife { get; }
        string SecondSystem { get; }
        string SecondToFirstSignature { get; }
    }

    public class EveScoutWormholeConnection : IWormholeConnection
    {
        private readonly EveScoutWormholeConnectionJson _model;

        public EveScoutWormholeConnection(EveScoutWormholeConnectionJson model)
        {
            _model = model;
            var isTheraFirst = string.CompareOrdinal("Thera", _model.destinationSolarSystem.name) < 0;

            FirstSystem = isTheraFirst ? "Thera" : _model.destinationSolarSystem.name;
            SecondSystem = isTheraFirst ? _model.destinationSolarSystem.name : "Thera";
            FirstToSecondSignature = isTheraFirst ? _model.signatureId : _model.wormholeDestinationSignatureId;
            SecondToFirstSignature = isTheraFirst ? _model.wormholeDestinationSignatureId : _model.signatureId;
        }

        public string FirstSystem { get; private set; }
        public string FirstToSecondSignature { get; private set; }

        public DateTime MaxEndOfLife
        {
            get { return _model.wormholeEstimatedEol; }
        }

        public string SecondSystem { get; private set; }
        public string SecondToFirstSignature { get; private set; }

        public string Status { get { return _model.status; } }

        public string LastStatusUpdateStr { get { return (_model.statusUpdatedAt).Humanize(); } }
    }

    public class EveScoutWormholeConnectionJson
    {
        public SolarSystem destinationSolarSystem { get; set; }

        public string signatureId { get; set; }
        public string wormholeDestinationSignatureId { get; set; }
        public DateTime wormholeEstimatedEol { get; set; }

        public class SolarSystem
        {
            public String name { get; set; }
        }

        public string status { get; set; }
        public DateTime statusUpdatedAt { get; set; }
    }

    public class WormholeConnection : IWormholeConnection
    {
        public WormholeConnection(string system1, string system2) : this()
        {
            if (string.CompareOrdinal(system1, system2) < 0)
            {
                FirstSystem = system1;
                SecondSystem = system2;
            }
            else
            {
                FirstSystem = system2;
                SecondSystem = system1;
            }
        }

        //public string Id
        //{
        //    get
        //    {
        //        return FirstSystem+"_"+SecondSystem;
        //    }
        //    set { }
        //}

        public WormholeConnection()
        {
            var now = DateTime.UtcNow;
            TimeOfFirstSighting = now;
            LastLifetimeUpdate = new WormholeLifetimeUpdate
                                     {
                                         Time = now,
                                         LifetimeStatus = WormholeLifetimeStatus.Unknown
                                     };
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
            return Equals((WormholeConnection) obj);
        }

        [BsonId]
        public string FirstSystem { get; set; }

        public string FirstToSecondSignature { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FirstSystem != null ? FirstSystem.GetHashCode() : 0) * 397)
                       ^ (SecondSystem != null ? SecondSystem.GetHashCode() : 0);
            }
        }

       

        public WormholeLifetimeUpdate LastLifetimeUpdate { get; set; }
        public WormholeMassStatus MassStatus { get; set; }

        public DateTime MaxEndOfLife
        {
            get
            {
                var maxSpan = TimeOfFirstSighting + new TimeSpan(24, 0, 0);
                return LastLifetimeUpdate.LifetimeStatus == WormholeLifetimeStatus.Critical
                           ? DateTimeUtilities.Min(LastLifetimeUpdate.Time + new TimeSpan(1, 0, 0), maxSpan)
                           : maxSpan;
            }
        }
        [BsonId]
        public string SecondSystem { get; set; }
        public string SecondToFirstSignature { get; set; }
        public DateTime TimeOfFirstSighting { get; set; }

        protected bool Equals(WormholeConnection other)
        {
            return string.Equals(FirstSystem, other.FirstSystem) && string.Equals(SecondSystem, other.SecondSystem);
        }
    }

    public static class DateTimeUtilities
    {
        public static DateTime Max(DateTime first, DateTime second)
        {
            return first > second ? first : second;
        }

        public static DateTime Min(DateTime first, DateTime second)
        {
            return first < second ? first : second;
        }
    }
}

