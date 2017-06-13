using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Utilities.PositionTracking;

namespace EveLocalChatAnalyser.Utilities.RouteFinding
{
    public enum RouteType
    {
        Shortest = 0,
        PreferLow,
        PreferHigh
    }

    public class RouteFinder : IRouteFinder
    {
        private readonly IStaticUniverseData _staticUniverseData;
        private readonly IWormholeConnectionRepository _wormholeConnectionRepository;
        private readonly IEveScoutService _eveScoutService;
        private readonly ILookup<int, int> _staticConnections;
        private ILookup<int, int> _connections;
        private readonly Dictionary<int, int> _mapSystemIdToArrayIndex;
        private readonly StaticSolarSystemInfo[] _staticSolarSystemInfos;
        private readonly int _systemCount;
        private int[] _distance;
        private bool[] _isScanned;
        private QueueNode[] _nodes;
        private IRouteFinderOptions _options;
        private int[] _previous;
        private HeapPriorityQueue<QueueNode> _queue;
        private int _sourceIndex;
        private int _targetIndex;
        private int _securityPenalty;

        public RouteFinder(IStaticUniverseData staticUniverseData, IWormholeConnectionRepository wormholeConnectionRepository, IEveScoutService eveScoutService)
        {
            _staticUniverseData = staticUniverseData;
            _wormholeConnectionRepository = wormholeConnectionRepository;
            _eveScoutService = eveScoutService;
            _staticSolarSystemInfos = staticUniverseData.AllSystems.ToArray();

            _systemCount = _staticSolarSystemInfos.Length;
            _mapSystemIdToArrayIndex = new Dictionary<int, int>(_systemCount);

            _staticConnections = staticUniverseData.AllConnections;
            Options = new SettingsBasedRouteFinderOptions();
        }

        public StaticSolarSystemInfo[] GetRouteBetween(string source, string target)
        {
            if (source == target)
            {
                return new StaticSolarSystemInfo[0];
            }
            lock (this)
            {
                ResetState();
                InitState(source, target);

                while (_queue.Any())
                {
                    var u = _queue.Dequeue();
                    _isScanned[u.IndexInSystemArray] = true;

                    ProcessNeighbours(u);

                    if (IsTargetUnreachable)
                    {
                        return null;
                    }

                    if (u.IndexInSystemArray == _targetIndex)
                    {
                        return ExtractPath(_sourceIndex, _targetIndex, _previous, _staticSolarSystemInfos);
                    }
                }
                throw new Exception("Error in Dijkstra implementation");
            }
        }

        public IRouteFinderOptions Options
        {
            get { return _options; }
            set
            {
                lock (this)
                {
                    _options = value ?? new RouteFinderOptions();
                }
            }
        }

        private static StaticSolarSystemInfo[] ExtractPath(int sourceIndex,
                                                          int targetIndex,
                                                          int[] previous,
                                                          StaticSolarSystemInfo[] staticSolarSystemInfos)
        {
            var path = new List<StaticSolarSystemInfo>();
            for (var p = targetIndex; p != sourceIndex; p = previous[p])
            {
                path.Add(staticSolarSystemInfos[p]);
            }

            path.Add(staticSolarSystemInfos[sourceIndex]);

            path.Reverse();

            return path.ToArray();
        }

        private void InitState(string source, string target)
        {
            for (var i = 0; i < _systemCount; ++i)
            {
                var staticSolarSystemInfo = _staticSolarSystemInfos[i];
                _mapSystemIdToArrayIndex[staticSolarSystemInfo.Id] = i;
                if (staticSolarSystemInfo.Name == source)
                {
                    _sourceIndex = i;
                    _distance[i] = 0;
                    _isScanned[i] = true;
                }
                else
                {
                    if (staticSolarSystemInfo.Name == target)
                    {
                        _targetIndex = i;
                    }
                    _previous[i] = int.MaxValue;
                    _distance[i] = int.MaxValue;
                }

                var queueNode = new QueueNode(i);
                _nodes[i] = queueNode;
                _queue.Enqueue(queueNode, _distance[i]);
            }
        }

        private bool IsTargetUnreachable
        {
            get
            {
                return Enumerable.Range(0, _systemCount)
                                 .Where(x => !_isScanned[x])
                                 .All(x => _distance[x] == int.MaxValue);
            }
        }

        private int GetDistance(int index)
        {
            var staticSolarSystemInfo = _staticSolarSystemInfos[index];
            if (Options.IsAvoidingSystems && Options.SystemIdsToAvoid.Contains(staticSolarSystemInfo.Id))
            {
                return int.MaxValue;
            }
            int length;
            switch (Options.RouteType)
            {
                case RouteType.Shortest:
                    length = 1;
                    break;
                case RouteType.PreferLow:
                    length = staticSolarSystemInfo.SecurityStatus > 0 && staticSolarSystemInfo.SecurityStatus < 0.45
                                 ? 1
                                 : _securityPenalty;
                    break;
                default:
                    length = staticSolarSystemInfo.SecurityStatus > 0.45 ? 1 : _securityPenalty;
                    break;
            }
            return _distance[index] + length;
        }

        private void ProcessNeighbours(QueueNode u)
        {
            foreach (var curNeighbour in _connections[_staticSolarSystemInfos[u.IndexInSystemArray].Id])
            {
                var indexOfNeighbour = _mapSystemIdToArrayIndex[curNeighbour];
                if (_isScanned[indexOfNeighbour])
                {
                    continue;
                }

                var alt = GetDistance(u.IndexInSystemArray);
                if (alt < _distance[indexOfNeighbour])
                {
                    _distance[indexOfNeighbour] = alt;
                    _previous[indexOfNeighbour] = u.IndexInSystemArray;
                    _queue.UpdatePriority(_nodes[indexOfNeighbour], alt);
                }
            }
        }

        private void ResetState()
        {
            _securityPenalty = Options.SecurityPenality;
            _distance = new int[_systemCount];
            _previous = new int[_systemCount];
            _isScanned = new bool[_systemCount];
            _nodes = new QueueNode[_systemCount];
            _sourceIndex = -1;
            _targetIndex = -1;

            _queue = new HeapPriorityQueue<QueueNode>(8030);
            
            if (Options.IsIgnoringWormholes)
            {
                _connections = _staticConnections;
                return;
            }

            var customWormholeConnections = _wormholeConnectionRepository.GetAllWormholeConnections();
            var customIdConnections = customWormholeConnections.SelectMany(ToIdConnections)
                                                               .ToLookup(x => x.Key, x => x.Value);

            var eveScoutConnections = _eveScoutService.GetConnectionsToThera();
            var eveScoutIdConnections = eveScoutConnections.SelectMany(ToIdConnections)
                                                           .ToLookup(x => x.Key, x => x.Value);

            _connections = _staticConnections.Concat(customIdConnections)
                                             .Concat(eveScoutIdConnections)
                                             .SelectMany(x => x.Select(y => new KeyValuePair<int, int>(x.Key, y)))
                                             .ToLookup(x => x.Key, x => x.Value);
        }

        private IEnumerable<KeyValuePair<int,int>> ToIdConnections(IWormholeConnection arg)
        {
            var firstId = _staticUniverseData.GetSystemByName(arg.FirstSystem)
                                             .Id;
            var secondId = _staticUniverseData.GetSystemByName(arg.SecondSystem)
                                              .Id;
            return new[] {new KeyValuePair<int, int>(firstId, secondId), new KeyValuePair<int, int>(secondId, firstId)};
        }
    }
}
