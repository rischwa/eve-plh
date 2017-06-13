using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using EveLocalChatAnalyser.Utilities.PosMapper;

namespace EveLocalChatAnalyser.Ui.Map
{
    /// <summary>
    ///     Interaction logic for DScanShipAggregator.xaml
    /// </summary>
    public partial class DScanShipAggregator : UserControl, IDisposable
    {
        private const string NUMBER_FORMAT = "###\u200A###\u200A##0";
        private static readonly RoleComparer ROLE_COMPARER = new RoleComparer();
        private static readonly GroupComparer GROUP_COMPARER = new GroupComparer();

        private readonly ClipboardParser _clipboardParser = DIContainer.GetInstance<ClipboardParser>();

        private readonly IPositionTracker _positionTracker = DIContainer.GetInstance<IPositionTracker>();
        private string _positionOfScan;
        private List<AggregatedShipTypeViewModel> _shipsTypes;
        private IDScanItem[] _lastScan;

        public DScanShipAggregator()
        {
            InitializeComponent();

            CboGroupBy.SelectedItem = CboGroupBy.Items.Cast<ComboBoxItem>()
                                                .FirstOrDefault(
                                                                x =>
                                                                (string) x.Content == Properties.Settings.Default.DScanShipAggregatorGroupBy);

            _clipboardParser.DirectionalScan += SetScan;
        }

        public void Dispose()
        {
            _clipboardParser.DirectionalScan -= SetScan;
        }

        public void SetScan(IList<IDScanItem> scan)
        {
            _positionOfScan = _positionTracker.CurrentSystemOfActiveCharacter;
            _lastScan = scan.ToArray();
            InitViewModel(scan);

            SetGroupedShips();

            InitTotals();
        }

        //TODO copy buttons disablen wenn keine schiffe da sind


        private void BtnCopyIntel_OnClicked(object sender, RoutedEventArgs e)
        {
            if (_shipsTypes == null || _shipsTypes.All(x => x.Category.Category == "Civilian"))
            {
                return;
            }

            var builder = new StringBuilder();
            if (_positionOfScan != null)
            {
                builder.Append(_positionOfScan)
                       .Append(": ");
            }

            builder.Append(string.Join(", ",
                                       _shipsTypes.Where(x => x.Category.Category != "Civilian")
                                                  .Select(x => x.Count == 1 ? x.Name : x.Count + "x " + x.Name)));

            Clipboard.SetDataObject(builder.ToString());
        }

        private void CboGroupBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.Cast<ComboBoxItem>()
                        .FirstOrDefault();
            if (item == null)
            {
                return;
            }
            Properties.Settings.Default.DScanShipAggregatorGroupBy = (string) item.Content;

            SetGroupedShips();
        }

