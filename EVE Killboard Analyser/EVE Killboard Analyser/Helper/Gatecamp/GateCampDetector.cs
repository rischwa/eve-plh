using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EveLocalChatAnalyser.Utilities.RouteFinding;
using log4net;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public delegate void GateCampAdded(GateCamp gateCamp);

    public delegate void GateCampRemoved(GateCamp gateCamp);

    public delegate void GateCampIndexChanged(GateCamp gateCamp);

    public sealed class GateCampDetector : IGateCampDetector
    {
        private static readonly TimeSpan FIVE_MIN = new TimeSpan(0, 5, 0);
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (GateCampDetector));
        private readonly HeapPriorityQueue<KillNode> _queue = new HeapPriorityQueue<KillNode>(10000);
        private readonly IStargateLocationRepository _stargateLocationRepository;
        private List<GateCamp> _gateCamps = new List<GateCamp>();

        public GateCampDetector(IStargateLocationRepository stargateLocationRepository)
        {
            _stargateLocationRepository = stargateLocationRepository;
        }

        public IReadOnlyCollection<GateCamp> GateCamps => _gateCamps.AsReadOnly();

        public void AddRange(IEnumerable<KillResult> kills)
        {
            foreach (var curKill in kills)
            {
                StargateLocation location;
                _stargateLocationRepository.TryGetStargateLocation(curKill.SolarSystemId, curKill.Position, out location);

                _queue.Enqueue(new KillNode(curKill, location) , GetPriority(curKill));
            }
        }

        public void DetectGateCamps()
        {
            _gateCamps = new List<GateCamp>();
            var beforeSize = _queue.Count;
            CleanupQueue();
            foreach (var curKill in _queue)
            {
                if (curKill.StargateLocation != null)
                {
                    HandleGateKill(curKill.Kill, curKill.StargateLocation);
                }
                else
                {
                    ChangePreviousGatecamps(curKill.Kill);
                }
            }
            LOGGER.Debug($"Detected GateCamps; Queue before: {beforeSize}; Queue after: {_queue.Count}; Gate camps: {GateCamps.Count}");
        }

        public void AddKill(Kill kill)
        {
            var lastHour = GetLastHour();
            if (kill.KillTime < lastHour)
            {
                return;
            }


            var killResult = new KillResult(kill);

            StargateLocation location;
            _stargateLocationRepository.TryGetStargateLocation(killResult.SolarSystemId, killResult.Position, out location);
            _queue.Enqueue(new KillNode(killResult, location), GetPriority(killResult));
        }

        private void CleanupQueue()
        {
            var oneHour = new TimeSpan(1, 0, 0);
            var oneHourAgo = DateTime.UtcNow.Subtract(oneHour);
            while (_queue.Count != 0 && _queue.First.Kill.KillTime < oneHourAgo)
            {
                _queue.Dequeue();
            }
        }

        private void HandleGateKill(KillResult kill, StargateLocation location)
        {
            var stargateLocations = new[] { location };
            var existingGateCampByLocation =
                _gateCamps.FirstOrDefault(x=> x.IsAtSameLocation(stargateLocations));

            if (existingGateCampByLocation == null)
            {
                HandleNewGateCamp(kill, location);
            }
            else
            {
                HandleExistingGateCamp(existingGateCampByLocation, kill, location);
            }
        }

        private void HandleExistingGateCamp(GateCamp existingGateCampByLocation, KillResult kill, StargateLocation location)
        {
            existingGateCampByLocation.Kills.Add(kill);
            if (!existingGateCampByLocation.StargateLocations.Contains(location))
            {
                existingGateCampByLocation.StargateLocations.Add(location);
            }
            existingGateCampByLocation.GateCampIndex = CalculateGateCampIndex(existingGateCampByLocation.Kills);
        }

        private static double CalculateGateCampIndex(IEnumerable<KillResult> kills)
        {
            //TODO orderby redundant??
            var timeOrderedKills = kills.OrderBy(x => x.KillTime)
                .ToArray();

            var parts = GetKillClusterCount(timeOrderedKills);

            var last30Min = DateTime.UtcNow - new TimeSpan(0, 30, 0);

            var wasInLast30Mins = timeOrderedKills.Last()
                                      .KillTime > last30Min;
            var hasGateCampKills = timeOrderedKills.Any(x => x.Attackers.Any(IsGateCampShip));

            if (parts == 1)
            {
                if (wasInLast30Mins)
                {
                    return hasGateCampKills ? 0.6 : 0.3;
                }
                return hasGateCampKills ? 0.3 : 0.1;
            }

            if (wasInLast30Mins)
            {
                return hasGateCampKills ? 1 : 0.7;
            }
            return hasGateCampKills ? 0.8 : 0.6;
        }

        private static int GetKillClusterCount(IEnumerable<KillResult> timeOrderedKills)
        {
            var parts = 0;
            var start = DateTime.MinValue;
            foreach (var curKill in timeOrderedKills)
            {
                if ((curKill.KillTime - start) > FIVE_MIN)
                {
                    parts += 1;
                }
                start = curKill.KillTime;
            }
            return parts;
        }

        private void HandleNewGateCamp(KillResult kill, StargateLocation location)
        {
            ChangePreviousGatecamps(kill);
            CreateNewGateCamp(kill, location);
        }

        private void ChangePreviousGatecamps(KillResult kill)
        {
            var gateCampsToRemove = new List<GateCamp>();
            foreach (var curGateCamp in _gateCamps)
            {
                var sameAttackerCoefficient = GetSameAttackerCoefficent(curGateCamp.Kills, kill);

                if (sameAttackerCoefficient >= 1) //all people of gc moved to another gc
                {
                    gateCampsToRemove.Add(curGateCamp);
                }
                else
                {
                    //TODO wert sollte evtl. anders sein
                    curGateCamp.GateCampIndex = Math.Max(0.1d, curGateCamp.GateCampIndex * (1.0 - sameAttackerCoefficient));
                    //OnGateCampIndexChanged(curGateCamp);
                }
            }
            foreach (var curGateCamp in gateCampsToRemove)
            {
                _gateCamps.Remove(curGateCamp);
                //OnGateCampRemoved(curGateCamp);
            }
        }

        private static double GetSameAttackerCoefficent(IEnumerable<KillResult> kills, KillResult kill)
        {
            var players = kills.SelectMany(
                                           x => x.Attackers.Where(IsPlayer)
                                                    .Select(a => a.CharacterID))
                .Distinct()
                .ToArray();
            var countOnNew = kill.Attackers.Where(IsPlayer)
                .Count(x => players.Contains(x.CharacterID));

            return ((double) countOnNew) / players.Length;
        }

        private static bool IsPlayer(Attacker arg)
        {
            return arg.CharacterID != 0;
        }

        private void CreateNewGateCamp(KillResult kill, StargateLocation location)
        {
            var killResults = new List<KillResult>
                              {
                                  kill
                              };
            var gateCampIndex = CalculateGateCampIndex(killResults);
            var gateCamp = new GateCamp
                           {
                               GateCampIndex = gateCampIndex,
                               StargateLocations = new List<StargateLocation>

                                                   {
                                                       location
                                                   },
                               Kills = killResults
                           };
            _gateCamps.Add(gateCamp);
        }

        private static bool IsGateCampShip(Attacker attacker)
        {
            return Types.IsGateCampShip(attacker.ShipTypeID, attacker.WeaponTypeID);
        }

        private static double GetPriority(KillResult curKill)
        {
            return double.MaxValue - curKill.KillTime.ToOADate();
        }

        private static DateTime GetLastHour()
        {
            var oneHour = new TimeSpan(1, 0, 0);
            return DateTime.UtcNow.Subtract(oneHour);
        }
    }
}
