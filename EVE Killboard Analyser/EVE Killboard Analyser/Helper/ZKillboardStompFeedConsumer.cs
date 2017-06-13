using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using EVE_Killboard_Analyser.Helper.DatabaseWriter;
using EVE_Killboard_Analyser.Models;
using Newtonsoft.Json;
using PLHLib;
using StompDotNet;
using StompDotNet.Message.Server;
using log4net;

namespace EVE_Killboard_Analyser.Helper
{
    public class ZKillboardStompFeedConsumer
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (ZKillboardStompFeedConsumer));
        private static Client _client;
        private static readonly BlockingCollection<Kill> KILLS = new BlockingCollection<Kill>();
        private static CancellationTokenSource _cancellationTokenSource;

        public static void Start()
        {
            while (true)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                if (true)
                {
                    try
                    {
                        LOG.Debug(string.Format("Connecting to zkillboard stomp server"));

                        _client = new Client("stomp.eve-kill.net", 61613, new Client.Authentication("rischwa", "6364d3f0f495b6ab9dcf8d3b5c6e0b01"));
                        _client.ErrorMessageReceived += ClientOnErrorMessageReceived;
                        _client.Disconnected += ClientOnDisconnected;
                        _client.Subscribe("/topic/kills", KillMessageReceived);

                        LOG.Debug(string.Format("Successfully connected to stomp server"));

                        var token = _cancellationTokenSource.Token;
                        while (!token.IsCancellationRequested)
                        {
                            var kill = KILLS.Take(token);
                            KillEntryWriter.Instance.ForceAdd(new KillEntries() { CharacterId = -1, Kills = new List<Kill> { kill } });
                            //LOG.Debug(string.Format("Wrote kill from stomp message in {0}s", (DateTime.UtcNow - startTime).TotalSeconds));
                        }

                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception e)
                    {
                        LOG.Warn("Error in stomp connection: " + e.Message, e);
                    }
                }
                Thread.Sleep(30000);
            }
        }

        private static void ClientOnDisconnected(Exception exception)
        {
            LOG.Warn(string.Format("Exception in stomp client: {0}", exception.Message),exception);
            CancelClient();
        }

        private static void KillMessageReceived(MessageMessage messageMessage)
        {
            try
            {
                var kill = JsonConvert.DeserializeObject<Kill>(messageMessage.Body);
                InitKillIds(kill);
                KILLS.Add(kill);
                
            }
            catch (Exception)
            {
                LOG.Warn("Could not deserialize to kill: " + messageMessage.Body);
            }
        }

        private static void InitKillIds(Kill kill)
        {
            var killID = kill.KillID;
            kill.Victim.KillID = killID;
            foreach (var curAttacker in kill.Attackers)
            {
                curAttacker.KillID = killID;
            }
            foreach (var curItem in kill.Items)
            {
                curItem.KillID = killID;
            }
        }

        private static void ClientOnErrorMessageReceived(ErrorMessage errorMessage)
        {
            LOG.Warn(string.Format("STOMP: Received error message: {0}\n{1}", errorMessage.HeaderMessage, errorMessage.BodyMessage));
            CancelClient();
        }

        private static void CancelClient()
        {
            _client.ErrorMessageReceived -= ClientOnErrorMessageReceived;
            _client.Disconnected -= ClientOnDisconnected;
            LOG.Debug("Cancelled stomp connection");
            _cancellationTokenSource.Cancel();
            _client.Dispose();
        }
    }
}