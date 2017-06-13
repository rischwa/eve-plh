using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Ui.PosMapper;
using EveLocalChatAnalyser.Utilities;
using Brushes = EveLocalChatAnalyser.Ui.Models.Brushes;

namespace EveLocalChatAnalyser.Ui.DScanLocator
{
    /// <summary>
    ///     Interaction logic for DScanLocatorControl.xaml
    /// </summary>
    public partial class DScanLocatorControl : UserControl, IDisposable
    {
        private static readonly Brush POTENTIALLY_FOUND_BRUSH = new SolidColorBrush(Colors.LightYellow);
        private static readonly Brush SUCCESS_BACKGROUND_BRUSH = new SolidColorBrush(Colors.LightGreen);
        private static readonly Brush RED_BRUSH = new SolidColorBrush(Colors.LightCoral);
        private static WindowInstanceManager<PosMapperDialog> _posMapper;

        private static readonly string[] POTENTIAL_TARGET_TYPES = {"Mobile Depot", "Mobiles Depot", "Infrastructure Hub", "Territorial Claim Unit", "Sovereignty Blockade Unit", "Jump Bridge" }; // TODO deutsche uebersetzungen


        private readonly DScanFinder _dScanFinder;
        private bool _areAnomaliesAdded;
        private List<IDScanItem> _itemsOnInitialScan;
        private IList<IProbeScanItem> _lastAnomalies;
        private DScanItem _selectedItem;
        private int _stepCount;
        private readonly IEveTypes _types;

        public DScanLocatorControl()
        {
            if (_posMapper == null)
            {
                _posMapper = new WindowInstanceManager<PosMapperDialog>((MainWindow) Application.Current.MainWindow);
            }

            CurrentRangeItems = new ObservableCollection<DScanItemViewModel>();
            CurrentShips = new ObservableCollection<IDScanItem>();
            InitializeComponent();
            //TODO DI und so
            _dScanFinder = DIContainer.GetInstance<DScanFinder>();
            _dScanFinder.InitialItems += InitialItems;
            _dScanFinder.AddedAnomlies += AddedAnomalies;
            _dScanFinder.NextScanRequested += NextScanRequested;
            _dScanFinder.PotentiallyFoundAtDScanItemGroup += TargetPotentiallyFoundAt;
            _dScanFinder.Fail += ScanFailed;
            _dScanFinder.Success += ScanSuccess;
            _dScanFinder.LowerVerificationScanRequested += LowerVerification;
            _dScanFinder.UpperVerificationScanRequested += UpperVerification;
            _dScanFinder.IsActiveChanged += DScanFinderOnIsActiveChanged;

            _types = DIContainer.GetInstance<IEveTypes>();

            Initialize();
        }

        public ObservableCollection<DScanItemViewModel> CurrentRangeItems { get; private set; }

        public ObservableCollection<IDScanItem> CurrentShips { get; private set; }

        public void Dispose()
        {
            _dScanFinder.InitialItems -= InitialItems;
            _dScanFinder.AddedAnomlies -= AddedAnomalies;
            _dScanFinder.NextScanRequested -= NextScanRequested;
            _dScanFinder.PotentiallyFoundAtDScanItemGroup -= TargetPotentiallyFoundAt;
            _dScanFinder.Fail -= ScanFailed;
            _dScanFinder.Success -= ScanSuccess;
            _dScanFinder.LowerVerificationScanRequested -= LowerVerification;
            _dScanFinder.UpperVerificationScanRequested -= UpperVerification;
            _dScanFinder.IsActiveChanged -= DScanFinderOnIsActiveChanged;

            _dScanFinder.Dispose();
        }

        private void DScanFinderOnIsActiveChanged(bool isActive)
        {
            DScanLocatorBusyIndicator.IsBusy = !isActive;
        }

        private void UpperVerification()
        {
            AppendTextLine("\n" + _stepCount++ +
                           ": for final verification (upper bound) double-click on DScan range input and press ctrl+v, press \"Scan\" button, click into result list and press ctrl+a, ctrl+c");
            SoundPlayer.PlayNextScan();
        }

