using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EveLocalChatAnalyser.Model;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Ui.Map.Statistics;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Ui.Wormholes;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using EveLocalChatAnalyser.Utilities.RouteFinding;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.EdgeRouting;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.PCL.Logic.Models;
using log4net;
using QuickGraph;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class MapGraph : BidirectionalGraph<SolarSystemViewModel, SolarSystemConnection>
    {
    }

    public class LogicCoreExample :
        GXLogicCore<SolarSystemViewModel, SolarSystemConnection, BidirectionalGraph<SolarSystemViewModel, SolarSystemConnection>>
    {
    }

    //TODO den ganzen kack aufraeumen, insbes. map rausziehen in eigenes control

    //TODO race condition bei systemwechsel, wenn daten noch nicht geladen wurden und so

    /// <summary>
    ///     Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : Window
    {
        private const int CURRENT_LAYOUT_VERSION = 7;
        private const int HIGH_QUALITY_LAYOUT_ITERATIONS = 2500;
        private const int DEFAULT_MAX_ITERATIONS = 5000;
        private static readonly ILog LOG = LogManager.GetLogger("Map");
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static readonly IPositionTracker POSITION_TRACKER = DIContainer.GetInstance<IPositionTracker>();
        private static readonly IWormholeConnectionTracker WORMHOLE_TRACKER = DIContainer.GetInstance<IWormholeConnectionTracker>();
        public static RoutedCommand UpdateTheraCommand = new RoutedCommand();
        private readonly LocalChatAnalyser _analyser = DIContainer.GetInstance<LocalChatAnalyser>();
        private readonly BasicSystemInfoLoadingService _basicSystemInfoLoadingService = new BasicSystemInfoLoadingService();
        private readonly MapStatistics _mapStatistics;
        private readonly IWormholeConnectionTracker _wormholeConnectionTracker = DIContainer.GetInstance<IWormholeConnectionTracker>();
        private KKLayoutParameters _defaultLayoutAlgorithmParams;
        private string _destinationSystemName;
        private volatile int _lastSelectedRange;
        private volatile int _lastSelectedSecurityPenalty;
        private SolarSystemViewModel _lastSelectedSolarSystem;
        private MapGraph _map;
        private readonly IMapGateCamps _mapGatecamps;
        private volatile int _rangeSelectUpdates;
        private volatile int _securityPenaltyUpdates;
        private string _selectedSystem;
        private LayoutContent _theSelectedSystemNotesAnchorable;

        //TODO den mainwindow crap weglassen, ist atm nru da, damit der windowinstancemanager benutzt werden kann
        public Map(MainWindow parameterIsOnlyThereForWindowInstanceManagerButOtherwiseUselessTodoRefactor)
        {
            InitializeComponent();
            this.SanitizeWindowSizeAndPosition();

            CboSystemNames.ItemsSource = UniverseDataDB.AllSystemNames;
            CboDestinationSystemNames.ItemsSource = UniverseDataDB.AllSystemNames;

            if (POSITION_TRACKER.CurrentSystemOfActiveCharacter == null)
            {
                ToggleFollowCharacter.IsChecked = false;
            }

            ToggleFollowCharacter.Checked += ToggleFollowCharacter_OnChecked;
            ToggleFollowCharacter.Unchecked += ToggleFollowCharacter_OnChecked;

            _theSelectedSystemNotesAnchorable = SelectedSystemNotesAnchorable;
            DScanLocatorCtrl.BtnPosMapper.Visibility = Visibility.Collapsed; //TODO schoener loesen

            DockManager.LayoutUpdated += DockManagerOnLayoutUpdated;
            RestoreLayout();

            _mapStatistics = new MapStatistics();
            _mapGatecamps = DIContainer.GetInstance<IMapGateCamps>();

            InitMapArea();
            InitCurrentLocal();
            //because value updates are triggered a lot of times, although slide is only moved one tick,
            //we store this from settings and make sure to only trigger updates on actual changes :/
            _lastSelectedRange = Properties.Settings.Default.MapRangeAroundRoute;
            _lastSelectedSecurityPenalty = Properties.Settings.Default.RouteFinderSecurityPenalty;

            Properties.Settings.Default.PropertyChanged += SettingsOnPropertyChanged;
            POSITION_TRACKER.ActiveCharacterSystemChanged += SystemChanged;
            WORMHOLE_TRACKER.WormholeConnectionCreated += WormholeTrackerOnWormholeConnectionCreated;
            WORMHOLE_TRACKER.WormholeConnectionUpdate += WormholeTrackerOnWormholeConnectionUpdate;

            Loaded += OnLoaded;
        }

        public static IRouteFinder RouteFinder { get; } = DIContainer.GetInstance<IRouteFinder>();

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Properties.Settings.Default.MapWindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
            Loaded -= OnLoaded;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DockManager != null)
            {
                DockManager.LayoutUpdated -= DockManagerOnLayoutUpdated;
                SaveLayout();
                Properties.Settings.Default.LastSavedDockVersion = CURRENT_LAYOUT_VERSION;
            }

            if (ShipAggregator != null)
            {
                ShipAggregator.Dispose();
            }

