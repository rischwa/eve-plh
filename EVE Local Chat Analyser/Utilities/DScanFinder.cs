using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities.PosMapper;

namespace EveLocalChatAnalyser.Utilities
{
    public class DScanFinder : IsActiveBase
    {
        public const int KM_OFFSET = 7001;
        public const int MIN_DISTANCE_IN_KM = 500;
        private List<DScanGroup> _groups;
        private IList<IDScanItem> _items;
        private long _median;
        private string _selectedItemName;
        private string _selectedItemType;

        public IList<DScanGroup> Groups { get { return _groups.ToList(); } }

        public DScanFinder()
        {
            ScanAccess.RegisterCallback(this, DscanCallback, ProbeScanCallback);
            Init();
        }

        private void ProbeScanCallback(IList<IProbeScanItem> probeScanItems)
        {
            var handler = ProbeScan;
            if (handler != null) handler(probeScanItems);
        }

        private void DscanCallback(IList<IDScanItem> dScanItems)
        {
            var handler = DirectionalScan;
            if (handler != null) handler(dScanItems);
        }

        private event Action<IList<IDScanItem>> DirectionalScan;

        private event Action<IList<IProbeScanItem>> ProbeScan;

        public event Action<DScanGroup> Success;

        public event Action LowerVerificationScanRequested;

        public event Action UpperVerificationScanRequested;


        protected virtual void OnUpperVerificationScanRequested()
        {
            Action handler = UpperVerificationScanRequested;
            if (handler != null) handler();
        }

        protected virtual void OnLowerVerificationScanRequested()
        {
            Action handler = LowerVerificationScanRequested;
            if (handler != null) handler();
        }

        protected virtual void OnSuccess(DScanGroup obj)
        {
            Action<DScanGroup> handler = Success;
            if (handler != null) handler(obj);
        }

        public event Action Fail;

        protected virtual void OnFail()
        {
            Action handler = Fail;
            if (handler != null) handler();
        }

        public void Init()
        {
            DirectionalScan -= NextScan;
            ProbeScan -= AddAnomalies;
            DirectionalScan -= VerificationScan;
            _median = 0;
            _items = null;
            _groups = null;
            DirectionalScan += InitDScan;
        }

        private void InitDScan(IList<IDScanItem> dScanItems)
        {
            _items = dScanItems;
            DirectionalScan -= InitDScan;
            ProbeScan += AddAnomalies;

            SetInitialItems();
            if (_groups != null)//todo sehr unschoen, passiert wenn init leer ist
            {
                CheckFinished();
            }
        }

        private void SetInitialItems()
        {
            var initialGroups = (from curItem in _items
                         where curItem.Distance.HasValue && curItem.Distance.KmValue > MIN_DISTANCE_IN_KM
                         let boundary = new Boundary(LINQExtensions.GetLowerBound(curItem.Distance.KmValue), LINQExtensions.GetUpperBound(curItem.Distance.KmValue))
                         group curItem by boundary
                         into g
                         orderby g.Key.Upper ascending
                         select new DScanGroup {Boundary = g.Key, Items = g.ToList()}).ToList();

            if (!initialGroups.Any())
            {
                Init();
                return;
            }

            _groups = MergeCloseGroups(initialGroups);

            if (_groups.Any())
            {
                _median = _groups.Median();
            }

            OnInitialItems(_items);
        }

        private static List<DScanGroup> MergeCloseGroups(IEnumerable<DScanGroup> groups)
        {
            var result = new List<DScanGroup>();

            DScanGroup lastGroup = null;
            foreach (var curGroup in groups)
            {
                if (lastGroup == null)
                {
                    lastGroup = curGroup;
                }
                else
                {
                    if (curGroup.Boundary.Upper - lastGroup.Boundary.Upper <= KM_OFFSET)
                    {
                        lastGroup.Boundary = new Boundary(lastGroup.Boundary.Lower, curGroup.Boundary.Upper);
                        lastGroup.Items = lastGroup.Items.Union(curGroup.Items).ToList();
                    }
                    else
                    {
                        result.Add(lastGroup);
                        lastGroup = curGroup;
                    }
                }
            }

            result.Add(lastGroup);

            return result;
        }

        public event Action<IList<IProbeScanItem>> AddedAnomlies;

        protected virtual void OnAddedAnomlies(IList<IProbeScanItem> anoms)
        {
            Action<IList<IProbeScanItem>> handler = AddedAnomlies;
            if (handler != null) handler(anoms);
        }