        private void LowerVerification()
        {
            AppendTextLine("\n" + _stepCount++ +
                           ": for final verification (lower bound) double-click on DScan range input and press ctrl+v, press \"Scan\" button, click into result list and press ctrl+a, ctrl+c");
        }

        private void ScanSuccess(DScanFinder.DScanGroup dScanGroup)
        {
            var textData = String.Format("\n\n{0} most likely found", _selectedItem);
            AppendTextLine(textData);

            LstRangeItems.Background = SUCCESS_BACKGROUND_BRUSH;
            LstRangeItems.FontWeight = FontWeights.Bold;
            SetRangeItems(dScanGroup.Items);

            SoundPlayer.PlayScanSuccess();
        }

        private void ScanFailed()
        {
            var textData = String.Format("\n\n{0} could not be found at a specific location", _selectedItem);
            AppendTextLine(textData);

            LstRangeItems.Background = RED_BRUSH;
            SetRangeItems(new List<IDScanItem>());

            SoundPlayer.PlayScanFailure();
        }

        private void AddedAnomalies(IList<IProbeScanItem> probeScanItems)
        {
            BtnResetButKeepAnoms.IsEnabled = true; //TODO viewmodel ...
            _lastAnomalies = probeScanItems;
            _areAnomaliesAdded = true;
            AppendTextLine("\n" + _stepCount++ +
                           ": Anomalies are added, now double-click on the target you want to search for (in this window)");
            SoundPlayer.PlayScanItemsAdded();
        }

        public void Initialize()
        {
            _stepCount = 1;
            _areAnomaliesAdded = false;
            var color = Resources["ContainerBackground"] as  Color?;
            LstRangeItems.Background = Brushes.TransparentBrush;//color == null ? Brushes.TransparentBrush : new SolidColorBrush(color.Value);
            LstRangeItems.FontWeight = FontWeights.Normal;

            CurrentShips.Clear();
            CurrentRangeItems.Clear();

            _dScanFinder.Init();

            LblTarget.Visibility = Visibility.Collapsed;
            LstRangeItems.Visibility = Visibility.Collapsed;
            LstTargets.Visibility = Visibility.Visible;

            TxtStatus.Clear();
            AppendTextLine(_stepCount++ +
                           ": Deselect \"Use active overview settings\" in DScan, press \"Scan\", click into the result list and press ctrl+a, ctrl+c");

            DScanFinderOnIsActiveChanged(_dScanFinder.IsActive);
        }

        private void TargetPotentiallyFoundAt(DScanFinder.DScanGroup dScanGroup)
        {
            var textData = String.Format("\n\n{0} potentially found", _selectedItem);
            AppendTextLine(textData);

            LstRangeItems.Background = POTENTIALLY_FOUND_BRUSH;

            SetRangeItems(dScanGroup.Items);

            SoundPlayer.PlayScanPotentialSuccess();
        }

        private void SetRangeItems(IEnumerable<IDScanItem> dScanItems)
        {
            CurrentRangeItems.Clear();
            foreach (var curItem in dScanItems)
            {
                CurrentRangeItems.Add(new DScanItemViewModel(curItem));
            }
        }

        private void NextScanRequested(long obj)
        {
            AppendTextLine("\n" + _stepCount++ +
                           ": Double-click on DScan range input and press ctrl+v, press \"Scan\" button, click into result list and press ctrl+a, ctrl+c");
            MarkDScanItemsOutOfRange();
            SoundPlayer.PlayNextScan();
        }

        private void MarkDScanItemsOutOfRange()
        {
            var remainingItems = _dScanFinder.Groups.SelectMany(x => x.Items).ToList();
            foreach (
                var curItem in
                    LstRangeItems.Items.Cast<DScanItemViewModel>()
                                 .Where(curItem => !remainingItems.Contains(curItem.Item)))
            {
                //curItem.BackgroundColor = RED_BRUSH;
                curItem.Visibility = Visibility.Collapsed;
            }
        }

