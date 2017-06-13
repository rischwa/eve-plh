using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities.PosMapper
{
    public class MoonItemModel
    {
        public MoonItemModel()
        {
        }

        public MoonItemModel(MoonItem item)
        {
            ItemsOnScan = item.ItemsOnScan.ToList();
            Id = item.Moon.Name;
            ScanTime = item.ScanTime;
        }
        
        public string Id { get; set; }
        public DateTime ScanTime { get; set; }
        public List<IDScanItem> ItemsOnScan { get; set; }
    }
}