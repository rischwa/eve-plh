using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using EveLocalChatAnalyser.Utilities.PosMapper;

namespace EveLocalChatAnalyser.Ui.PosMapper
{
    /// <summary>
    ///     Interaction logic for PosMapperControl.xaml
    /// </summary>
    public partial class PosMapperControl : UserControl, IDisposable
    {
        private static readonly IPositionTracker POSITION_TRACKER = DIContainer.GetInstance<IPositionTracker>();
        private PosMapper2 _posMapper;
        private int _towersScannedCount;

        public PosMapperControl()
        {
            InitializeComponent();

            InitPosMapper();
            Reset();

            POSITION_TRACKER.ActiveCharacterSystemChanged += SystemChanged;
            DataMoons.ItemsSource = ExperimentalInefficientItemLoading(POSITION_TRACKER.CurrentSystemOfActiveCharacter);
        }

        public void Dispose()
        {
            // ReSharper disable DelegateSubtraction
            POSITION_TRACKER.ActiveCharacterSystemChanged -= SystemChanged;
            // ReSharper restore DelegateSubtraction
            RemovePosMapperBindings();
        }

        private void PosMapperOnIsActiveChanged(bool isActive)
        {
            PosMapperBusyIndicator.IsBusy = !isActive;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Activate();
        }

        private void SystemChanged(string characterName, string newSystem)
        {
            _posMapper.Reset();
            ResetVisuals();
            DataMoons.ItemsSource = ExperimentalInefficientItemLoading(newSystem);
        }

        private void RemovePosMapperBindings()
        {
            if (_posMapper == null)
            {
                return;
            }
            _posMapper.ScanDone -= PosMapperOnScanDone;
            _posMapper.NextScanRequest -= PosMapperOnNextScanRequest;
            _posMapper.MoonsScanned -= PosMapperOnMoonsScanned;
            _posMapper.OfflineTowersFound -= PosMapperOnOfflineTowersFound;
            _posMapper.MoonClusterIsTooDense -= PosMapperOnSkippingMoonsBecauseTheyAreTooClose;
            _posMapper.NeedToDeselectOverviewSettings -= PosMapperOnNeedToDeselectOverviewSettings;
            _posMapper.IsActiveChanged -= PosMapperOnIsActiveChanged;
            _posMapper.Dispose();
            _posMapper = null;
        }

        private void PosMapperOnNeedToDeselectOverviewSettings()
        {
            MessageBox.Show(Application.Current.MainWindow, "Please deselect 'Use Active Overview Settings' first");
        }

        private void PosMapperOnMoonsScanned(IList<MoonItem> moonItems)
        {
            TaskEx.Run(
                       () =>
                       {
                           lock (this)
                           {
                               UpdateMoonStore(moonItems);
                               var result = UpdatedMoonList(moonItems);

                               Application.Current.Dispatcher.Invoke(
                                                                     new Action(
                                                                         () =>
                                                                         {
                                                                             DataMoons.ItemsSource = result;

                                                                             _towersScannedCount += moonItems.SelectMany(x => x.ItemsOnScan)
                                                                                 .Count(PosMappingUtils.IsTower);
                                                                             ProgressScan.Value = _towersScannedCount;

                                                                             ProgressScan.Maximum = _posMapper.TowersOnInitialScanCount;

                                                                             TxtScanProgress.Text = string.Format(
                                                                                                                  "Towers scanned: {1}/{0}",
                                                                                                                  _posMapper
                                                                                                                      .TowersOnInitialScanCount,
                                                                                                                  _towersScannedCount);
                                                                         }));
                           }
                       });
        }

        private void PosMapperOnNextScanRequest(long obj)
        {
            ProgressScan.Maximum = _posMapper.TowersOnInitialScanCount;

            TxtScanProgress.Text = string.Format("Towers scanned: {1}/{0}", _posMapper.TowersOnInitialScanCount, _towersScannedCount);
            SoundPlayer.PlayNextScan();
        }

        private void InitPosMapper()
        {
            _posMapper = new PosMapper2();
            _posMapper.ScanDone += PosMapperOnScanDone;
            _posMapper.NextScanRequest += PosMapperOnNextScanRequest;
            _posMapper.MoonsScanned += PosMapperOnMoonsScanned;
            _posMapper.OfflineTowersFound += PosMapperOnOfflineTowersFound;
            _posMapper.MoonClusterIsTooDense += PosMapperOnSkippingMoonsBecauseTheyAreTooClose;
            _posMapper.NeedToDeselectOverviewSettings += PosMapperOnNeedToDeselectOverviewSettings;
            _posMapper.IsActiveChanged += PosMapperOnIsActiveChanged;
        }

        private void PosMapperOnSkippingMoonsBecauseTheyAreTooClose(IList<IDScanItem> dScanItems)
        {
            //TODO show info on what moons can't be scanned
            TxtInfo.Text = "Some Moons are too close to each other, you have to scan them from another spot";
            //  TxtInfo.Foreground = Brushes.SolidWhiteBrush;
            TxtInfo.Background = Brushes.SolidRedBrush;
        }

        private void ScrollMoons_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer) sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void PosMapperOnOfflineTowersFound(int count)
        {
            TxtInfo.Text = string.Format("Found {0} offline POS'", count);
            TxtInfo.Background = Brushes.SolidLightRedBrush;
            SoundPlayer.PlayScanPotentialSuccess();
        }

        private void PosMapperOnScanDone()
        {
            Reset();
            SoundPlayer.PlayScanSuccess();
        }

