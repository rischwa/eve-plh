using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;
using LiteDB;

namespace EveLocalChatAnalyser.Utilities.PositionTracking
{
    public interface IWormholeConnectionRepository
    {
        IEnumerable<WormholeConnection> GetAllWormholeConnections();

        WormholeConnection GetWormholeConnection(string firstSystem, string secondSystem);

        IEnumerable<WormholeConnection> GetWormholeConnectionsForSystem(string system);

        void UpsertWormholeConnection(WormholeConnection whConnection);

        void DeleteWormholeConnection(WormholeConnection whConnection);

        IEnumerable<WormholeConnection> GetWormholeConnectionsBetweenSystems(IList<SolarSystemViewModel> allSystems);

        IEnumerable<WormholeConnection> GetWormholeConnectionsBetweenSystemsWithout(IList<SolarSystemViewModel> allSystems,
                                                                                    IEnumerable<SolarSystemViewModel> without);
    }

    public class WormholeConnectionRepository : IWormholeConnectionRepository
    {
        private static readonly string FIRST_SYSTEM_PROPERTY_NAME = NotifyUtils.GetPropertyName((WormholeConnection p) => p.FirstSystem);
        private static readonly string SECOND_SYSTEM_PROPERTY_NAME = NotifyUtils.GetPropertyName((WormholeConnection p) => p.SecondSystem);

        public IEnumerable<WormholeConnection> GetAllWormholeConnections()
        {
            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<WormholeConnection>();
                return CleanupConnections(collection.All(), collection)
                    .ToArray();
            }
        }

        public WormholeConnection GetWormholeConnection(string firstSystem, string secondSystem)
        {
            if (String.Compare(firstSystem, secondSystem, StringComparison.Ordinal) > 0)
            {
                var tmp = firstSystem;
                firstSystem = secondSystem;
                secondSystem = tmp;
            }

            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<WormholeConnection>();
                var connection = collection.FindOne(x => x.FirstSystem == firstSystem && x.SecondSystem == secondSystem);

                return connection == null
                           ? null
                           : CleanupConnections(new[] {connection}, collection)
                                 .FirstOrDefault();
            }
        }

        public IEnumerable<WormholeConnection> GetWormholeConnectionsForSystem(string system)
        {
            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<WormholeConnection>();
                var connections = collection.Find(x => x.FirstSystem == system || x.SecondSystem == system);

                return CleanupConnections(connections, collection);
            }
        }

        public void UpsertWormholeConnection(WormholeConnection whConnection)
        {
            if (string.CompareOrdinal(whConnection.FirstSystem, whConnection.SecondSystem) > 0)
            {
                throw new Exception("Illegal whConnection");
            }

            using (var session = App.CreateStorageEngine())
            {
                session.GetCollection<WormholeConnection>().Upsert(whConnection);
            }
        }

        public void DeleteWormholeConnection(WormholeConnection whConnection)
        {
            if (string.CompareOrdinal(whConnection.FirstSystem, whConnection.SecondSystem) > 0)
            {
                throw new Exception("Illegal whConnection");
            }

            App.GetFromCollection<WormholeConnection, int>(c => c.Delete(x=>x.FirstSystem == whConnection.FirstSystem && x.SecondSystem == whConnection.SecondSystem));
        }

        public IEnumerable<WormholeConnection> GetWormholeConnectionsBetweenSystems(IList<SolarSystemViewModel> allSystems)
        {
            var systemNames = allSystems.Select(x => x.Name)
                .ToArray();
            using (var db = App.CreateStorageEngine())
            {
                var collection = db.GetCollection<WormholeConnection>();

                var connections =
                    collection.Find(
                                    Query.AND(
                                              Query.In(FIRST_SYSTEM_PROPERTY_NAME, systemNames),
                                              Query.In(SECOND_SYSTEM_PROPERTY_NAME, systemNames)));

                return CleanupConnections(connections, collection);
            }
        }

        public IEnumerable<WormholeConnection> GetWormholeConnectionsBetweenSystemsWithout(IList<SolarSystemViewModel> allSystems,
                                                                                           IEnumerable<SolarSystemViewModel> without)
        {
            var systemNames = allSystems.Select(x => x.Name)
                .ToArray();
            var withoutNames = without.Select(x => x.Name)
                .ToArray();
            using (var db = App.CreateStorageEngine())
            {
                var collection = db.GetCollection<WormholeConnection>();
                var connections = collection.Find(
                                                  Query.AND(
                                                            Query.In(FIRST_SYSTEM_PROPERTY_NAME, systemNames),
                                                            Query.In(SECOND_SYSTEM_PROPERTY_NAME, systemNames)))
                    .Where(x => !withoutNames.Contains(x.FirstSystem) || !withoutNames.Contains(x.SecondSystem));
                return CleanupConnections(connections, collection);
            }
        }

        private static IEnumerable<WormholeConnection> CleanupConnections(IEnumerable<WormholeConnection> connections,
                                                                          Collection<WormholeConnection> session)
        {
            var result = new List<WormholeConnection>();
            var minDate = DateTime.UtcNow - new TimeSpan(2, 0, 0, 0);
            var minDateCritical = DateTime.UtcNow - new TimeSpan(0, 1, 0, 0);
            foreach (var curConnection in connections)
            {
                if (curConnection.TimeOfFirstSighting < minDate
                    || (curConnection.LastLifetimeUpdate.LifetimeStatus == WormholeLifetimeStatus.Critical
                        && curConnection.TimeOfFirstSighting < minDateCritical))
                {
                    session.Delete(x => x.FirstSystem == curConnection.FirstSystem && x.SecondSystem == curConnection.SecondSystem);
                }
                else
                {
                    result.Add(curConnection);
                }
            }
            return result;
        }
    }
}
