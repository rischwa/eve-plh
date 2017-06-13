using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using EveLocalChatAnalyser.Exceptions;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Ui;
using EveLocalChatAnalyser.Utilities;
using log4net;
using Newtonsoft.Json;
using PLHLib;

namespace EveLocalChatAnalyser.Services
{
    public static class KillboardAnalysisService
    {
        private static readonly LinkedList<IEveCharacter> REQUEST_QUEUE = new LinkedList<IEveCharacter>();
        private static readonly Thread THREAD = new Thread(Run);
        private static readonly ILog LOG = LogManager.GetLogger("KillboardAnalysis");

        static KillboardAnalysisService()
        {
            THREAD.IsBackground = true;
            THREAD.Start();
        }

        private static void Run()
        {
            while (true)
            {
                IEveCharacter character = null;
                try
                {
                    lock (REQUEST_QUEUE)
                    {
                        if (!REQUEST_QUEUE.Any())
                        {
                            Monitor.Wait(REQUEST_QUEUE);
                            continue;
                        }
                        character = REQUEST_QUEUE.First();
                        REQUEST_QUEUE.RemoveFirst();
                        if (character.KillboardInformation != null)
                        {
                            continue;
                        }
                    }
                    character.KillboardInformation = GetKillboardAnalysisFor(int.Parse(character.Id));
                }
                catch (Exception e)
                {
                    var message = character == null
                                      ? "Error during KB-Data retrieval"
                                      : string.Format("Could not retrieve killboard information for {0}", character);
                    LOG.Warn(message, e);
                    Application.Current.Dispatcher.Invoke(
                                                          new Action(
                                                              () =>
                                                              {
                                                                  if (Application.Current != null && Application.Current.MainWindow != null)
                                                                  {
                                                                      ((MainWindow) Application.Current.MainWindow).SetTitleToError(message);
                                                                  }
                                                              }));
                }
            }
        }

        public static void OnLocalChangedTo(IList<IEveCharacter> eveCharacters)
        {
            if (eveCharacters.Count > 200)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                                           new Action(
                                                               () =>
                                                               MessageBox.Show(
                                                                               Application.Current.MainWindow,
                                                                               "Local is too full, skipping killboard analysis",
                                                                               "Warning")));
                return;
            }
            if (!Settings.Default.IsUsingKillboardAnalysis)
            {
                return;
            }
            lock (REQUEST_QUEUE)
            {
                //keep the existing ones, so if someone got manually added to front of list, he keeps being at the front
                var noLongerIncludedOnes = REQUEST_QUEUE.Except(eveCharacters)
                    .ToList();
                foreach (var curChar in noLongerIncludedOnes)
                {
                    REQUEST_QUEUE.Remove(curChar);
                }

                foreach (var curChar in
                    REQUEST_QUEUE.Where(eveCharacter => eveCharacter.LocalChangeStatus == LocalChangeStatus.Exited)
                        .ToList())
                {
                    REQUEST_QUEUE.Remove(curChar);
                }

                var newOnes = eveCharacters.Except(REQUEST_QUEUE)
                    .Where(eveCharacter => eveCharacter.LocalChangeStatus != LocalChangeStatus.Exited);
                foreach (var eveCharacter in newOnes)
                {
                    REQUEST_QUEUE.AddLast(eveCharacter);
                }
                Monitor.Pulse(REQUEST_QUEUE);
            }
        }

        public static void AddLast(IEveCharacter character)
        {
            lock (REQUEST_QUEUE)
            {
                REQUEST_QUEUE.AddLast(character);
                Monitor.Pulse(REQUEST_QUEUE);
            }
        }

        public static void AddFirst(IEveCharacter character)
        {
            lock (REQUEST_QUEUE)
            {
                REQUEST_QUEUE.AddFirst(character);
                Monitor.Pulse(REQUEST_QUEUE);
            }
        }

        public static KillboardInformation GetKillboardAnalysisFor(int characterId)
        {
            var url = string.Format(RischwaNetService.BASE_URL + "/CharactersV1/{0}", characterId);
            var response = WebUtilities.GetHttpGetResponseFrom(url);

            var message = JsonConvert.DeserializeObject<KillboardMessage>(response);
            if (message == null || message.Data == null || message.Status != "success")
            {
                throw new EveLocalChatAnalyserException(
                    string.Format(
                                  "Could not retrieve killboard analysis for char {0}; Message: {1}",
                                  characterId,
                                  message != null ? message.Message : "none"));
            }

            return message.Data;
        }
    }
}
