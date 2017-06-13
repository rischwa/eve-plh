using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EveLocalChatAnalyser.Model;

namespace EveLocalChatAnalyser.Utilities.PositionTracking
{
    public enum WormholeMassStatus
    {
        Unknown = 0,
        Full,
        Reduced,
        Critical
    }

    public enum WormholeLifetimeStatus
    {
        Unknown = 0,
        Forming,
        Main,
        Critical,
        Closed
    }

    public class WormholeLifetimeUpdate
    {
        public DateTime Time { get; set; }

        public WormholeLifetimeStatus LifetimeStatus { get; set; }
    }

    //public class WormholeConnectionVersion1ToVersion2Converter : IDocumentConversionListener
    //{
    //    public void EntityToDocument(object entity, RavenJObject document, RavenJObject metadata)
    //    {
    //        var c = entity as WormholeConnection;
    //        if (c == null)
    //            return;

    //        metadata["Schema-Version"] = 2;
            
    //    }

    //    public void DocumentToEntity(object entity, RavenJObject document, RavenJObject metadata)
    //    {
    //        Customer c = entity as Customer;
    //        if (c == null)
    //            return;
    //        if (metadata.Value<int>("Customer-Schema-Version") >= 2)
    //            return;

    //        c.FirstName = document.Value<string>("Name").Split().First();
    //        c.LastName = document.Value<string>("Name").Split().Last();
    //        c.CustomerEmail = document.Value<string>("Email");
    //    }
    //}

    //TODO die einzuhaltende reihenfolge der systemnamen ist scheiss design, entweder muss das enforced werden, oder egal sein

    public delegate void WormholeConnectionsPotentiallyClosed(IEnumerable<WormholeConnection> whConnection);
    public interface IWormholeConnectionTracker
    {
        event WormholeConnectionClosed WormholeConnectionClosed;

        event WormholeConnectionUpdate WormholeConnectionUpdate;

        event WormholeConnectionCreated WormholeConnectionCreated;

        event WormholeConnectionsPotentiallyClosed WormholeConnectionsPotentiallyClosed;

        
        
        void CloseWormholeConnection(WormholeConnection whConnection);

        void CloseWormholeConnection(string firstSystem, string secondSystem);

        void InsertWormholeConnection(WormholeConnection whConnection);

        void UpdateWormholeConnection(WormholeConnection whConnection);
    }

    public delegate void WormholeConnectionCreated(WormholeConnection whConnection);

    public delegate void WormholeConnectionUpdate(WormholeConnection whConnection);

    public delegate void WormholeConnectionClosed(WormholeConnection whConnection);

    public class WormholeConnectionTracker : IWormholeConnectionTracker
    {
        public static readonly Regex WH_REGEX = new Regex("^((J\\d{6,6}$)|(Thera))$");
        private readonly ClipboardParser _clipboardParser;
        private readonly IPositionTracker _positionTracker;
        private readonly IWormholeConnectionRepository _repository;

        public WormholeConnectionTracker(IPositionTracker positionTracker, IWormholeConnectionRepository repository,
                                         ClipboardParser clipboardParser)
        {
            _positionTracker = positionTracker;
            _repository = repository;
            _clipboardParser = clipboardParser;

            _clipboardParser.ProbeScan += ClipboardParserOnProbeScan;
            _positionTracker.SystemChanged += PositionTrackerOnSystemChanged;
        }


        public event WormholeConnectionClosed WormholeConnectionClosed;
        public event WormholeConnectionUpdate WormholeConnectionUpdate;

        public event WormholeConnectionCreated WormholeConnectionCreated;
        public event WormholeConnectionsPotentiallyClosed WormholeConnectionsPotentiallyClosed;
       

        public void CloseWormholeConnection(WormholeConnection whConnection)
        {
            whConnection.LastLifetimeUpdate = new WormholeLifetimeUpdate
                {
                    Time = DateTime.UtcNow,
                    LifetimeStatus = WormholeLifetimeStatus.Closed
                };

            _repository.DeleteWormholeConnection(whConnection);

            OnWormholeConnectionClosed(whConnection);
        }