        public void AddAnomalies(IList<IProbeScanItem> anomalies)
        {
            ProbeScan -= AddAnomalies;
            _items = _items.Union(anomalies.Where(item => item.Distance.KmValue <= Int32.MaxValue)).ToList();
            OnAddedAnomlies(anomalies);
            SetInitialItems();
        }

        public event Action<IList<IDScanItem>> InitialItems;

        protected virtual void OnInitialItems(IList<IDScanItem> obj)
        {
            Action<IList<IDScanItem>> handler = InitialItems;
            if (handler != null) handler(obj);
        }

        public event Action<DScanGroup> PotentiallyFoundAtDScanItemGroup;

        protected virtual void OnPotentiallyFoundAtDScanItemGroup(DScanGroup obj)
        {
            Action<DScanGroup> handler = PotentiallyFoundAtDScanItemGroup;
            if (handler != null) handler(obj);
        }

        public event Action<long> NextScanRequested;

        protected virtual void OnNextScanRequested(long obj)
        {
            Action<long> handler = NextScanRequested;
            if (handler != null) handler(obj);
        }

        private bool CheckFinished()
        {
            if (_groups.Count > 1)
            {
                return false;
            }

            DirectionalScan -= NextScan;
            if (IsFinished)
            {
                OnPotentiallyFoundAtDScanItemGroup(_groups.First());
            }
            return true;
        }

        private bool IsFinished
        {
            get { return _groups.Count == 1; }
        }

        public void SelectItem(DScanItem item)
        {
            _selectedItemName = item.Name;
            _selectedItemType = item.Type;

            ProbeScan -= AddAnomalies;
            DirectionalScan += NextScan;

            SetClipboardTextToCurrentMedian();
            
            OnNextScanRequested(_median);
        }

        private bool _isLowerVerificationScan;

        private void NextScan(IList<IDScanItem> newItems)
        {
            if (newItems.Any(item => item.Name == _selectedItemName && item.Type == _selectedItemType))
            {
                _groups = _groups.TakeWhile(grouping => grouping.Boundary.Upper <= _median).ToList();
            }
            else
            {
                _groups = _groups.SkipWhile(grouping => grouping.Boundary.Upper <= _median).ToList();
            }
            if (!CheckFinished())
            {
                _median = _groups.Median();
                SetClipboardTextToCurrentMedian();
                OnNextScanRequested(_median);
            }
            else
            {
                if (!_groups.Any())
                {
                    Fail();
                }
                else
                {
                    _isLowerVerificationScan = true;
                    DirectionalScan += VerificationScan;
                    _median = _groups.First().Boundary.Lower;
                    SetClipboardTextToCurrentMedian();
                    OnLowerVerificationScanRequested();
                }
            }
        }

        private void VerificationScan(IList<IDScanItem> items)
        {
            if (_isLowerVerificationScan)
            {
                LowerVerificationScan(items);
            }
            else
            {
                UpperVerificationScan(items);
            }
        }

        private void UpperVerificationScan(IList<IDScanItem> items)
        {
            DirectionalScan -= VerificationScan;
            if (!items.Any(x => x.Name == _selectedItemName && x.Type == _selectedItemType))
            {
                OnFail();
            }
            else
            {
                OnSuccess(_groups.First());
            }
        }

        private void LowerVerificationScan(IList<IDScanItem> items)
        {
            if (items.Any(x => x.Name == _selectedItemName && x.Type == _selectedItemType))
            {
                OnFail();
            }
            else
            {
                _median = _groups.First().Boundary.Upper;
                SetClipboardTextToCurrentMedian();
                _isLowerVerificationScan = false;
                OnUpperVerificationScanRequested();
            }
        }

        public void SetClipboardTextToCurrentMedian()
        {
            ScanAccess.SetClipboardText(this, _median.ToString(CultureInfo.InvariantCulture));
        }

        public class Boundary
        {
            public readonly long Lower;
            public readonly long Upper;

            public Boundary(long lower, long upper)
            {
                Upper = upper;
                Lower = lower;
            }

            protected bool Equals(Boundary other)
            {
                return Lower == other.Lower && Upper == other.Upper;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Lower.GetHashCode()*397) ^ Upper.GetHashCode();
                }
            }
        }

        public class DScanGroup
        {
            public IList<IDScanItem> Items { get; set; }
            public Boundary Boundary { get; set; }
        }
    }
}