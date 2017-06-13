#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Ui;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using EveLocalChatAnalyser.Utilities.PosMapper;
using log4net;
using LiteDB;

#endregion

namespace EveLocalChatAnalyser
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog LOG = LogManager.GetLogger("Main");
        private static readonly int CURRENT_DB_VERSION = 3;
        private bool _isInitialized;
        private LocalChatAnalyser _localChatAnalyser;
        private IDictionary<Window, WindowState> _nonMinimizedStatus = new Dictionary<Window, WindowState>();

        static App()
        {
            try
            {
                InitDatabase();
                SetupDefaultConfiguration();
            }
            catch (Exception e)
            {
                LOG.Error("Error during startup", e);
            }
        }

        public static string DatabasePath
        {
            get { return GetAndEnsurePathExists(DATABASE_PATH); }
        }

        public static string DataPath
        {
            get { return GetAndEnsurePathExists(BASE_PATH); }
        }

        public static LiteEngine CreateStorageEngine()
        {
            GetAndEnsurePathExists(BASE_PATH);
            return new LiteEngine(DATABASE_PATH);
        }

        public static IEnumerable<RT> GetFromCollection<T, RT>(Func<Collection<T>, IEnumerable<RT>> map) where T : new()
        {
            using (var db = CreateStorageEngine())
            {
                var collection = db.GetCollection<T>(typeof (T).Name);
                return map(collection)
                    .ToArray();
            }
        }

        public static RT GetFromCollection<T, RT>(Func<Collection<T>, RT> map) where T : class, new()
        {
            using (var db = CreateStorageEngine())
            {
                var collection = db.GetCollection<T>(typeof (T).Name);
                return map(collection);
            }
        }

        private static void InitDatabase()
        {
            try
            {
                if (File.Exists(DATABASE_PATH))
                {
                    using (var db = CreateStorageEngine())
                    {
                        var col = db.GetCollection<DBVersion>("db_version");
                        var version = col.FindById(0);
                        if (version == null || version.Version < 1)
                        {
                            var moonItemCollection = db.GetCollection<MoonItemModel>(typeof (MoonItemModel).Name);
                            moonItemCollection.Drop();

                            var charachterPositionCollection =
                                db.GetCollection<EveCharacterApiService.EveCharacterPositions>(
                                                                                               typeof (
                                                                                                   EveCharacterApiService.EveCharacterPositions).Name);
                            charachterPositionCollection.Drop();

                            db.GetCollection<MoonItemModel>(typeof (MoonItemModel).Name)
                                .EnsureIndex(x => x.Id);
                            col.Upsert(
                                       new DBVersion
                                       {
                                           Version = CURRENT_DB_VERSION
                                       });
                        }
                        if (version == null || version.Version < 2)
                        {
                            var charachterPositionCollection =
                                db.GetCollection<EveCharacterApiService.EveCharacterPositions>(
                                                                                               typeof (
                                                                                                   EveCharacterApiService.EveCharacterPositions).Name);
                            charachterPositionCollection.Drop();

                        
                        }
                        if (version == null || version.Version < 3)
                        {
                            var charachterPositionCollection =
                              db.GetCollection<EveCharacterApiService.EveCharacterPositions>(
                                                                                             typeof(
                                                                                                 EveCharacterApiService.EveCharacterPositions).Name);
                            charachterPositionCollection.Drop();

                            var newCollection = db.GetCollection<EveCharacterPositions>(
                                                                                             typeof(
                                                                                                 EveCharacterPositions).Name);
                            newCollection.EnsureIndex(x => x.CharacterId);

                            var whConnection = db.GetCollection<WormholeConnection>(typeof(WormholeConnection).Name);
                            whConnection.EnsureIndex(x => x.FirstSystem);
                            whConnection.EnsureIndex(x => x.SecondSystem);
                            col.Upsert(
                                      new DBVersion
                                      {
                                          Version = CURRENT_DB_VERSION
                                      });
                        }
                    }
                    return;
                }

                using (var db = CreateStorageEngine())
                {

                    var newCollection = db.GetCollection<EveCharacterPositions>(
                                                                                     typeof(
                                                                                         EveCharacterPositions).Name);
                    newCollection.EnsureIndex(x => x.CharacterId);

                    db.GetCollection<ScannedSignature>(typeof (ScannedSignature).Name)
                        .EnsureIndex(x => x.System);

                    db.GetCollection<MoonItemModel>(typeof (MoonItemModel).Name)
                        .EnsureIndex(x => x.Id);

                    var whConnection = db.GetCollection<WormholeConnection>(typeof (WormholeConnection).Name);
                    whConnection.EnsureIndex(x => x.FirstSystem);
                    whConnection.EnsureIndex(x => x.SecondSystem);

                    db.GetCollection<DBVersion>("dbVersion")
                        .Upsert(
                                new DBVersion
                                {
                                    Version = CURRENT_DB_VERSION
                                });
                }
            }
            catch (Exception e)
            {
                LOG.Error(e);
                throw;
            }
        }

        private static void AppDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            LOG.Error("app domain unhandled", unhandledExceptionEventArgs.ExceptionObject as Exception);
        }

        private static string GetAndEnsurePathExists(string path)
        {
            var info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }
            return info.FullName;
        }

        public void OnToggleMinimize(string name)
        {
            if (Current.Windows.Cast<Window>()
                .All(x => x.WindowState == WindowState.Minimized))
            {
                foreach (var curEntry in _nonMinimizedStatus.Where(curEntry => curEntry.Key.IsLoaded))
                {
                    curEntry.Key.WindowState = curEntry.Value;
                }
                return;
            }

            _nonMinimizedStatus = Current.Windows.Cast<Window>()
                .ToDictionary(x => x, x => x.WindowState);
            foreach (Window window in Current.Windows)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        //TODO wenn geladen event benutzen -> geht nicht mit mainwindow stuff (onstartup)
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            _localChatAnalyser = DIContainer.GetInstance<LocalChatAnalyser>();

            SetupErrorHandler();
            SetupMainWindow(); //TODO der kram sollte woanders rein und das hier onstartup passieren
            SetupKosWarner();
            SetupStandings();
            Settings.Default.PropertyChanged += SettingsOnPropertyChanged;
            SetupShortcuts();
            //todo cancellation fuer analyser
            new Thread(_localChatAnalyser.Run)
            {
                IsBackground = true
            }.Start();
            //TODO im background thread starten?
            try
            {
                WebServer.Instance.Start();
            }
            catch (Exception ex)
            {
                var messageBoxText = string.Format("Error during start of web server: {0}", ex.Message);
                LOG.Warn(messageBoxText, ex);
                MessageBox.Show(messageBoxText, "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            ((MainWindow) MainWindow).UpdateClipboardContent += _localChatAnalyser.OnUpdateOfClipboard;
            CheckNews();
            CheckMessageOfTheDay();
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //TODO extract in extra class/binder between settings/shortcutregistry/editorwidget
            if (propertyChangedEventArgs.PropertyName == NotifyUtils.GetPropertyName((Settings s) => s.ShortcutToggleMinimizeAll) || propertyChangedEventArgs.PropertyName == NotifyUtils.GetPropertyName((Settings s) => s.ShortcutQuickAction))
            {
                Settings.RegisterGlobalShortcut();
            }
        }

        private static void SetupShortcuts()
        {
            Settings.RegisterGlobalShortcut();
        }

        private void CheckNews()
        {
            if (Settings.Default.NewsVersionToHide < News.CURRENT_NEWS_VERSION)
            {
                var _newsWindow = new News();

                _newsWindow.ShowDialog();
            }
        }

        private void CheckMessageOfTheDay()
        {
            Task.Factory.StartNew(
                                  () =>
                                  {
                                      var motd = MotdService.MessageOfTheDay;
                                      if (motd.MessageNumber > Settings.Default.MessageOfTheDayNumber)
                                      {
                                          Current.Dispatcher.Invoke(
                                                                    new Action(
                                                                        () =>
                                                                        {
                                                                            var motdDialog = new MessageOfTheDayDialog
                                                                                             {
                                                                                                 MessageOfTheDay = motd
                                                                                             };
                                                                            motdDialog.ActivateSoftwareRendering();
                                                                            motdDialog.ShowDialog();
                                                                        }));
                                      }
                                  },
                                  TaskCreationOptions.LongRunning);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                WebServer.Instance.Stop();
            }
            catch (Exception)
            {
            }

            base.OnExit(e);
        }

        private void SetupStandings()
        {
            try
            {
                Settings.Default.EnsureUpdatedStandings();
            }
            catch (Exception e)
            {
                LOG.Warn("Could not update standings", e);
                MessageBox.Show(string.Format("Could not update standings: {0}", e.Message));
            }
        }

        private void SetupErrorHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += TopLevelExceptionHandler;
        }

        private static void ShowError(string message)
        {
            var parent = Current != null ? Current.MainWindow : null;
            if (parent == null)
            {
                MessageBox.Show(string.Format("Error: {0}", message), "ERROR");
            }
            else
            {
                MessageBox.Show(parent, string.Format("Error: {0}", message), "ERROR");
            }
        }

        private static void SetupDefaultConfiguration()
        {
            ServicePointManager.DefaultConnectionLimit = 20;
        }

        private void SetupKosWarner()
        {
            var kosWarner = new KosWarner();
            _localChatAnalyser.UpdateCharacters += kosWarner.UpdateOfLocal;
        }

        private void SetupMainWindow()
        {
            var mainWindow = (MainWindow) MainWindow;

            CreateLocalChatAnalyserBindings(mainWindow);
            mainWindow.Clear += ClearLocalChat;
        }

        private void ClearLocalChat(object sender, object args)
        {
            _localChatAnalyser.Clear();
        }

        private void CreateLocalChatAnalyserBindings(MainWindow mainWindow)
        {
            mainWindow.Statistics = _localChatAnalyser.Statistics;
            _localChatAnalyser.UpdateCharacters += mainWindow.UpdateLocal;
            _localChatAnalyser.UpdateCharacters += characters => mainWindow.SetTitleToSuccess();
            _localChatAnalyser.UpdateCharacters += KillboardAnalysisService.OnLocalChangedTo;
            _localChatAnalyser.Error += mainWindow.SetTitleToError;
        }

        private static void TopLevelExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var exceptionObject = (Exception) e.ExceptionObject;

            LOG.Error("Toplevel Exception", exceptionObject);

            var errorMessage = exceptionObject.Message;
            ShowError(errorMessage);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var comException = e.Exception as COMException;
            if (comException != null && comException.ErrorCode == -2147221040)
            {
                e.Handled = true;
                return;
            }
            LOG.Error("kaputt", e.Exception);
            e.Handled = true; //TODO nur fuers dbeugging
        }

        public class DBVersion
        {
            public int Id
            {
                get { return 0; }
                set { }
            }

            public int Version { get; set; }
        }

        private static readonly string BASE_PATH = Environment.GetEnvironmentVariable("APPDATA") + "\\EVE Pirate's Little Helper";
        private static readonly string DATABASE_PATH = BASE_PATH + "\\localstore.db";
    }
}
