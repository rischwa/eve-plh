using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using PLHLib;

namespace EveLocalChatAnalyser.Services
{
    public interface IEveScoutService
    {
        IEnumerable<EveScoutWormholeConnection> GetConnectionsToThera();

        void ClearCache();
    }

    public class EveScoutService : IEveScoutService
    {
        private const string REQUEST_URL = "http://www.eve-scout.com/api/wormholes?limit=1000&offset=0&order=asc";
        private static readonly TimeSpan TIMEOUT = new TimeSpan(0, 0, 0, 3);
        private static readonly TimeSpan CACHING_TIME = new TimeSpan(0, 0, 0, 10);

        private IEnumerable<EveScoutWormholeConnection> _connections =
            new ReadOnlyCollection<EveScoutWormholeConnection>(new List<EveScoutWormholeConnection>());

        private DateTime _nextUpdate = DateTime.MinValue;

        public void ClearCache()
        {
            _nextUpdate = DateTime.MinValue;
        }

        public IEnumerable<EveScoutWormholeConnection> GetConnectionsToThera()
        {
            if (_nextUpdate < DateTime.UtcNow)
            {
                UpdateConnections();
            }

            return _connections;
        }

        private void UpdateConnections()
        {
            try
            {
                var connections = WebUtilities.GetFromHttpJson<EveScoutWormholeConnectionJson[]>(REQUEST_URL, TIMEOUT);
                _connections = connections.Select(x => new EveScoutWormholeConnection(x))
                                          .ToList()
                                          .AsReadOnly();

                _nextUpdate = DateTime.UtcNow + CACHING_TIME;
            }
            catch (Exception e)
            {
                //TODO loggen? werte resetten?
            }
        }
    }
}
