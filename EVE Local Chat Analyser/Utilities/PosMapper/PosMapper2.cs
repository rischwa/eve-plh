using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    public delegate void IsActiveChanged(bool isActive);

    public abstract class IsActiveBase : IDisposable
    {
        private bool _isActive;
        public event IsActiveChanged IsActiveChanged;
        protected readonly IScanAccess ScanAccess = DIContainer.GetInstance<IScanAccess>();

        protected IsActiveBase()
        {
            ScanAccess.ScanAccessChanged += OnScanAccessChanged;
        }

        private void OnIsActiveChanged(bool isactive)
        {
            var handler = IsActiveChanged;
            if (handler != null) handler(isactive);
        }

        public bool IsActive
        {
            get { return _isActive; }
            protected set
            {
                if (_isActive == value)
                {
                    return;
                }

                _isActive = value;
                OnIsActiveChanged(value);
            }
        }

        protected void OnScanAccessChanged(object target)
        {
            IsActive = target == this;
        }

        public void Activate()
        {
            ScanAccess.GetExclusiveAccess(this);
        }

        public void Deactivate()
        {
            ScanAccess.RemoveAccess(this);
        }

        public void Dispose()
        {
            ScanAccess.ScanAccessChanged -= OnScanAccessChanged;
            ScanAccess.UnregisterCallback(this);
        }
    }

    public sealed class PosMapper2 : IsActiveBase
    {
        private static readonly ISet<String> EXCLUDED_TYPES = new HashSet<string>
            {
                "Asteroid Belt",
                "Asteroidengürtel",
                "Customs Office", //TODO german
                "Sun",
                "Sonne",
                "Stargate",
                "Sternentor" //TODO heisst das so in german
            };

        //TODO di un dso
        
        private readonly Stack<IPosMappingScanNode> _nodesToScan = new Stack<IPosMappingScanNode>();
        private IPosMappingScanNode _currentNodeToScan;

        public PosMapper2()
        {
            ScanAccess.RegisterCallback(this, ClipboardParserOnNewDirectionalScan, null);
        }      

        public int TowersOnInitialScanCount { get; private set; }

        public event Action<IList<MoonItem>> MoonsScanned;
        public event Action<IList<IDScanItem>> MoonClusterIsTooDense;
        public event Action<int> OfflineTowersFound;
        public event Action NeedToDeselectOverviewSettings;

        private void OnNeedToDeselectOverviewSettings()
        {
            var handler = NeedToDeselectOverviewSettings;
            if (handler != null) handler();
        }


        public event Action ScanDone;
        public event Action<long> NextScanRequest;

        private void OnNextScanRequest(long obj)
        {
            ScanAccess.SetClipboardText(this, _currentNodeToScan.PivotalScanRangeInKm.ToString(CultureInfo.InvariantCulture));

            var handler = NextScanRequest;
            if (handler != null) handler(obj);
        }

        private void OnOfflineTowersFound(int obj)
        {
            var handler = OfflineTowersFound;
            if (handler != null) handler(obj);
        }

        private void OnScanDone()
        {
            var handler = ScanDone;
            if (handler != null) handler();
        }

        public void OnMoonClusterIsTooDense(IList<IDScanItem> obj)
        {
            var handler = MoonClusterIsTooDense;
            if (handler != null) handler(obj);
        }


        private void ClipboardParserOnNewDirectionalScan(IList<IDScanItem> items)
        {
            OnNewDScan(items.Where(IsIncludedItem).ToList());
        }

        private static bool IsIncludedItem(IDScanItem arg)
        {
            return !EXCLUDED_TYPES.Contains(arg.Type) && !arg.Type.StartsWith("Planet (") && !arg.Type.EndsWith("Wreck") &&
                   !arg.Type.EndsWith("Wrack");
        }

        public void Reset()
        {
            _nodesToScan.Clear();
            _currentNodeToScan = null;
        }

        public void OnMoonsScanned(IList<MoonItem> obj)
        {
            var handler = MoonsScanned;
            if (handler != null) handler(obj);
        }

        private void OnNewDScan(IList<IDScanItem> items)
        {
            if (!items.Any(item => item.IsMoon()) && items.Any(item => item.IsTower()))
            {
                OnNeedToDeselectOverviewSettings();
                return;
            }
            if (_currentNodeToScan == null)
            {
                InitialScan(items);
                return;
            }

            _currentNodeToScan.SetItemsOnPivotalScan(items);

            if (!_nodesToScan.Any())
            {
                OnScanDone();
                return;
            }

            _currentNodeToScan = _nodesToScan.Pop();
            OnNextScanRequest(_currentNodeToScan.PivotalScanRangeInKm);
        }

        private void InitialScan(IList<IDScanItem> obj)
        {
            CheckForOfflineTowers(obj);

            _currentNodeToScan = new DefaultPosMappingScanNode(this, PosMappingUtils.GetMoonGroups(obj),
                                                               new IDScanItem[0], obj);
            if (_currentNodeToScan.IsLeaf)
            {
                var moonItems = _currentNodeToScan.GetMoonItems();
                if (moonItems.Any())
                {
                    OnMoonsScanned(moonItems);
                }
                OnScanDone();
                return;
            }

            OnNextScanRequest(_currentNodeToScan.PivotalScanRangeInKm);
        }

        private void CheckForOfflineTowers(IList<IDScanItem> obj)
        {
            TowersOnInitialScanCount = obj.Count(PosMappingUtils.IsTower);
            var forceFieldCount = obj.Count(PosMappingUtils.IsForceField);
            var offlineTowerCount = TowersOnInitialScanCount - forceFieldCount;
            if (offlineTowerCount > 0)
            {
                OnOfflineTowersFound(offlineTowerCount);
            }
        }

        public void AddNodeToScan(IPosMappingScanNode node)
        {
            _nodesToScan.Push(node);
        }
    }
}