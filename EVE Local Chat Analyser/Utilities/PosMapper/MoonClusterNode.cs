using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    public sealed class MoonClusterNode : AbstractPosMappingScanNode
    {
        private readonly long _lowerBoundaryInKm;
        private readonly long _sizeInKm;
        private readonly long _upperBoundaryInKm;

        public MoonClusterNode(PosMapper2 posMapper, long lowerBoundaryInKm, long upperBoundaryInKm,
                               IList<IDScanItem> leftBorderScan, IList<IDScanItem> rightBorderScan)
            : base(posMapper, leftBorderScan, rightBorderScan)
        {
            _upperBoundaryInKm = upperBoundaryInKm;
            _lowerBoundaryInKm = lowerBoundaryInKm;

            _sizeInKm = upperBoundaryInKm - lowerBoundaryInKm;
            PivotalScanRangeInKm = upperBoundaryInKm - _sizeInKm/2;

            var towerCount = ItemsOnScan.Count(PosMappingUtils.IsTower);

            if (IsClusterTooDense)
            {
                IsLeaf = true;
                PosMapper.OnMoonClusterIsTooDense(ItemsOnScan.Where(PosMappingUtils.IsMoon).ToList());
                //TODO on dings nicht public machen, sondern actions uebergeben oder so
            }
            else
            {
                IsLeaf = towerCount == 0 ||
                         (towerCount == 1 && ItemsOnScan.Count(PosMappingUtils.IsMoon) == 1);//TODO extension method fuer count == 1 (kann bei 2 abbrechen)
            }
        }

        private bool IsClusterTooDense
        {
            get { return _sizeInKm < 12000; }
        }

        public override IList<MoonItem> GetMoonItems()
        {
            return IsClusterTooDense ? new MoonItem[0] : base.GetMoonItems();
        }

        public override void SetItemsOnPivotalScan(IList<IDScanItem> items)
        {
            var leftChild = new MoonClusterNode(PosMapper, _lowerBoundaryInKm, PivotalScanRangeInKm, LeftBorderScan,
                                                items);

            var rightChild = new MoonClusterNode(PosMapper, PivotalScanRangeInKm, _upperBoundaryInKm, items,
                                                 RightBorderScan);

            ProcessChild(leftChild);
            ProcessChild(rightChild);
        }
    }
}