        private IList<MoonItemViewModel> UpdatedMoonList(IList<MoonItem> moonItems)
        {
            var now = DateTime.UtcNow;
            foreach (var curItem in moonItems)
            {
                curItem.ScanTime = now;
            }

            var systemName = GetCurrentSystem(moonItems);

            //TODO das sollte auch woanders hin ...
            if (systemName != null && DataMoons.ItemsSource == null || !DataMoons.ItemsSource.Cast<object>()
                                                                            .Any())
            {
                return ExperimentalInefficientItemLoading(systemName);
            }

            if (DataMoons.ItemsSource == null)
            {
                return moonItems.Select(x => new MoonItemViewModel(x))
                    .OrderBy(x => x.MoonName)
                    .ToList();
            }

            var oldItems = DataMoons.ItemsSource.Cast<MoonItemViewModel>()
                .ToArray();
            return moonItems.Select(x => new MoonItemViewModel(x))
                .Concat(oldItems.Where(x => moonItems.All(m => m.Moon.Name != x.MoonName)))
                .OrderBy(x => x.MoonName)
                .ToList();
        }

        private static string GetCurrentSystem(IList<MoonItem> moonItems)
        {
            if (moonItems.Any())
            {
                var moonName = moonItems.First()
                    .Moon.Name;
                return moonName.Substring(0, moonName.IndexOf(' '));
            }

            return POSITION_TRACKER.CurrentSystemOfActiveCharacter;
        }

        //TODO das kann man natuerlich deutlich effizienter tun ...
        /// <summary>
        /// </summary>
        /// <param name="systemName"></param>
        private static IList<MoonItemViewModel> ExperimentalInefficientItemLoading(string systemName)
        {
            if (systemName == null)
            {
                return new List<MoonItemViewModel>();
            }

            var allItems = App.GetFromCollection<MoonItemModel, MoonItemModel>(c => c.Find(x => x.Id.StartsWith(systemName)));

            var tmp = App.GetFromCollection<MoonItemModel, MoonItemModel>(c => c.All());

            return allItems.Select(
                                   x => new MoonItemViewModel(
                                            new MoonItem(x.ItemsOnScan)
                                            {
                                                ScanTime = x.ScanTime
                                            }))
                .OrderBy(x => x.MoonName)
                .ToList();
        }

        private static void UpdateMoonStore(IEnumerable<MoonItem> moonItems)
        {
            var now = DateTime.UtcNow;
            using (var db = App.CreateStorageEngine())
            {
                var collection = db.GetCollection<MoonItemModel>(typeof (MoonItemModel).Name);
                foreach (var curItem in moonItems.Select(x => new MoonItemModel(x)))
                {
                    if (!collection.Update(curItem))
                    {
                        collection.Insert(curItem);
                    }
                }
            }
        }

        //TODO theoretisch gibt's hier race conditions beim anzeigen des progress texts und so, das muesste eigentlich nochmal anstaendig gemacht werden
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            ResetVisuals();
        }

        private void ResetVisuals()
        {
            TxtScanProgress.Text = "";

            ProgressScan.Value = 0;
            ProgressScan.Maximum = 1;
            _towersScannedCount = 0;
            ProgressScan.IsEnabled = false;
        }

        private void Reset()
        {
            //TxtInfo.Foreground = Brushes.Whi;
            TxtInfo.Background = Brushes.SolidTransparentBrush;
            TxtInfo.Text = "Copy DScan result to clipboard to start mapping POS'";

            _posMapper.Reset();
        }

        public void Activate()
        {
            if (_posMapper != null)
            {
                _posMapper.Activate();
            }
        }

        public void Deactivate()
        {
            if (_posMapper != null)
            {
                _posMapper.Deactivate();
            }
        }

        private void BtnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(x => x.TowerType != "none");
        }

        private void CopyToClipboard(Func<MoonItemViewModel, bool> constraint = null)
        {
            if (DataMoons.ItemsSource == null)
            {
                Clipboard.SetDataObject("");
                return;
            }

            var moonItems = constraint != null
                                ? DataMoons.ItemsSource.Cast<MoonItemViewModel>()
                                      .Where(constraint)
                                      .ToArray()
                                : DataMoons.ItemsSource.Cast<MoonItemViewModel>()
                                      .ToArray();
            var text = string.Join("\n", moonItems.Select(ToMoonItemCsv));

            Clipboard.SetDataObject(text);

            BtnCopyToClipboard.IsOpen = false;
        }

        private static string ToMoonItemCsv(MoonItemViewModel moonItem)
        {
            var text = string.Format("[{0}]\n", moonItem.MoonName);
            if (moonItem.TowerType == "none")
            {
                return text;
            }

            text = text
                   + string.Format(
                                   "POS Type\tFF\tPOS Name\tTime\t#Lootable\n{0}\t{1}\t{2}\t{3}\t{4}\n",
                                   moonItem.TowerType,
                                   moonItem.HasForceField ? "true" : "false",
                                   moonItem.TowerName,
                                   moonItem.ScanTimeString,
                                   moonItem.LootableModuleCount);
            if (moonItem.AggregatedItemsOnScan.Any())
            {
                var itemData = string.Format(
                                             "\nModule\tcount\n{0}\n",
                                             string.Join(
                                                         "\n",
                                                         moonItem.AggregatedItemsOnScan.OrderBy(x => x.Group)
                                                             .Select(x => string.Format("{0}\t{1}", x.Name, x.Amount))));

                text = text + itemData;
            }

            return text;
        }

        private void BtnCopyToClipboardWithEmptyMoons_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        private void BtnCopySelectionToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(x => DataMoons.SelectedItems.Contains(x));
        }
    }
}
