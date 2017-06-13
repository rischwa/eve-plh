#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Themes;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Ui.Settings;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using EveLocalChatAnalyser.Utilities.VoiceCommands;
using log4net;
using Microsoft.Win32;
using PLHLib;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

#endregion

namespace EveLocalChatAnalyser.Ui
{
    public partial class MainWindow : Window
    {
        private static readonly string VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private readonly CharacterSorter _characterSorter = new CharacterSorter();
        public Clear Clear;
        public UpdateClipboardContent UpdateClipboardContent;
        private string _currentSolarSystem;
        //TODO dirty hack with public properties, create real solution
        public readonly WindowInstanceManager<DScanRangeFinder> _dscanRangeFinder;
        public readonly WindowInstanceManager<ProbeScan> _probeScan;
        public readonly WindowInstanceManager<Map.Map> _map;
        private Info _info;
        private bool _isCollapsed;
        private SettingsPage _settings;
        private double _windowHeightBeforeCollapsing;
        private static readonly ILog LOG = LogManager.GetLogger("MainWindow");
        public MainWindow()
        {
            Closing += MainWindowClosing;
            StateChanged += OnStateChanged;
            History = new LocalChatHistory();
            Characters = new NotifyThroughDispatcherCollection<EveCharacterViewModel>();
            
            InitializeComponent();
            SetTitleToSuccess();

            var activeCharacterTracker = DIContainer.GetInstance<IActiveCharacterTracker>();
            activeCharacterTracker.ActiveCharacterChanged += ActiveCharacterTrackerOnActiveCharacterChanged;

            //initialize voice commands TODO should happen elsewhere
            DIContainer.GetInstance<IVoiceCommands>();
            
            var positionTracker = DIContainer.GetInstance<IPositionTracker>();
            positionTracker.ActiveCharacterSystemChanged += OnSystemChanged;

            var clipboardParser = DIContainer.GetInstance<ClipboardParser>();
            clipboardParser.Local += ClipboardParserOnLocal;
            clipboardParser.ProbeScan += ScanningStorage.OnProbeScan;

            _dscanRangeFinder = new WindowInstanceManager<DScanRangeFinder>(this);
            _probeScan = new WindowInstanceManager<ProbeScan>(this);
            _map = new WindowInstanceManager<Map.Map>(this);

            this.SanitizeWindowSizeAndPosition();
        }

        private void ActiveCharacterTrackerOnActiveCharacterChanged(string activeCharacter)
        {
            Title = "Switched to " + activeCharacter;
        }

        private void ClipboardParserOnLocal(IList<string> characterNames)
        {
            if (UpdateClipboardContent != null)
            {
                BusyIndicator.IsBusy = true;
                UpdateClipboardContent(characterNames);
            }
        }


        public LocalChatHistory History { get; set; }

        public NotifyThroughDispatcherCollection<EveCharacterViewModel> Characters { get; private set; }

        public EveLocalStatistics Statistics { get; set; }
        

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.WindowSize = new Size((int) Width, (int) Height);
            Properties.Settings.Default.WindowPosition = new Point((int) Left, (int) Top);
            Properties.Settings.Default.Save();

            _probeScan.Close();
            _dscanRangeFinder.Close();
            _map.Close();
        }