        private void AppendTextLine(string textData)
        {
            TxtStatus.AppendText(textData);
            TxtStatus.ScrollToEnd();
        }

        private void InitialItems(IList<IDScanItem> obj)
        {
            CurrentShips.Clear();
            foreach (var curItem in obj.Where(IsTargetItemNotOnGrid))
            {
                CurrentShips.Add(curItem);
            }

            var dScanItems = _dScanFinder.Groups.SelectMany(x => x.Items);
            SetRangeItems(dScanItems);


            if (!_areAnomaliesAdded)
            {
                if (_lastAnomalies != null)
                {
                    if (IsScanFromADifferentPosition(obj))
                    {
                        DisableLastAnomalies();
                        MessageBox.Show(Application.Current.MainWindow,
                                        "It seems that you have moved from the position where you added the anomalies.\nPlease add them again.\nMake sure, you deselected \"Show Anomalies\" and select it again, before you add them.\nBecause otherwise the (in game) probe scanner window won't update the ranges.");
                        AddOptionalAnomTextLines();
                    }
                    else
                    {
                        _dScanFinder.AddAnomalies(_lastAnomalies);
                    }
                }
                else
                {
                    AddOptionalAnomTextLines();
                }
            }

            _itemsOnInitialScan = obj.Where(x => x.Distance.HasValue && x.Distance.KmValue > 10000).ToList();
            SoundPlayer.PlayScanItemsAdded();
        }

        private void DisableLastAnomalies()
        {
            _lastAnomalies = null;
            BtnResetButKeepAnoms.IsEnabled = false;
        }

        private void AddOptionalAnomTextLines()
        {
            AppendTextLine("\n" + _stepCount++ +
                           ": (Optional) add anomalies by opening \"Probe Scanner\", deselecting and reselecting \"Show Anomlies\", click into the result list and press ctrl+a, ctrl+c");
            AppendTextLine("\n" + _stepCount++ +
                           ": Double-click on the target you want to search for (in this window)");
        }

        private bool IsScanFromADifferentPosition(IEnumerable<IDScanItem> dScanItems)
        {
            return !(from item in _itemsOnInitialScan
                     let correspondingItem = dScanItems.FirstOrDefault(x => x.Name == item.Name)
                     where
                         correspondingItem == null ||
                         Math.Abs(correspondingItem.Distance.KmValue - item.Distance.KmValue) > 5000
                     select item).Any();
        }

        private bool IsTargetItemNotOnGrid(IDScanItem arg)
        {
            return !arg.Distance.HasValue && (POTENTIAL_TARGET_TYPES.Contains(arg.Type) || _types.IsShipTypeName(arg.Type));
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listview = (ListView) sender;

            var selectedItem = listview.SelectedItem as DScanItem;
            if (selectedItem == null)
            {
                return;
            }

            _selectedItem = selectedItem;

            LblTarget.Content = selectedItem;
            LblTarget.Visibility = Visibility.Visible;

            LstTargets.Visibility = Visibility.Collapsed;
            LstRangeItems.Visibility = Visibility.Visible;

            CurrentShips.Clear();
            CurrentShips.Add(selectedItem);
            _dScanFinder.SelectItem(selectedItem);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DisableLastAnomalies();
            _itemsOnInitialScan = null;
            Clipboard.SetDataObject(Int32.MaxValue.ToString(CultureInfo.InvariantCulture));
            Initialize();
        }

        private void BtnPosMapper_Click(object sender, RoutedEventArgs e)
        {
            _posMapper.Show();
        }

        private void BtnResetButKeepAnoms_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(Int32.MaxValue.ToString(CultureInfo.InvariantCulture));
            Initialize();
        }

        public void Activate()
        {
            if (_dScanFinder != null)
            {
                _dScanFinder.Activate();
            }
        }

        public void Deactivate()
        {
            if (_dScanFinder != null)
            {
                _dScanFinder.Deactivate();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Activate();
        }
    }
}