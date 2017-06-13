using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EVE_Killboard_Analyser.Helper.Gatecamp;
using log4net;
using Newtonsoft.Json;
using PLHLib;
using WebSocketSharp;

namespace EveLocalChatAnalyser.Utilities.GateCampDetection
{
    public delegate void GateCampAdded(GateCampMessageModel gateCamp);

    public delegate void GateCampRemoved(GateCampMessageModel gateCamp);

    public delegate void GateCampIndexChanged(GateCampMessageModel gateCamp);

    public interface IGateCampDetectionService
    {
        event GateCampAdded GateCampAdded;

        event GateCampRemoved GateCampRemoved;

        event GateCampIndexChanged GateCampIndexChanged;
        //TODO thread safety
        GateCampMessageModel[] GateCamps { get; } 
    }

    internal sealed class GateCampDetectionService : IGateCampDetectionService, IDisposable
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (GateCampDetectionService));
        private readonly WebSocket _ws;
        private volatile bool _isClosed;
        private readonly List<GateCampMessageModel> _gateCamps = new List<GateCampMessageModel>();

        public GateCampDetectionService()
        {
            _ws = new WebSocket("ws://rischwa.net/killboard/api/GateCampsV1");
            _ws.OnMessage += WsOnOnMessage;
            _ws.OnError += WsOnOnError;
            _ws.OnClose += WsOnOnClose;

            StartupWebservice();
        }

        public void Dispose()
        {
            try
            {
                _isClosed = true;
                _ws.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public event GateCampAdded GateCampAdded;

        public event GateCampRemoved GateCampRemoved;

        public event GateCampIndexChanged GateCampIndexChanged;

        public GateCampMessageModel[] GateCamps { get { lock(this){ return _gateCamps.ToArray();} } }

        private void StartupWebservice()
        {
            _ws.ConnectAsync();
        }

        private void WsOnOnClose(object sender, CloseEventArgs closeEventArgs)
        {
            if (!_isClosed)
            {
                TryReconnectingEveryMinute();
            }
        }

        private void WsOnOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            //try to reconnect every minute after an error
            TryReconnectingEveryMinute();
        }

        private void TryReconnectingEveryMinute()
        {
            TaskEx.Delay(new TimeSpan(0, 1, 0))
                .ContinueWith(t => { StartupWebservice(); })
                .ConfigureAwait(false);
        }

        private void WsOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<GateCampMessage>(messageEventArgs.Data);
                switch (message.GateCampMessageType)
                {
                    case GateCampMessageType.ADD:
                        OnGateCampAdded(message.GateCamp);
                        break;
                    case GateCampMessageType.REMOVE:
                        OnGateCampRemoved(message.GateCamp);
                        break;
                    case GateCampMessageType.CHANGE:
                        OnGateCampIndexChanged(message.GateCamp);
                        break;
                }
            }
            catch (Exception e)
            {
                LOGGER.Warn("Error in gatecamp deserialization", e);
            }
        }

        private void OnGateCampAdded(GateCampMessageModel gatecamp)
        {
            lock (this)
            {
                _gateCamps.Add(gatecamp);
            }
            Application.Current.Dispatcher.BeginInvoke(new Action(() => { GateCampAdded?.Invoke(gatecamp); }));
        }

        private void OnGateCampRemoved(GateCampMessageModel gatecamp)
        {
            lock (this)
            {
                var gateCampToRemove = _gateCamps.FirstOrDefault(x => x.StargateLocations.HasIntersection(gatecamp.StargateLocations));
                if (gateCampToRemove != null)
                {

                    _gateCamps.Remove(gateCampToRemove);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() => { GateCampRemoved?.Invoke(gateCampToRemove); }));
                }
            }
        }

        private void OnGateCampIndexChanged(GateCampMessageModel gatecamp)
        {
            var gateCampToChange = _gateCamps.FirstOrDefault(x => x.StargateLocations.HasIntersection(gatecamp.StargateLocations));
            if (gateCampToChange != null)
            {
                gateCampToChange.StargateLocations = gateCampToChange.StargateLocations;
                gateCampToChange.GateCampIndex = gatecamp.GateCampIndex;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { GateCampIndexChanged?.Invoke(gateCampToChange); }));
            }
        }
    }
}
