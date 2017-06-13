using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    public abstract class AbstractPosMappingScanNode : IPosMappingScanNode
    {
        protected readonly IList<IDScanItem> LeftBorderScan;
        protected readonly PosMapper2 PosMapper;
        protected readonly IList<IDScanItem> RightBorderScan;

        protected AbstractPosMappingScanNode(PosMapper2 posMapper, IList<IDScanItem> leftBorderScan,
                                             IList<IDScanItem> rightBorderScan)
        {
            PosMapper = posMapper;
            LeftBorderScan = leftBorderScan;
            RightBorderScan = rightBorderScan;

            ItemsOnScan = rightBorderScan.Without(leftBorderScan).ToList();
        }

        public long PivotalScanRangeInKm { get; protected set; }

        public bool IsLeaf { get; protected set; }

        public abstract void SetItemsOnPivotalScan(IList<IDScanItem> items);

        public IList<IDScanItem> ItemsOnScan { get; private set; }

        public virtual IList<MoonItem> GetMoonItems()
        {
            var moons = ItemsOnScan.Where(PosMappingUtils.IsMoon).ToList();

            if (moons.Count == 1)
            {
                return new[] {new MoonItem(ItemsOnScan)};
            }

            return moons.Where(PosMappingUtils.IsMoon).Select(x => new MoonItem(new[] {x})).ToList();
        }

        protected void ProcessChild(IPosMappingScanNode node)
        {
            if (node.IsLeaf)
            {
                var moonItems = node.GetMoonItems();
                if (moonItems.Any())
                {
                    PosMapper.OnMoonsScanned(moonItems);
                }
                return;
            }

            PosMapper.AddNodeToScan(node);
        }
    }
}