        private void InitTotals()
        {
            TotalCount.Text = _shipsTypes.Sum(x => x.Count)
                                         .ToString(CultureInfo.InvariantCulture);
            TotalMinDps.Text = _shipsTypes.Sum(x => x.MinDps)
                                          .ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture);
            TotalMaxDps.Text = _shipsTypes.Sum(x => x.MaxDps)
                                          .ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture);
            TotalRep.Text = _shipsTypes.Sum(x => x.Rep)
                                       .ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture);
        }

        private void InitViewModel(IList<IDScanItem> scan)
        {
            _shipsTypes = scan.Where(PosMappingUtils.IsShip)
                              .GroupBy(item => item.Type)
                              .Select(items => new AggregatedShipTypeViewModel
                                                   {
                                                       Count = items.Count(),
                                                       Name = items.Key
                                                   })
                              .ToList();

            InsertGroupCounts();
        }

        private void InsertGroupCounts()
        {
            var groupedByGroup = _shipsTypes.GroupBy(x => x.ShipGroup);
            foreach (var curGroup in groupedByGroup)
            {
                var count = curGroup.Sum(x => x.Count);
                foreach (var aggregatedShipTypeViewModel in curGroup)
                {
                    aggregatedShipTypeViewModel.GroupCount = count;
                }
            }
        }

        private void SetGroupedShips()
        {
            if (_shipsTypes == null)
            {
                return;
            }

            var order = Properties.Settings.Default.DScanShipAggregatorGroupBy;
            if (order == "Role")
            {
                _shipsTypes = _shipsTypes.OrderBy(x => x, ROLE_COMPARER)
                                         .ToList();

                var vs = new ListCollectionView(_shipsTypes);
                // ReSharper disable PossibleNullReferenceException
                vs.GroupDescriptions.Add( // ReSharper restore PossibleNullReferenceException
                                         new PropertyGroupDescription(
                                             NotifyUtils.GetPropertyName((AggregatedShipTypeViewModel a) => a.FleetRole)));

                DataShips.ItemsSource = vs;
            }
            else
            {
                _shipsTypes = _shipsTypes.OrderBy(x => x, GROUP_COMPARER)
                                         .ToList();
                var vs = new ListCollectionView(_shipsTypes);
                // ReSharper disable PossibleNullReferenceException
                vs.GroupDescriptions.Add( // ReSharper restore PossibleNullReferenceException
                                         new PropertyGroupDescription(
                                             NotifyUtils.GetPropertyName((AggregatedShipTypeViewModel a) => a.ShipGroup)));

                DataShips.ItemsSource = vs;
            }
        }

        private class GroupComparer : IComparer<AggregatedShipTypeViewModel>
        {
            public int Compare(AggregatedShipTypeViewModel x, AggregatedShipTypeViewModel y)
            {
                //if (x.Category.GroupPriority == y.Category.GroupPriority)
                //  {
                if (x.GroupCount == y.GroupCount)
                {
                    //oder priority?
                    return CompareByGroupName(x, y);
                }

                return x.GroupCount > y.GroupCount ? -1 : 1;
                //   }
                //  return x.Category.GroupPriority < y.Category.GroupPriority ? -1 : 1;
            }

            private static int CompareByGroupName(AggregatedShipTypeViewModel x, AggregatedShipTypeViewModel y)
            {
                return x.ShipGroup == y.ShipGroup
                           ? String.Compare(x.Name, y.Name, StringComparison.Ordinal)
                           : String.Compare(x.ShipGroup, y.ShipGroup, StringComparison.Ordinal);
            }
        }

        private class RoleComparer : IComparer<AggregatedShipTypeViewModel>
        {
            private static readonly Dictionary<string, int> TYPE_ORDER = new Dictionary<string, int>
                                                                             {
                                                                                 {"Capitals", 0},
                                                                                 {"DPS", 1},
                                                                                 {"Tackle", 4},
                                                                                 {"EWAR", 3},
                                                                                 {"Logi", 2},
                                                                                 {"Fancy", 5},
                                                                                 {"Civilian", 7},
                                                                                 {"unknown", 6}
                                                                             };

            public int Compare(AggregatedShipTypeViewModel x, AggregatedShipTypeViewModel y)
            {
                var xOrder = TYPE_ORDER[x.FleetRole];
                var yOrder = TYPE_ORDER[y.FleetRole];

                if (xOrder == yOrder)
                {
                    if (x.Count == y.Count)
                    {
                        return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    }
                    return x.Count > y.Count ? -1 : 1;
                }

                return xOrder < yOrder ? -1 : 1;
            }
        }

        private void BtnCopyIntelLink_OnClicked(object sender, RoutedEventArgs e)
        {
            if (_lastScan == null)
            {
                return;
            }
            try
            {
                var dscanString = string.Join("\n", _lastScan.Select(x => x.ToDScanString()));

                const string URL = "http://vserver.zap.de.com/intel/intelSubmit";

                var client = new HttpClient();
                {
                    client.Timeout = new TimeSpan(0, 0, 0, 1, 500);
                    var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                                                                                               {"text", dscanString},
                                                                                               {"submit", "Ok!"}
                                                                                           });

                    client.PostAsync(URL, content)
                          .ContinueWith(task =>
                              {
                                  var referer = task.Result.RequestMessage.RequestUri.ToString();
                                 // Process.Start(referer);
                                  Application.Current.Dispatcher.BeginInvoke(new Action(() => { Clipboard.SetDataObject(referer); SoundPlayer.PlayNextScan(); }));
                                  client.Dispose();
                              });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't create link: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }



            //     var request = WebRequest.CreateHttp(url);
       //     using (WebClient client = new WebClient())
       //     {

       //         byte[] response =
       //         client.UploadValues("http://dork.com/service", new NameValueCollection()
       //{
       //    { "text", dscanString },
       //    { "submit", "Ok!" }
       //});

       //         string result = System.Text.Encoding.UTF8.GetString(response);
       //     }

        }
    }
}
