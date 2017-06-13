using System;
using EVE_Killboard_Analyser.Helper.Gatecamp;
using log4net;
using Microsoft.Web.WebSockets;
using Newtonsoft.Json;

namespace EVE_Killboard_Analyser.Controllers
{
    public class GateCampsWebSocketHandler : WebSocketHandler
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(GateCampsWebSocketHandler));
        private static readonly WebSocketCollection WEB_SOCKET_CLIENTS = new WebSocketCollection();
        public static IGateCampDetector GateCampDetector { get; set; }

        public override void OnOpen()
        {
            base.OnOpen();
            WEB_SOCKET_CLIENTS.Add(this);

            LOGGER.Debug("WebServiceConnection to GateCampDetector opened");
            try
            {
                foreach (var curGateCamp in GateCampDetector.GateCamps)
                {
                    var gateCampMessage = new GateCampMessage
                                          {
                                              GateCampMessageType = GateCampMessageType.ADD,
                                              GateCamp = new GateCampMessageModel(curGateCamp)
                                          };
                    Send(JsonConvert.SerializeObject(gateCampMessage));
                }
            }
            catch (Exception e)
            {
                LOGGER.Error("error in sending initial gatecamps", e);
                throw;
            }
        }

        public override void OnClose()
        {
            WEB_SOCKET_CLIENTS.Remove(this);
            base.OnClose();
            LOGGER.Debug("WebServiceConnection to GateCampDetector closed");
        }

        public override void OnMessage(string message)
        {
        }

        public override void OnError()
        {
            LOGGER.Error("Error in websocket server", Error);
        }

        public static void Broadcast(string message)
        {
            WEB_SOCKET_CLIENTS.Broadcast(message);
        }
    }
}