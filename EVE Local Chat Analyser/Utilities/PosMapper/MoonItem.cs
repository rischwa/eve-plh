using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    public class MoonItem
    {
        public MoonItem(IList<IDScanItem> itemsOnScan)
        {
            ItemsOnScan = itemsOnScan;
            Moon = ItemsOnScan.First(PosMappingUtils.IsMoon);
            Tower = ItemsOnScan.FirstOrDefault(PosMappingUtils.IsTower);
            ScanTime = DateTime.UtcNow;
        }

        public DateTime ScanTime { get; set; }

        public IList<IDScanItem> ItemsOnScan { get; private set; }

        public IDScanItem Moon { get; private set; }

        public IDScanItem Tower { get; private set; }
    }
}