using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities
{
    public static class DScanParser
    {
        public static bool TryParseDScan(string dscanFromClipboard, out IList<IDScanItem> items)
        {
            if (String.IsNullOrWhiteSpace(dscanFromClipboard))
            {
                items = null;
                return false;
            }

            //TODO andere sprachen
            if (dscanFromClipboard == "No Result from Directional Scan" || dscanFromClipboard == "Richtungsscan brachte kein Ergebnis")
            {
                items = new List<IDScanItem>();
                return true;
            }
            string splitString = dscanFromClipboard.Contains("\r\n") ? "\r\n" : "\n";
            var lines = dscanFromClipboard.Split(new[] {splitString}, StringSplitOptions.None);
            try
            {
                items = lines.Select(s => new DScanItem(s)).Cast<IDScanItem>().ToList();
                return true;
            }
            catch (Exception)
            {
                items = null;
                return false;
            }
        }
    }
}