using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser
{
    //public interface IClipboardParser
    //{
    //    event Action<IList<IProbeScanItem>> ProbeScan;
    //    event Action<IList<IDScanItem>> DirectionalScan;
    //    event Action<IList<String>> Local;
    //}

    public sealed class ClipboardParser : IDisposable
    {
        
        private readonly IClipboardHook _clipboardHook;

        public ClipboardParser(IClipboardHook clipboardHook)
        {
            _clipboardHook = clipboardHook;
            _clipboardHook.NewClipboardAsciiData += ClipboardHookOnNewClipboardAsciiData;
        }

        private void ClipboardHookOnNewClipboardAsciiData(object o, string s)
        {
            //DScan Locator puts scan values into clipboard, do not interpret it as character name
            int intValue;
            if (int.TryParse(s, out intValue))
            {
                return;
            }

            IList<IDScanItem> dScanItems;
            if (DScanParser.TryParseDScan(s, out dScanItems))
            {
                OnDirectionalScan(dScanItems);
                return;
            }

            IList<IProbeScanItem> probeScan;
            if (ProbeScanParser.TryParseProbeScan(s, out probeScan))
            {
                OnProbeScan(probeScan);
                return;
            }

            IList<string> characterNames;
            if (IsCharacterNames(s, out characterNames))
            {
                OnLocal(characterNames);
            }
        }

        public event Action<IList<IProbeScanItem>> ProbeScan;

        private void OnProbeScan(IList<IProbeScanItem> obj)
        {
            var handler = ProbeScan;
            if (handler != null) handler(obj);
        }

        public event Action<IList<IDScanItem>> DirectionalScan;

        private void OnDirectionalScan(IList<IDScanItem> obj)
        {
            var handler = DirectionalScan;
            if (handler != null) handler(obj);
        }


        public event Action<IList<string>> Local;

        private void OnLocal(IList<string> obj)
        {
            var handler = Local;
            if (handler != null) handler(obj);
        }

        public static bool IsCharacterNames(string clipboardText)
        {
            IList<string> charNames;
            return IsCharacterNames(clipboardText, out charNames);
        }

        private static bool IsCharacterNames(string clipboardText, out IList<string> characterNames)
        {
            characterNames = clipboardText.Replace("\r", "").Split('\n');

            return characterNames.All(IsCharacterName);
        }

        private static bool IsCharacterName(string arg)
        {
            //offical info is at least 4 chars long, but appearantly there are shorter names out there
            //names also can only contain once ' ' and one '\''
            if (arg.Length < 3 || arg.Length > 36)
            {
                return false;
            }
            var i = 0;
            foreach (var c in arg)
            {
                if (c == ' ')
                {
                    if (i == 0)
                    {
                        return false;
                    }
                    continue;
                }
                if (c == '\'')
                {
                    if (i == 0)
                    {
                        return false;
                    }
                    continue;
                }
                if (!Char.IsLetterOrDigit(c) && c != '-')
                {
                    return false;
                }
                ++i;
            }
            return true;
        }

        public void Dispose()
        {
            _clipboardHook.NewClipboardAsciiData -= ClipboardHookOnNewClipboardAsciiData;
        }
    }
}