        public void CloseWormholeConnection(string firstSystem, string secondSystem)
        {
            string first, second;
            if (firstSystem.CompareTo(secondSystem) < 0)
            {
                first = firstSystem;
                second = secondSystem;
            }
            else
            {
                first = secondSystem;
                second = firstSystem;
            }

            //TODO repository sollte die ordnung herstellen
            var connection = _repository.GetWormholeConnection(first, second);
            if (connection == null)
            {
                return;
            }

            CloseWormholeConnection(connection);
        }

        public void InsertWormholeConnection(WormholeConnection whConnection)
        {
            _repository.UpsertWormholeConnection(whConnection);

            OnWormholeConnectionCreated(whConnection);
        }

        public void UpdateWormholeConnection(WormholeConnection whConnection)
        {
            _repository.UpsertWormholeConnection(whConnection);

            OnWormholeConnectionUpdate(whConnection);
        }

        private void ClipboardParserOnProbeScan(IList<IProbeScanItem> probeScanItems)
        {
            var currentSystemOfActiveCharacter = _positionTracker.CurrentSystemOfActiveCharacter;
            if (currentSystemOfActiveCharacter == null)
            {
                return;
            }

            var signatures = probeScanItems.Select(x => x.Name).ToArray();
            
            var potentiallyClosedConnections =
                _repository.GetWormholeConnectionsForSystem(currentSystemOfActiveCharacter)
                           .Where(
                               connection => IsConnectingSignatureMissing(signatures, currentSystemOfActiveCharacter, connection))
                           .ToList();

            OnWormholeConnectionsPotentiallyClosed(potentiallyClosedConnections);
        }

        private static bool IsConnectingSignatureMissing(string[] signatures, string currentSystemOfActiveCharacter,
                                                WormholeConnection connection)
        {
            var signatureId = connection.FirstSystem == currentSystemOfActiveCharacter
                                  ? connection.FirstToSecondSignature
                                  : connection.SecondToFirstSignature;
            return signatureId != null && !signatures.Contains(connection.FirstToSecondSignature);
        }


        private void PositionTrackerOnSystemChanged(string character, string oldSystem, string newSystem)
        {
            if (UniverseDataDB.AreSystemsConnected(oldSystem, newSystem))
            {
                return;
            }

            string firstSystem, secondSystem;
            if (string.Compare(oldSystem, newSystem, StringComparison.Ordinal) < 0)
            {
                firstSystem = oldSystem;
                secondSystem = newSystem;
            }
            else
            {
                firstSystem = newSystem;
                secondSystem = oldSystem;
            }

            var whConnection = _repository.GetWormholeConnection(firstSystem, secondSystem);
            if (whConnection != null)
            {
                return;
            }

            whConnection = new WormholeConnection
                {
                    FirstSystem = firstSystem,
                    SecondSystem = secondSystem
                };

            _repository.UpsertWormholeConnection(whConnection);

            OnWormholeConnectionCreated(whConnection);
        }

        private static bool IsWormholeSystemName(string oldSystem)
        {
            return WH_REGEX.IsMatch(oldSystem);
        }

        protected virtual void OnWormholeConnectionsPotentiallyClosed(IEnumerable<WormholeConnection> whconnection)
        {
            var handler = WormholeConnectionsPotentiallyClosed;
            if (handler != null) handler(whconnection);
        }

        protected virtual void OnWormholeConnectionClosed(WormholeConnection whconnection)
        {
            var handler = WormholeConnectionClosed;
            if (handler != null)
            {
                //TODO on gui thread?
                handler.Invoke(whconnection);
            }
        }

        protected virtual void OnWormholeConnectionUpdate(WormholeConnection whconnection)
        {
            var handler = WormholeConnectionUpdate;
            if (handler != null)
            {
                handler.Invoke(whconnection);
            }
        }

        protected virtual void OnWormholeConnectionCreated(WormholeConnection whconnection)
        {
            var handler = WormholeConnectionCreated;
            if (handler != null)
            {
                handler.Invoke(whconnection);
            }
        }
    }
}