using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    public class DefaultPosMappingScanNode : AbstractPosMappingScanNode
    {
        private readonly IList<IGrouping<long, IDScanItem>> _moonGroups;

        public DefaultPosMappingScanNode(PosMapper2 posMapper, IList<IGrouping<long, IDScanItem>> moonGroups,
                                         IList<IDScanItem> leftBorderScan, IList<IDScanItem> rightBorderScan)
            : base(posMapper, leftBorderScan, rightBorderScan)
        {
            _moonGroups = moonGroups;

            var towerCount = ItemsOnScan.Count(PosMappingUtils.IsTower);
            IsLeaf = towerCount == 0 || (towerCount == 1 && moonGroups.Count < 2);

            if (!IsLeaf)
            {
                PivotalScanRangeInKm =
                    PosMappingUtils.GetLowerBoundInKm(_moonGroups.Skip(_moonGroups.Count/2).First().First().Distance);
            }
        }

        protected IList<IDScanItem> ItemsOnPivotalScan { get; set; }

        private static IPosMappingScanNode CreateNode(PosMapper2 posMapper, IList<IDScanItem> leftBorderItems,
                                                      IList<IDScanItem> rightBorderItems)
        {
            var itemsAtNode = rightBorderItems.Without(leftBorderItems).ToList();
            var moonGroups = PosMappingUtils.GetMoonGroups(itemsAtNode);

            if (IsCluster(moonGroups))
            {
                var moonGroup = moonGroups.First().First();

                return new MoonClusterNode(posMapper, PosMappingUtils.GetLowerBoundInKm(moonGroup.Distance),
                                           PosMappingUtils.GetUpperBoundInKm(moonGroup.Distance), leftBorderItems,
                                           rightBorderItems);
            }

            return new DefaultPosMappingScanNode(posMapper, moonGroups, leftBorderItems, rightBorderItems);
        }

        public override void SetItemsOnPivotalScan(IList<IDScanItem> items)
        {
            ItemsOnPivotalScan = items;

            var leftChild = CreateNode(PosMapper, LeftBorderScan, items);
            var rightChild = CreateNode(PosMapper, items, RightBorderScan);

            ProcessChild(leftChild);
            ProcessChild(rightChild);
        }

        private static bool IsCluster(IList<IGrouping<long, IDScanItem>> moonGroups)
        {
            return moonGroups.Count == 1 && moonGroups.First().Count() > 1;
        }
    }
}