        //private void OnStateChanged(object sender, EventArgs e)
        //{
        //    if (WindowState == WindowState.Maximized)
        //    {
        //        WindowState = WindowState.Normal;
        //    }
        //}


        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (_settings != null)
            {
                _settings.Close();
            }
            if (_info != null)
            {
                _info.Close();
            }
        }

        private void OnSystemChanged(string character, string newsystem)
        {
            if (_currentSolarSystem == newsystem)
            {
                return;
            }
            _currentSolarSystem = newsystem;
            SetTitleToSystemChange();

            if (DockAjacentSystems.Visibility == Visibility.Collapsed)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    DockAjacentSystems.Visibility = Visibility.Visible;
                    SplitterAdjacentSystems.Visibility = Visibility.Visible;
                }));
            }

            Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        var analysis = await SolarSystemKillboardAnalysis.GetInfoForAdjacentSystemsOf(newsystem);
                        SetAdjacentSolarSystemKillboardAnalysisResult(analysis);
                    }
                    catch (Exception e)
                    {
                        LOG.Warn("Error during solar system analysis", e);
                        Application.Current.Dispatcher.Invoke( //TODO dispatcher zeug extrahieren in utility
                            new Action(() =>
                            {
                                if (Application.Current != null && Application.Current.MainWindow != null)
                                {
                                    ((MainWindow) Application.Current.MainWindow).SetTitleToError(
                                                                                                  e.Message);
                                }
                            }));
                    }
                });
        }

        private void SetAdjacentSolarSystemKillboardAnalysisResult(List<SolarSystemKills> analysis)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { DataAdjacentSystems.ItemsSource = analysis; }));
        }

        private void SetTitleToSystemChange()
        {
            var application = Application.Current;
            if (application == null)
            {
                return;
            }
            application.Dispatcher.Invoke(new Action(() =>
            {
                Title = "Please update local for new system " + _currentSolarSystem;
                //lblTitle.Foreground = new SolidColorBrush(Colors.BlueViolet);
            }));
        }

        public void SetTitleToError(string message)
        {
            var application = Application.Current;
            if (application == null)
            {
                return;
            }
            application.Dispatcher.Invoke(new Action(() =>
            {
                BusyIndicator.IsBusy = false;//TODO schlechter hack, falls fehler bei local update, muss busy weggehen
                //  if (lblTitle != null)
                {
                    Title = "ERROR: " + message;
                    //     lblTitle.Foreground = new SolidColorBrush(Colors.Red);
                }
            }));
        }

        public void SetTitleToSuccess()
        {
            var application = Application.Current;
            if (application == null)
            {
                return;
            }
            application.Dispatcher.Invoke(new Action(() =>
            {
                //   if (lblTitle != null)
                {
                    var position = _currentSolarSystem != null ? string.Format(" [{0}]", _currentSolarSystem) : "";

                    Title = string.Format("EVE Pirate's Little Helper ({0}){1}",
                                          VERSION, position);
                    //     lblTitle.Foreground = new SolidColorBrush(Colors.Black);
                }
            }));
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var curChar = ((DataGridRow) sender).Item as EveCharacterViewModel;
            if (curChar == null)
            {
                return;
            }

            var service = DIContainer.GetInstance<IExternalKillboardService>();
            service.OpenForCharacter(curChar.EveCharacter);
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MyMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.Show();
            }
            else
            {
                _settings = new SettingsPage();
                _settings.Closed += SettingsClosed;
                try
                {
                    _settings.ShowDialog();
                }
                catch (Exception ex)
                {
                    LOG.Error("Error in settings dialog", ex);
                    MessageBox.Show(
                        string.Format(
                            "Error in settings dialog. This should not happen and i am very sorry about it.\nPlease contact me (jonas.jacobi@web.de) with the following error description:\n{0}\n\n{1}",
                            ex.Message, ex.StackTrace), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                DataMain.Items.Refresh();
            }
        }

        private void SettingsClosed(object sender, EventArgs e)
        {
            _settings = null;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveWindowPositionAndSize();
        }

        private void SaveWindowPositionAndSize()
        {
            if (WindowState != WindowState.Normal)
            {
                return;
            }
            Properties.Settings.Default.WindowSize = new Size((int) Width, (int) Height);
            Properties.Settings.Default.WindowPosition = new Point((int) Left, (int) Top);
            Properties.Settings.Default.Save();
        }

        private void ClearClicked(object sender, RoutedEventArgs e)
        {
            Clear.Invoke(sender, e);
        }

        private void InfoClick(object sender, RoutedEventArgs e)
        {
            if (_info != null)
            {
                _info.Show();
                return;
            }

            _info = new Info();
            _info.Closed += InfoClosed;
            _info.Show();
            //_info.ActivateSoftwareRendering();
        }

        private void InfoClosed(object sender, EventArgs e)
        {
            _info = null;
        }

        private void lstMain_MouseMove(object sender, MouseEventArgs e)
        {
            var eveChar = GetEveCharacter(e);
            if (eveChar == null)
            {
                ResetHighlighting();
                return;
            }

            SetHighlighting(eveChar);
        }

        private static EveCharacterViewModel GetEveCharacter(RoutedEventArgs e)
        {
            var element = (FrameworkElement) e.OriginalSource;
            return element.DataContext as EveCharacterViewModel;
        }

        private void SetHighlighting(EveCharacterViewModel eveChar)
        {
            eveChar.IsHighlighted = true;
            for (var i = 0; i < DataMain.Items.Count; ++i)
            {
                var curChar = (EveCharacterViewModel) DataMain.Items[i];
                SetHighlighting(eveChar, curChar);
            }
        }

        private static void SetHighlighting(EveCharacterViewModel eveChar, EveCharacterViewModel curChar)
        {
            curChar.IsHighlighted = AreConnected(eveChar.EveCharacter, curChar.EveCharacter);
        }


        private static bool AreConnected(IEveCharacter eveChar, IEveCharacter curChar)
        {
            if (Properties.Settings.Default.IsShowingCoalitionsColumn &&
                eveChar.Coalitions.Any(c => curChar.Coalitions.Contains(c)))
            {
                return true;
            }

            return (!string.IsNullOrEmpty(eveChar.Alliance)
                        ? eveChar.Alliance == curChar.Alliance
                        : !string.IsNullOrEmpty(eveChar.Corporation) && curChar.Corporation == eveChar.Corporation) ||
                   IsAssociatedTo(eveChar, curChar) || IsAssociatedTo(curChar, eveChar);
        }

        private static bool IsAssociatedTo(IEveCharacter eveChar, IEveCharacter curChar)
        {
            return eveChar.KillboardInformation != null && (
                                                               eveChar.KillboardInformation.AssociatedAlliances.Contains
                                                                   (curChar.Alliance) ||
                                                               eveChar.KillboardInformation.AssociatedCorporations
                                                                      .Contains(curChar.Corporation));
        }

        private void ResetHighlighting()
        {
            foreach (var curChar in Characters)
            {
                curChar.IsHighlighted = false;
            }
        }

        private static bool TryGetEveCharacterFromMenuItem(object sender, out IEveCharacter eveChar)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                eveChar = null;
                return false;
            }
            var vm = menuItem.CommandParameter as EveCharacterViewModel;
            if (vm != null)
            {
                eveChar = vm.EveCharacter;
                return true;
            }
            eveChar = null;
            return false;
        }

        private void ShowInfo_Click(object sender, RoutedEventArgs e)
        {
            IEveCharacter eveChar;
            if (!TryGetEveCharacterFromMenuItem(sender, out eveChar) || eveChar.KillboardInformation != null)
            {
                return;
            }
            KillboardAnalysisService.AddFirst(eveChar);
        }

        public void UpdateLocal(IList<IEveCharacter> characters)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                History.AddEntry(characters, _currentSolarSystem);
                MoveToNewestInHistory();
                Characters.SetContent(
                                      _characterSorter.Sorted(characters
                                                                  .Select(
                                                                          character =>
                                                                          new EveCharacterViewModel
                                                                              (character,
                                                                               Statistics))));
                BusyIndicator.IsBusy = false;
            }));
        }

        private void MoveToNewestInHistory()
        {
            History.Reset(); //Reset() moves "in front" of the first history entry
            if (History.HasNext)
            {
                var next = History.Next;
            }
        }

        private void lstMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataMain.SelectedIndex = -1;
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            var entries = History.Previous;
            Characters.SetContent(
                _characterSorter.Sorted(
                    entries.Characters.Select(character => new EveCharacterViewModel(character, Statistics))));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var entries = History.Next;
            Characters.SetContent(
                _characterSorter.Sorted(
                    entries.Characters.Select(character => new EveCharacterViewModel(character, Statistics))));
        }

        private void DScanLocatorClick(object sender, RoutedEventArgs e)
        {
            _dscanRangeFinder.Show();
        }

        private void TitleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isCollapsed)
            {
                ExpandWindow();
            }
            else
            {
                CollapseWindow();
            }
        }

        private void ExpandWindow()
        {
            _isCollapsed = false;
            Height = _windowHeightBeforeCollapsing;
        }

        private void CollapseWindow()
        {
            _isCollapsed = true;
            _windowHeightBeforeCollapsing = Height;
            Height = 14;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isCollapsed && Height > _windowHeightBeforeCollapsing)
            {
                _isCollapsed = false;
            }
        }

        private void LstAdjacentSystems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                return;
            }
            var item = e.AddedItems[0] as SolarSystemKills;
            if (item == null)
            {
                return;
            }

            DataKills.ItemsSource = item.RecentKills.Select(x => new RecentKillModel(x)).ToList();
        }

        private void DataKills_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        private void ScrollKills_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer) sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void BtnProbeScanClicked(object sender, RoutedEventArgs e)
        {
            _probeScan.Show();
            
        }

        private void BtnMapClick(object sender, RoutedEventArgs e)
        {
            _map.Show();
        }




        private void ReloadKillboardInformation_Clicked(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            var contextMenu = (ContextMenu)menuItem.Parent;
            var item = (DataGrid)contextMenu.PlacementTarget;
            foreach (var curChar in item.SelectedCells.Reverse().Select(x => x.Item)
                .Cast<EveCharacterViewModel>())
            {
                curChar.EveCharacter.KillboardInformation = null;
                KillboardAnalysisService.AddFirst(curChar.EveCharacter);
            }
        }
        //TODO code duplication with localtablecontrol, remove from here
        private void SetCustomIcon_Clicked(object sender, RoutedEventArgs e)
        {
            EveCharacterViewModel viewModel;
            if (!TryGetFirstSelectedCharacter(sender, out viewModel))
            {
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter =
                                                         "Image Files (*.jpg,*.jpeg,*.png,*.gif)|*.jpg;*.jpeg;*.png;*.gif"
            };


            var result = dlg.ShowDialog();

            if (result != true)
            {
                return;
            }

            var filename = dlg.FileName;

            viewModel.CustomCharacterInfo.IconImage = filename;

        }

        private void RemoveCustomIcon_Clicked(object sender, RoutedEventArgs e)
        {
            EveCharacterViewModel viewModel;
            if (!TryGetFirstSelectedCharacter(sender, out viewModel))
            {
                return;
            }

            viewModel.CustomCharacterInfo.IconImage = null;
        }

        private static bool TryGetFirstSelectedCharacter(object sender, out EveCharacterViewModel viewModel)
        {
            var menuItem = (MenuItem)sender;

            var contextMenu = (ContextMenu)menuItem.Parent;
            var item = (DataGrid)contextMenu.PlacementTarget;
            if (!item.SelectedCells.Any())
            {
                viewModel = null;
                return false;
            }
            viewModel = (EveCharacterViewModel)item.SelectedCells[0].Item;
            return true;
        }




        //-------------------------------------------------------------------------------

        /// <summary>
        /// Handles the MouseLeftButtonDown event. This event handler is used here to facilitate
        /// dragging of the Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            // Check if the control have been double clicked.
            if (e.ClickCount == 2)
            {
                // If double clicked then maximize the window.
                TitleDoubleClick(sender, e);
            }
            else
            {
                // If not double clicked then just drag the window around.
                window.DragMove();
            }
        }

        /// <summary>
        /// Fires when the user clicks the Close button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        /// <summary>
        /// Fires when the user clicks the minimize button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Fires when the user clicks the maximize button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            // Check the current state of the window. If the window is currently maximized, return the
            // window to it's normal state when the maximize button is clicked, otherwise maximize the window.
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
            else
            {
                window.Focus();
                window.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Called when the window gets resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            // Update window's contraints like max height and width.
            UpdateWindowConstraints(window);

            // Get window sub parts
            Image icon = (Image)window.Template.FindName("IconApp", window);
            Grid windowRoot = (Grid)window.Template.FindName("WindowRoot", window);
            Border windowFrame = (Border)window.Template.FindName("WindowFrame", window);
            Grid windowLayout = (Grid)window.Template.FindName("WindowLayout", window);

            // Adjust the window icon size
            if (icon != null)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    icon.Height = 20;
                    icon.Width = 20;
                    icon.Margin = new Thickness(10, 5, 0, 0);
                }
                else
                {
                    icon.Height = 24;
                    icon.Width = 24;
                    icon.Margin = new Thickness(10, 3, 0, 0);
                }
            }
        }

        private void OnStateChanged(object sender, EventArgs eventArgs)
        {
            var window = (Window)sender;
            ((Image)window.Template.FindName("MaximizeImage", window)).Source = window.WindowState == WindowState.Maximized
                                       ? (BitmapImage)window.Resources["ResizeSmallImage"]
                                       : (BitmapImage)window.Resources["ResizeFullImage"];
        }

        /// <summary>
        /// Called when a window gets loaded.
        /// We initialize resizers and update constraints.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            //TODO on closing remove listeners
            window.StateChanged += OnStateChanged;

            this.AddResizeHook();

            // Update constraints.
            UpdateWindowConstraints(window);

            // Attach resizer
            WindowResizer wr = new WindowResizer(window);
            wr.addResizerRight((Rectangle)window.Template.FindName("rightSizeGrip", window));
            wr.addResizerLeft((Rectangle)window.Template.FindName("leftSizeGrip", window));
            wr.addResizerUp((Rectangle)window.Template.FindName("topSizeGrip", window));
            wr.addResizerDown((Rectangle)window.Template.FindName("bottomSizeGrip", window));
            wr.addResizerLeftUp((Rectangle)window.Template.FindName("topLeftSizeGrip", window));
            wr.addResizerRightUp((Rectangle)window.Template.FindName("topRightSizeGrip", window));
            wr.addResizerLeftDown((Rectangle)window.Template.FindName("bottomLeftSizeGrip", window));
            wr.addResizerRightDown((Rectangle)window.Template.FindName("bottomRightSizeGrip", window));
        }

        /// <summary>
        /// Called when the user drags the title bar when maximized.
        /// </summary>
        private void OnBorderMouseMove(object sender, MouseEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            if (window != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed && window.WindowState == WindowState.Maximized)
                {
                    System.Windows.Size maxSize = new System.Windows.Size(window.ActualWidth, window.ActualHeight);
                    System.Windows.Size resSize = window.RestoreBounds.Size;

                    double curX = e.GetPosition(window).X;
                    double curY = e.GetPosition(window).Y;

                    double newX = curX / maxSize.Width * resSize.Width;
                    double newY = curY;

                    window.WindowState = WindowState.Normal;

                    window.Left = curX - newX;
                    window.Top = curY - newY;
                    window.DragMove();
                }
            }
        }

        /// <summary>
        /// Updates the window constraints based on its state.
        /// For instance, the max width and height of the window is set to prevent overlapping over the taskbar.
        /// </summary>
        /// <param name="window">Window to set properties</param>
        private void UpdateWindowConstraints(Window window)
        {
            //if (window != null)
            //{
            //    // Make sure we don't bump the max width and height of the desktop when maximized
            //    GridLength borderWidth = (GridLength)window.FindResource("BorderWidth");
            //    if (borderWidth != null)
            //    {
            //        window.MaxHeight = SystemParameters.WorkArea.Height + borderWidth.Value * 2;
            //        window.MaxWidth = SystemParameters.WorkArea.Width + borderWidth.Value * 2;
            //    }
            //}
        }




       
    }

    //deferred DI loading
    //TODO verschieben

    public delegate void Clear(object sender, object args);

    //TODO kann entfern werden, der teil sollte ueber das zentrale ding gehen
    public delegate void UpdateClipboardContent(IList<string> clipboardContent);
}