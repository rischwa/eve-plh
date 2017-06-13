using System.Collections.Generic;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    public interface IPosMappingScanNode
    {
        long PivotalScanRangeInKm { get; }
        bool IsLeaf { get; }
        IList<IDScanItem> ItemsOnScan { get; }
        void SetItemsOnPivotalScan(IList<IDScanItem> items);
        IList<MoonItem> GetMoonItems();
    }
}