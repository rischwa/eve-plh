using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public sealed class GateCampDifferenceDetector : IGateCampDifferenceDetector
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (GateCampDifferenceDetector));
        private List<GateCamp> _previousGateCamps;

        public event GateCampAdded GateCampAdded;

        public event GateCampRemoved GateCampRemoved;

        public event GateCampIndexChanged GateCampIndexChanged;

        public void SetNextStatus(IReadOnlyCollection<GateCamp> gateCamps)
        {
            if (_previousGateCamps == null)
            {
                InitialDetection(gateCamps);
                return;
            }

            NextIterativeDetection(gateCamps);
        }

        private void NextIterativeDetection(IReadOnlyCollection<GateCamp> gateCamps)
        {
            //LOGGER.Debug($"previous gatecamp count: {_previousGateCamps.Count}; new gatecamp count: {gateCamps.Count}");
            foreach (var curGateCamp in gateCamps)
            {
                var isNew = true;
                foreach (var curPreviousGateCamp in _previousGateCamps)
                {
                  
                    if (
                        curGateCamp.IsAtSameLocation(curPreviousGateCamp.StargateLocations))
                    {
                        if (Math.Abs(curGateCamp.GateCampIndex - curPreviousGateCamp.GateCampIndex) > 0.01)
                        {
                            OnGateCampIndexChanged(curGateCamp);
                        }
                        //LOGGER.Debug("ignoring: " + JsonConvert.SerializeObject(curPreviousGateCamp.StargateLocations));
                        _previousGateCamps.Remove(curPreviousGateCamp);
                        isNew = false;
                        break;
                    }
                 
                }
                if (isNew)
                {
                    OnGateCampAdded(curGateCamp);
                }
            }

            foreach (var curPreviousGateCamp in _previousGateCamps)
            {
                OnGateCampRemoved(curPreviousGateCamp);
            }

            _previousGateCamps = gateCamps.ToList();
        }

        private void InitialDetection(IReadOnlyCollection<GateCamp> gateCamps)
        {
            _previousGateCamps = gateCamps.ToList();
            foreach (var curGateCamp in gateCamps)
            {
                OnGateCampAdded(curGateCamp);
            }
        }

        private void OnGateCampAdded(GateCamp gatecamp)
        {
            GateCampAdded?.Invoke(gatecamp);
        }

        private void OnGateCampRemoved(GateCamp gatecamp)
        {
            GateCampRemoved?.Invoke(gatecamp);
        }

        private void OnGateCampIndexChanged(GateCamp gatecamp)
        {
            GateCampIndexChanged?.Invoke(gatecamp);
        }
    }
}