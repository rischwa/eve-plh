using System;
using System.Collections.Generic;
using System.Linq;

namespace EveLocalChatAnalyser.Utilities
{
    public static class ProbeScanParser
    {
        public static bool TryParseProbeScan(string probeScanFromClipboard, out IList<IProbeScanItem> items)
        {
            if (String.IsNullOrWhiteSpace(probeScanFromClipboard))
            {
                items = null;
                return false;
            }
            var splitString = probeScanFromClipboard.Contains("\r\n") ? "\r\n" : "\n";
            var lines = probeScanFromClipboard.Split(new[] { splitString }, StringSplitOptions.None);
            try
            {
                items = lines.Select(s => new ProbeScanItem(s)).Cast<IProbeScanItem>().ToList();
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