// ReSharper disable DelegateSubtraction
            _analyser.UpdateCharacters -= UpdateCharacters;
// ReSharper restore DelegateSubtraction

            Properties.Settings.Default.PropertyChanged += SettingsOnPropertyChanged;

            POSITION_TRACKER.ActiveCharacterSystemChanged -= SystemChanged;

            WORMHOLE_TRACKER.WormholeConnectionCreated -= WormholeTrackerOnWormholeConnectionCreated;
            WORMHOLE_TRACKER.WormholeConnectionUpdate -= WormholeTrackerOnWormholeConnectionUpdate;

            Properties.Settings.Default.MapSize = new Size((int) Width, (int) Height);
            Properties.Settings.Default.MapPosition = new Point((int) Left, (int) Top);
            Properties.Settings.Default.MapWindowState = WindowState;

            _mapStatistics.Dispose();
            _mapGatecamps.Dispose();

            base.OnClosing(e);
        }

        private static void AddRouteEdges(StaticSolarSystemInfo[] route, ICollection<SolarSystemConnection> edges)
        {
            if (route.Any())
            {
                ISet<int> idsOnRoute = new HashSet<int>(route.Select(x => x.Id));
                foreach (var curEdge in edges.Where(x => idsOnRoute.Contains(x.Source.ID) && idsOnRoute.Contains(x.Target.ID)))
                {
                    curEdge.IsPartOfRoute = true;
                }
            }
        }

        private async Task<StaticSolarSystemInfo[]> AddRouteSystems(SolarSystemViewModel currentSolarSystem,
                                                                    List<SolarSystemViewModel> allSystems)
        {
            var route = new StaticSolarSystemInfo[0];
            if (_destinationSystemName != null && currentSolarSystem.Name != _destinationSystemName)
            {
                route = RouteFinder.GetRouteBetween(currentSolarSystem.Name, _destinationSystemName);
                var systemsOnRoute = await TaskEx.WhenAll(
                                                          route.Skip(1)
                                                              .Select(
                                                                      x =>
                                                                      _basicSystemInfoLoadingService.GetSolarSystemViewModelBySystemName(
                                                                                                                                         x
                                                                                                                                             .Name)));
                allSystems.AddRange(systemsOnRoute);
                if (Properties.Settings.Default.MapRangeAroundRoute > 0)
                {
                    allSystems.AddRange(
                                        await
                                        _basicSystemInfoLoadingService.GetSurroundingSystemsFor(
                                                                                                Properties.Settings.Default
                                                                                                    .MapRangeAroundRoute,
                                                                                                systemsOnRoute));
                }

                var theraModel = systemsOnRoute.FirstOrDefault(x => x.Name == "Thera");
                if (theraModel != null)
                {
                    allSystems.AddRange(await _basicSystemInfoLoadingService.GetSurroundingSystemsFor(1, theraModel));
                }
            }
            return route;
        }

        private async Task<List<SolarSystemViewModel>> AddSurroundingSystems(SolarSystemViewModel currentSolarSystem)
        {
            var depthToLoad = currentSolarSystem.IsHighSec ? 3 : 5;
            var allSystems = (await _basicSystemInfoLoadingService.GetSurroundingSystemsFor(depthToLoad, currentSolarSystem)).ToList();
            allSystems.Add(currentSolarSystem);
            return allSystems;
        }

        private void AddWormholeConnectionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = (Control) sender;
            var system = ctrl.DataContext as SolarSystemViewModel;
            if (system == null)
            {
                return;
            }

            var dialog = new AddWormholeConnectionDialog
                         {
                             Owner = this
                         };
            if (!dialog.ShowDialog()
                     .GetValueOrDefault())
            {
                return;
            }

            if (dialog.SelectedSystem == null)
            {
                return;
            }

            if (system.Name == dialog.SelectedSystem)
            {
                MessageBox.Show(this, "Cannot create a wormhole connection to the same system");
                return;
            }

            if (UniverseDataDB.AreSystemsConnected(system.Name, dialog.SelectedSystem))
            {
                MessageBox.Show(this, "Cannot create a wormhole connection between already connected systems");
                return;
            }

            var newConnection = new WormholeConnection(system.Name, dialog.SelectedSystem);
            _wormholeConnectionTracker.InsertWormholeConnection(newConnection);
        }

        private void BtnClearDestination_OnClick(object sender, RoutedEventArgs e)
        {
            CboDestinationSystemNames.SelectedItem = null;
            SetDestinationSystemName(null);
        }

        private void CboDestinationSystemNames_OnDropDownClosed(object sender, EventArgs e)
        {
            SetDestinationSystemName((string) CboDestinationSystemNames.SelectedItem);
        }

        private void CboSystemNames_OnDropDownClosed(object sender, EventArgs eventArgs)
        {
            ToggleFollowCharacter.IsChecked = false;

            var system = CboSystemNames.Text;
            //TODO should be indexed
            if (!UniverseDataDB.AllSystemNames.Contains(system))
            {
                return;
            }

            if (_selectedSystem == system)
            {
                return;
            }

            _selectedSystem = system;
            InitMap()
                .ConfigureAwait(false);
        }

        private void CommandUpdateThera_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandUpdateThera_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DIContainer.GetInstance<IEveScoutService>()
                .ClearCache();

            InitMap()
                .ConfigureAwait(false);
        }

        private LogicCoreExample CreateLogicCore(MapGraph map)
        {
            //TODO zwischen vorhandenen border systemen edges einzeichen, falls vorhanden
            var logicCore = new LogicCoreExample
                            {
                                Graph = map
                            };

            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            _defaultLayoutAlgorithmParams =
                (KKLayoutParameters) logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
            logicCore.DefaultLayoutAlgorithmParams = _defaultLayoutAlgorithmParams;

            ((KKLayoutParameters) logicCore.DefaultLayoutAlgorithmParams).MaxIterations = DEFAULT_MAX_ITERATIONS;
            ((KKLayoutParameters) logicCore.DefaultLayoutAlgorithmParams).ExchangeVertices = true;

            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;

            logicCore.DefaultOverlapRemovalAlgorithmParams =
                logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters) logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 40;
            ((OverlapRemovalParameters) logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 40;

            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            logicCore.DefaultEdgeRoutingAlgorithmParams = new SimpleERParameters();

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            logicCore.AsyncAlgorithmCompute = true;
            return logicCore;
        }

        private void DScanLocatorAnchorable_OnIsActiveChanged(object sender, EventArgs e)
        {
            var content = (LayoutContent) sender;
            if (content.IsActive)
            {
                DScanLocatorCtrl.Activate();
            }
            else
            {
                DScanLocatorCtrl.Deactivate();
            }
        }

        private void DeleteWormholeConnectionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = (Control) sender;
            var connection = ctrl.DataContext as SolarSystemConnection;
            if (connection == null)
            {
                return;
            }

            if (MessageBox.Show(
                                this,
                                string.Format(
                                              "Remove the wormhole connection from {0} to {1}?",
                                              connection.Source.Name,
                                              connection.Target.Name),
                                "Remove Connection",
                                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _wormholeConnectionTracker.CloseWormholeConnection(connection.Source.Name, connection.Target.Name);
                InitMap()
                    .ConfigureAwait(false);
                //TODO init map should fire on close event of shown connection
            }
        }

        private void DockManagerOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            foreach (var x in DockManager.FloatingWindows)
            {
                x.Topmost = true;
            }
        }

        private void EditWormholeConnectionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = (Control) sender;
            var connection = ctrl.DataContext as WormholeSolarSystemConnection;
            if (connection == null)
            {
                return;
            }

            var editor = new WormholeConnectionEditDialog(connection.WormholeConnection);
            editor.Owner = this;

            editor.Show();
        }

        private void InitCurrentLocal()
        {
            _analyser.UpdateCharacters += UpdateCharacters;
        }

        private async Task InitMap()
        {
            //TODO refactor, what a mess ...
            string system = null;
            Application.Current.Dispatcher.Invoke(
                                                  new Func<string>(
                                                      () =>
                                                      system =
                                                      ToggleFollowCharacter.IsChecked.GetValueOrDefault()
                                                          ? POSITION_TRACKER.CurrentSystemOfActiveCharacter
                                                          : _selectedSystem));
            if (system == null)
            {
                return;
            }

            try
            {
                ResetMap();

                var currentSolarSystem = await _basicSystemInfoLoadingService.GetSolarSystemViewModelBySystemName(system);
                currentSolarSystem.IsCurrentSystem = true;

                var allSystems = await AddSurroundingSystems(currentSolarSystem);

                   var route = await AddRouteSystems(currentSolarSystem, allSystems);
                allSystems = allSystems.Distinct(SolarSystemViewModel.SolarSystemComparer)
                    .ToList(); //TODO distinct nur fauler workaround fuer duplikate, fehlerquelle beheben

                var edges = UniverseDataDB.GetConnectionsBetweenSystems(allSystems);
                 AddRouteEdges(route, edges);

               //var dotstr = string.Join(
               //             ";",
               //             allSystems.Select(x => "a" + x.ID)
               //                 .Concat(edges.Select(x => $"a{x.Source.ID} -- a{x.Target.ID}")));

               // File.WriteAllText("test.dot", dotstr);
               // return;

                _mapStatistics.AddSystems(allSystems);

                _mapGatecamps.AddConnections(edges);
                _map.AddVertexRange(allSystems);
                _map.AddEdgeRange(edges);

                UpdateMap(allSystems, edges);
            }
            catch (Exception e)
            {
                //TODO whatever
                LOG.Error("Error in loading map", e);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(
                                                      new Action(
                                                          () =>
                                                          {
                                                              BtnClearDestination.IsEnabled = true;
                                                              BtnRouteSettings.IsEnabled = true;
                                                              CboSystemNames.IsEnabled = true;
                                                              CboDestinationSystemNames.IsEnabled = true;
                                                              ToggleFollowCharacter.IsEnabled = true;
                                                              MapBusyIndicator.IsBusy = false;
                                                          }));
            }
        }

        private void InitMapArea()
        {
            _map = new MapGraph();

            MapArea.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(1));
            var logicCore = CreateLogicCore(_map);

            MapArea.LogicCore = logicCore;

            MapArea.ShowAllEdgesArrows(false);
            MapArea.RelayoutFinished += MapAreaOnRelayoutFinished;
            MapArea.GenerateGraphFinished += MapAreaOnGenerateGraphFinished;
            MapArea.VertexSelected += MapAreaOnVertexSelected;

            MapArea.LayoutUpdated += MapAreaOnLayoutUpdated;

            InitMap()
                .ConfigureAwait(false);
        }

        private void MapAreaOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            //Application.Current.Dispatcher.Invoke(
            //                                      new Action(
            //                                          () =>
            //                                          {
            //                                              var currentSystemVertexControl =
            //                                                  MapArea.VertexList.First(x => x.Key.IsCurrentSystem)
            //                                                      .Value;
            //                                              ZoomCtrl.CenterOnVertexControl(currentSystemVertexControl);
            //                                          }));
        }

        private void MapAreaOnVertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.RightButton == MouseButtonState.Pressed)
            {
                args.VertexControl.ContextMenu.DataContext = args.VertexControl.DataContext;
                args.VertexControl.ContextMenu.IsOpen = true;
            }
        }

        private async Task LoadSystemKills(string name, CancellationToken lastKillRequestCancellationToken)
        {
            var kills = await new SingleSolarSystemKillsService().GetKillsForSystem(name);
            if (lastKillRequestCancellationToken.IsCancellationRequested)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(
                                                  new Action(
                                                      () =>
                                                      {
                                                          KillsTable.Kills = kills.SelectMany(x => x.RecentKills)
                                                              .Select(x => new RecentKillModel(x))
                                                              .ToList();

                                                          LastKillsBusyIndicator.IsBusy = false;

                                                          var system = _map.Vertices.FirstOrDefault(x => x.Name == name);
                                                          if (system != null)
                                                          {
                                                              system.Killboard = kills.First();
                                                              _mapStatistics.Update();
                                                          }
                                                      }));
        }

        private async Task LoadSystems(SolarSystemViewModel system)
        {
            //TODO muesste eigentlich alles mit cancellation versehen werden
            var existingSystemIds = _map.Vertices.Select(x => x.ID);
            var newSystemModels =
                (await new BasicSystemInfoLoadingService().GetSurroundingSystemsFor(3, system)).Where(
                                                                                                      model =>
                                                                                                      !existingSystemIds.Contains(model.ID))
                    .Distinct()
                    .ToList();
            //TODO distinct sollte nicht notwendig sein

            if (!newSystemModels.Any())
            {
                Application.Current.Dispatcher.Invoke(
                                                      new Action(
                                                          () =>
                                                          {
                                                              MapBusyIndicator.IsBusy = false;
                                                              var systemControl = MapArea.VertexList[system];
                                                              ZoomCtrl.CenterOnVertexControl(systemControl);
                                                          }));
                return;
            }

            var edges = UniverseDataDB.GetConnectionsBetweenSystemsWithout(
                                                                           _map.Vertices.Concat(newSystemModels)
                                                                               .ToList(),
                                                                           _map.Vertices);
            try
            {
                _mapStatistics.AddSystems(newSystemModels);
                _mapGatecamps.AddConnections(edges);
                _map.AddVertexRange(newSystemModels);
                _map.AddEdgeRange(edges);
            }
            catch (Exception e)
            {
                LOG.Error("Could not load map data", e);
            }

            Application.Current.Dispatcher.Invoke(
                                                  new Action(
                                                      () =>
                                                      {
                                                          try
                                                          {
                                                              var idToControl = MapArea.VertexList.ToDictionary(x => x.Key.ID, x => x.Value);
                                                              foreach (var vertex in newSystemModels)
                                                              {
                                                                  var vertexControl = new VertexControl(vertex);
                                                                  MapArea.AddVertex(vertex, vertexControl);
                                                                  idToControl[vertex.ID] = vertexControl;
                                                              }

                                                              foreach (var edge in edges)
                                                              {
                                                                  var edgeControl = new EdgeControl(
                                                                      idToControl[edge.Source.ID],
                                                                      idToControl[edge.Target.ID],
                                                                      edge,
                                                                      false,
                                                                      false);
                                                                  MapArea.AddEdge(edge, edgeControl);
                                                              }
                                                              if (MapArea.VertexList.Count() == 1)
                                                              {
                                                                  var theOnlyVertex = MapArea.VertexList.First()
                                                                      .Value;
                                                                  theOnlyVertex.SetPosition(new System.Windows.Point(0, 0));
                                                                  ZoomCtrl.CenterOnVertexControl(theOnlyVertex);
                                                                  MapBusyIndicator.IsBusy = false;
                                                              }
                                                              else
                                                              {
                                                                  RelayoutGraph();
                                                                  MapBusyIndicator.IsBusy = false;
                                                              }
                                                          }
                                                          catch (Exception e)
                                                          {
                                                              LOG.Error("Could not load map extension", e);
                                                          }
                                                      }));
        }

        private void RelayoutGraph()
        {
            if (_defaultLayoutAlgorithmParams.MaxIterations < HIGH_QUALITY_LAYOUT_ITERATIONS)
            {
                _defaultLayoutAlgorithmParams.MaxIterations = MapArea.VertexList.Count > 25
                                                                  ? HIGH_QUALITY_LAYOUT_ITERATIONS
                                                                  : DEFAULT_MAX_ITERATIONS;
            }
            MapArea.RelayoutGraph();
        }

        private void MapAreaOnGenerateGraphFinished(object sender, EventArgs eventArgs)
        {
            ZoomCtrl.UpdateLayout();
            ZoomCtrl.ZoomToFill();

            MapBusyIndicator.IsBusy = false;
        }

        private void MapAreaOnRelayoutFinished(object sender, EventArgs eventArgs)
        {
            _defaultLayoutAlgorithmParams.MaxIterations = MapArea.VertexList.Count > 25
                                                              ? HIGH_QUALITY_LAYOUT_ITERATIONS
                                                              : DEFAULT_MAX_ITERATIONS;
            MapBusyIndicator.IsBusy = false;
            ZoomCtrl.UpdateLayout();
            //The delay is used because the layout change gets animated and during that
            //the vertex position changes.
            //I have not found a working event to connect to, to center, after the animation is finished, so
            //I use this dirty dirty hack -> enough time wasted in search for a good solution TODO look at this later
            TaskEx.Delay(1100)
                .ContinueWith(
                              a => Application.Current.Dispatcher.Invoke(
                                                                         new Action(
                                                                             () =>
                                                                             {
                                                                                 var currentSystemVertexControl =
                                                                                     MapArea.VertexList.First(x => x.Key.IsCurrentSystem)
                                                                                         .Value;
                                                                                 ZoomCtrl.CenterOnVertexControl(currentSystemVertexControl);
                                                                             })));
        }

        private void MapArea_OnVertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (_lastSelectedSolarSystem != null)
            {
                _cancellationTokenSource.Cancel();
                _lastSelectedSolarSystem.IsSelected = false;
            }

            if (args.VertexControl == null)
            {
                SystemNotesCtrl.DataContext = null;
                _cancellationTokenSource.Cancel();
                _lastSelectedSolarSystem = null;
                _theSelectedSystemNotesAnchorable.Title = "No system selected";
                LastLocalUpdateText.Text = "No local scan available";
                return;
            }

            _lastSelectedSolarSystem = (SolarSystemViewModel) args.VertexControl.DataContext;
            _lastSelectedSolarSystem.IsSelected = true;

            _theSelectedSystemNotesAnchorable.Title = "Notes for " + _lastSelectedSolarSystem.Name;
            SystemNotesCtrl.DataContext = _lastSelectedSolarSystem;

            LastKillsBusyIndicator.IsBusy = true;

            _cancellationTokenSource.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            TaskEx.Run(() => LoadSystemKills(_lastSelectedSolarSystem.Name, token), token);

            var lastScan =
                ((MainWindow) Application.Current.MainWindow).History.List.FirstOrDefault(x => x.System == _lastSelectedSolarSystem.Name);

            if (lastScan == null)
            {
                LastLocalUpdateText.Text = "No local scan available";
                LocalTable.Characters = new List<EveCharacterViewModel>();
            }
            else
            {
                LastLocalUpdateText.Text = $"Last Local Update: {lastScan.TimeStamp.GetTimeDifference()}";
                var statistics = new EveLocalStatistics();
                statistics.UpdateLocalStatistics(lastScan.Characters);
                LocalTable.Characters = lastScan.Characters.Select(x => new EveCharacterViewModel(x, statistics))
                    .ToList();
            }
        }

        private void MapArea_VertexDoubleClick(object sender, VertexSelectedEventArgs args)
        {
            if (args.VertexControl == null)
            {
                return;
            }

            MapBusyIndicator.IsBusy = true;
            var system = (SolarSystemViewModel) args.VertexControl.DataContext;
            TaskEx.Run(() => LoadSystems(system));
        }

        private void OpenSolarSystemKillboardMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = (Control) sender;
            var solarSystemViewModel = ctrl.DataContext as SolarSystemViewModel;
            if (solarSystemViewModel == null)
            {
                return;
            }

            var service = DIContainer.GetInstance<IExternalKillboardService>();
            service.OpenForSystem(solarSystemViewModel);
        }

        private void PosMapperAnchorable_OnIsActiveChanged(object sender, EventArgs e)
        {
            var content = (LayoutContent) sender;
            if (content.IsActive)
            {
                PosMapper.Activate();
            }
            else
            {
                PosMapper.Deactivate();
            }
        }

        private void RelayoutDefaultMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            RelayoutGraph();
        }

        private void RelayoutHQMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _defaultLayoutAlgorithmParams.MaxIterations = HIGH_QUALITY_LAYOUT_ITERATIONS;
            MapArea.RelayoutGraph();
        }

        private void ResetMap()
        {
            Application.Current.Dispatcher.Invoke(
                                                  new Action(
                                                      () =>
                                                      {
                                                          BtnClearDestination.IsEnabled = false;
                                                          BtnRouteSettings.IsEnabled = false;
                                                          CboDestinationSystemNames.IsEnabled = false;
                                                          CboSystemNames.IsEnabled = false;
                                                          ToggleFollowCharacter.IsEnabled = false;
                                                          MapBusyIndicator.IsBusy = true;
                                                          _map.Clear();
                                                          _mapStatistics.Clear();
                                                          _mapGatecamps.Clear();
                                                          MapArea.RemoveAllEdges();
                                                          MapArea.RemoveAllVertices();
                                                      }));
        }

        private void ResetMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            InitMap()
                .ConfigureAwait(false);
        }

        private void RestoreLayout()
        {
            if (CURRENT_LAYOUT_VERSION > Properties.Settings.Default.LastSavedDockVersion
                || string.IsNullOrWhiteSpace(Properties.Settings.Default.MapDockLayout))
            {
                return;
            }

            var serializer = new XmlLayoutSerializer(DockManager);
            serializer.LayoutSerializationCallback += SerializerOnLayoutSerializationCallback;
            using (var reader = new StringReader(Properties.Settings.Default.MapDockLayout))
            {
                serializer.Deserialize(reader);
            }
        }

        private void SaveLayout()
        {
            //DockManager.LayoutUpdateStrategy
            //before saving layout everything has to be shown, otherwise it can't be shown after layout restauration
            SelectedLocalAnchorable.Show();
            LastKillsInSystem.Show();
            DScanAggregator.Show();

            //SelectedLocalAnchorable.AddToLayout(DockManager, );

            var serializer = new XmlLayoutSerializer(DockManager);
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer);
                Properties.Settings.Default.MapDockLayout = writer.ToString();
            }
        }

        private void SerializerOnLayoutSerializationCallback(object sender, LayoutSerializationCallbackEventArgs e)
        {
            if (e.Model == null)
            {
                return;
            }

            switch (e.Model.ContentId)
            {
                //Do this for each LayoutAnchorable that has a menu item to show/hide it
                case "SelectedLocalAnchorable":
                    MenuSelectedLocalAnchorable.DataContext = e.Model;
                    break;
                case "ShipsOnDScan":
                    MenuDScanAggregator.DataContext = e.Model;
                    break;
                case "LastKillsInSystem":
                    MenuLastKillsInSystem.DataContext = e.Model;
                    break;
                case "SelectedSystemNotesAnchorable":
                    _theSelectedSystemNotesAnchorable = e.Model;
                    MenuSelectedSystemNotesAnchorable.DataContext = e.Model;
                    break;
                case "LastLocalAnchorable":
                    MenuLastLocal.DataContext = e.Model;
                    break;
                case "PosMapperAnchorable":
                    e.Model.IsActiveChanged += PosMapperAnchorable_OnIsActiveChanged;
                    MenuPosMapper.DataContext = e.Model;
                    break;
                case "DScanLocatorAnchorable":
                    e.Model.IsActiveChanged += DScanLocatorAnchorable_OnIsActiveChanged;
                    MenuDScanLocator.DataContext = e.Model;
                    break;
                case "MapSettingsAnchorable":
                    MenuMapSettings.DataContext = e.Model;
                    break;
                default:
                    break;
            }
        }

        private void SetDestinationSystemName(string name)
        {
            _destinationSystemName = name;
            InitMap()
                .ConfigureAwait(false);
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (_destinationSystemName != null
                && (propertyChangedEventArgs.PropertyName == NotifyUtils.GetPropertyName((Properties.Settings s) => s.RouteFinderRouteType)
                    || propertyChangedEventArgs.PropertyName
                    == NotifyUtils.GetPropertyName((Properties.Settings s) => s.RouteFinderIsIgnoringWormholes)))
            {
                InitMap()
                    .ConfigureAwait(false);
                return;
            }

            if (_destinationSystemName != null
                && propertyChangedEventArgs.PropertyName
                == NotifyUtils.GetPropertyName((Properties.Settings s) => s.RouteFinderSecurityPenalty))
            {
                //because value updates are triggered a lot of times, although slide is only moved one tick,
                //we store this from settings and make sure to only trigger updates on actual changes :/
                if (Properties.Settings.Default.RouteFinderSecurityPenalty == _lastSelectedSecurityPenalty)
                {
                    return;
                }
                _lastSelectedSecurityPenalty = Properties.Settings.Default.RouteFinderSecurityPenalty;
                ++_securityPenaltyUpdates;
                TaskEx.Delay(new TimeSpan(0, 0, 0, 2))
                    .ContinueWith(
                                  t =>
                                  {
                                      if (--_securityPenaltyUpdates == 0)
                                      {
                                          InitMap()
                                              .ConfigureAwait(false);
                                      }
                                  });
                return;
            }

            if (_destinationSystemName != null
                && propertyChangedEventArgs.PropertyName == NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapRangeAroundRoute))
            {
                //because value updates are triggered a lot of times, although slide is only moved one tick,
                //we store this from settings and make sure to only trigger updates on actual changes :/
                if (Properties.Settings.Default.MapRangeAroundRoute == _lastSelectedRange)
                {
                    return;
                }
                _lastSelectedRange = Properties.Settings.Default.MapRangeAroundRoute;
                ++_rangeSelectUpdates;
                TaskEx.Delay(new TimeSpan(0, 0, 0, 2))
                    .ContinueWith(
                                  t =>
                                  {
                                      var i = --_rangeSelectUpdates;
                                      if (i == 0)
                                      {
                                          InitMap()
                                              .ConfigureAwait(false);
                                      }
                                  });
            }
        }

        private void SystemChanged(string character, string newSystem)
        {
            if (!ToggleFollowCharacter.IsChecked.GetValueOrDefault())
            {
                return;
            }
            var oldSys = _map.Vertices.FirstOrDefault(x => x.IsCurrentSystem);
            if (oldSys != null)
            {
                oldSys.IsCurrentSystem = false;
            }

            var newSys = _map.Vertices.FirstOrDefault(x => x.Name == newSystem);
            if (newSys == null)
            {
                InitMap()
                    .ConfigureAwait(false);
                return;
            }

            Application.Current.Dispatcher.Invoke(
                                                  new Action(
                                                      () =>
                                                      {
                                                          newSys.IsCurrentSystem = true;
                                                          MapBusyIndicator.IsBusy = true;
                                                      }));

            LoadSystems(newSys)
                .ConfigureAwait(false);
        }

        private void ToggleFollowCharacter_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ToggleFollowCharacter.IsChecked.GetValueOrDefault())
            {
                if (Properties.Settings.Default.PositionTrackingCharacters == null
                    || Properties.Settings.Default.PositionTrackingCharacters.Count == 0)
                {
                    MessageBox.Show("Please select characters in to follow in the position tracking settings first.");
                    ToggleFollowCharacter.IsChecked = false;
                    return;
                }

                _selectedSystem = null;
                InitMap()
                    .ConfigureAwait(false);
            }
        }

        private void UpdateCharacters(List<IEveCharacter> characters)
        {
            var statistics = new EveLocalStatistics();
            statistics.UpdateLocalStatistics(characters);
            Application.Current.Dispatcher.BeginInvoke(
                                                       new Action(
                                                           () =>
                                                           {
                                                               LastLocalTable.Characters =
                                                                   characters.Select(x => new EveCharacterViewModel(x, statistics))
                                                                       .ToList();
                                                           }));
        }

        private void UpdateMap(List<SolarSystemViewModel> allSystems, ICollection<SolarSystemConnection> edges)
        {
            Application.Current.Dispatcher.Invoke(
                                                  new Action(
                                                      () =>
                                                      {
                                                          var idToControl = new Dictionary<int, VertexControl>();

                                                          foreach (var vertex in allSystems)
                                                          {
                                                              var vertexControl = new VertexControl(vertex);
                                                              MapArea.AddVertex(vertex, vertexControl);
                                                              idToControl[vertex.ID] = vertexControl;
                                                          }

                                                          foreach (var edge in edges)
                                                          {
                                                              MapArea.AddEdge(
                                                                              edge,
                                                                              new EdgeControl(
                                                                                  idToControl[edge.Source.ID],
                                                                                  idToControl[edge.Target.ID],
                                                                                  edge,
                                                                                  false,
                                                                                  false));
                                                          }

                                                          if (MapArea.VertexList.Count() == 1)
                                                          {
                                                              var firstVertexEntry = MapArea.VertexList.First()
                                                                  .Value;
                                                              firstVertexEntry.SetPosition(new System.Windows.Point(0, 0));
                                                              ZoomCtrl.CenterOnVertexControl(firstVertexEntry);
                                                              //TODO code duplication entfernen
                                                          }
                                                          else
                                                          {
                                                              MapArea.GenerateGraph(true);
                                                              MapArea.ShowAllEdgesArrows(false);
                                                          }
                                                      }));
        }

        private async void LoadRegion(SolarSystemViewModel system)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { MapBusyIndicator.IsBusy = true; }));
            var systems = await new BasicSystemInfoLoadingService().GetRegionalSystemsFor(system);
            var existingSystems = MapArea.VertexList.Keys;
            var newSystems = systems.Where(x => !existingSystems.Contains(x))
                .ToArray();

            if (!newSystems.Any())
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { MapBusyIndicator.IsBusy = false; }));
            }

            var newConnections = UniverseDataDB.GetConnectionsBetweenSystemsWithout(
                                                                                    newSystems.Concat(existingSystems)
                                                                                        .ToList(),
                                                                                    existingSystems);
            Application.Current.Dispatcher.Invoke(
                                                  new Action(
                                                      () =>
                                                      {
                                                          _mapStatistics.AddSystems(newSystems);
                                                          _mapGatecamps.AddConnections(newConnections);
                                                          _map.AddVertexRange(newSystems);
                                                          _map.AddEdgeRange(newConnections);

                                                          var idToControl = MapArea.VertexList.ToDictionary(x => x.Key.ID, x => x.Value);
                                                          foreach (var vertex in newSystems)
                                                          {
                                                              var vertexControl = new VertexControl(vertex);
                                                              MapArea.AddVertex(vertex, vertexControl);
                                                              idToControl[vertex.ID] = vertexControl;
                                                          }

                                                          foreach (var edge in newConnections)
                                                          {
                                                              MapArea.AddEdge(
                                                                              edge,
                                                                              new EdgeControl(
                                                                                  idToControl[edge.Source.ID],
                                                                                  idToControl[edge.Target.ID],
                                                                                  edge,
                                                                                  false,
                                                                                  false));
                                                              //TODO create edgeControl builder
                                                          }
                                                          RelayoutGraph();

                                                          MapBusyIndicator.IsBusy = false;
                                                      }));
        }

        private void WormholeTrackerOnWormholeConnectionCreated(WormholeConnection whConnection)
        {
            //TODO die reihenfolge der systeme sollte abhaengig vom entry sein
            var editor = new WormholeConnectionEditDialog(whConnection)
                         {
                             Owner = this
                         };

            editor.Show();
            InitMap()
                .ConfigureAwait(false);
        }

        private void WormholeTrackerOnWormholeConnectionUpdate(WormholeConnection whConnection)
        {
            var edge =
                _map.Edges.FirstOrDefault(
                                          x =>
                                          x is WormholeSolarSystemConnection
                                          && whConnection.Equals((((WormholeSolarSystemConnection) x).WormholeConnection)));
            if (edge == null)
            {
                return;
            }

            ((WormholeSolarSystemConnection) edge).WormholeConnection = whConnection;

            InitMap()
                .ConfigureAwait(false);
        }

        private void SetDestinationMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = (Control) sender;
            var solarSystemViewModel = ctrl.DataContext as SolarSystemViewModel;
            if (solarSystemViewModel == null)
            {
                return;
            }

            SetDestinationSystemName(solarSystemViewModel.Name);
        }

        private void LoadRegionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = (Control) sender;
            var solarSystemViewModel = ctrl.DataContext as SolarSystemViewModel;
            if (solarSystemViewModel == null)
            {
                return;
            }

            Task.Factory.StartNew(() => LoadRegion(solarSystemViewModel), TaskCreationOptions.LongRunning);
        }
    }

    public static class ExtensionMethods
    {
        private static readonly Action EmptyDelegate = delegate { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
}
