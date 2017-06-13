using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using EVE_Killboard_Analyser.Controllers;
using log4net;
using Newtonsoft.Json;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public class GateCampDetectionService
    {
        private static readonly TimeSpan THIRTY_SECONDS = new TimeSpan(0, 0, 30);
        private readonly IGateCampDetector _gateCampDetector;
        private readonly IGateCampDifferenceDetector _gateCampDifferenceDetector;
        private readonly ConcurrentQueue<Kill> _killsToAdd = new ConcurrentQueue<Kill>();
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (GateCampDetectionService));

        public GateCampDetectionService(IGateCampDetector gateCampDetector, IGateCampDifferenceDetector gateCampDifferenceDetector)
        {
            _gateCampDetector = gateCampDetector;
            _gateCampDifferenceDetector = gateCampDifferenceDetector;
        }

        private void GateCampDifferenceDetectorOnGateCampIndexChanged(GateCamp gateCamp)
        {
            //TODO index for gatecamps?
         
            BroadcastGatecampMessage(gateCamp, GateCampMessageType.CHANGE);
        }

        private void GateCampDifferenceDetectorOnGateCampRemoved(GateCamp gateCamp)
        {
            //TODO index for gatecamps?
            BroadcastGatecampMessage(gateCamp, GateCampMessageType.REMOVE);
        }

        private void GateCampDifferenceDetectorOnGateCampAdded(GateCamp gateCamp)
        {
            //TODO index for gatecamps?
            BroadcastGatecampMessage(gateCamp, GateCampMessageType.ADD);
        }

        private void BroadcastGatecampMessage(GateCamp gateCamp, GateCampMessageType gateCampMessageType)
        {
            var gateCampMessage = new GateCampMessage
                                  {
                                      GateCampMessageType = gateCampMessageType,
                                      GateCamp = new GateCampMessageModel(gateCamp)
                                  };

            var gateCampMessageString = JsonConvert.SerializeObject(gateCampMessage);

            LOGGER.Debug("BroadcastGateCampMessage:\n" + gateCampMessageString);
            //TODO async
            GateCampsWebSocketHandler.Broadcast(gateCampMessageString);
        }

        public void Start()
        {
            BindGateCampDifferenceDetectorEvents();
            InitGateCampsFromLastHour();

            while (true)
            {
                try
                {
                    if (_killsToAdd.Any())
                    {
                        Kill curKill;
                        while (_killsToAdd.TryDequeue(out curKill))
                        {
                            _gateCampDetector.AddKill(curKill);
                        }
                        _gateCampDetector.DetectGateCamps();

                        _gateCampDifferenceDetector.SetNextStatus(_gateCampDetector.GateCamps);
                    }
                }
                catch (Exception e)
                {
                    LOGGER.Error("Error in GateCampDetectionService", e);
                }

                Thread.Sleep(THIRTY_SECONDS);
            }
        }

     

        public void AddKill(Kill kill)
        {
            _killsToAdd.Enqueue(kill);
        }

        private void BindGateCampDifferenceDetectorEvents()
        {
            _gateCampDifferenceDetector.GateCampAdded += GateCampDifferenceDetectorOnGateCampAdded;
            _gateCampDifferenceDetector.GateCampRemoved += GateCampDifferenceDetectorOnGateCampRemoved;
            _gateCampDifferenceDetector.GateCampIndexChanged += GateCampDifferenceDetectorOnGateCampIndexChanged;
        }

        private void InitGateCampsFromLastHour()
        {
            LOGGER.Debug("Starting GateCampDetection Initialization");
            try
            {
                //declare the transaction options
                var transactionOptions = new System.Transactions.TransactionOptions
                                         {
                                             IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                                         };
                //set it to read uncommited
                //create the transaction scope, passing our options in
                using (
                    var transactionScope = new System.Transactions.TransactionScope(
                        System.Transactions.TransactionScopeOption.Required,
                        transactionOptions))
                {


                    var start = DateTime.UtcNow;
                    using (var context = new DatabaseContext())
                    {
                        context.ObjectContext.CommandTimeout = 300;
                        var startDateTime = DateTime.UtcNow - new TimeSpan(1, 0, 0);
                        var killsInTheLastHour = context.Kills.Where(kill => kill.KillTime > startDateTime)
                            .AsQueryable()
                            .Include(kill => kill.Attackers)
                            .Include(kill => kill.Victim)
                            .ToList()
                            .Select(kill => new KillResult(kill));

                        transactionScope.Complete();
                        _gateCampDetector.AddRange(killsInTheLastHour);

                        _gateCampDetector.DetectGateCamps();

                        LOGGER.Debug("Initial gate camps detected: " + _gateCampDetector.GateCamps.Count);

                        _gateCampDifferenceDetector.SetNextStatus(_gateCampDetector.GateCamps);
                    }
                    LOGGER.Debug("Loaded initial kills in " + (DateTime.UtcNow - start).TotalSeconds + "s");
                }
            }
            catch (Exception e)
            {
                LOGGER.Error("ERROR initializing gate camp detection", e);
                throw;
            }
        }
    }
}
