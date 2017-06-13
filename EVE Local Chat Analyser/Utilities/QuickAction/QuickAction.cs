using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EveLocalChatAnalyser.Ui;
using EveLocalChatAnalyser.Ui.Map;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Ui.QuickAction;

namespace EveLocalChatAnalyser.Utilities.QuickAction
{
    public enum ClipboardContent
    {
        None = 0,
        Local,
        DScan,
        ProbeScan
    }

    public class QuickAction : IQuickAction
    {
        private ClipboardContent _lastAction;
        private IList<IDScanItem> _lastDScan;
        private List<IEveCharacter> _lastLocal;

        public QuickAction(ClipboardParser clipboardParser, LocalChatAnalyser analyser)
        {
            clipboardParser.DirectionalScan += ClipboardParserOnDirectionalScan;
            clipboardParser.ProbeScan += ClipboardParserOnProbeScan;
            analyser.UpdateCharacters += UpdateCharacters;
        }

        public void Run()
        {
            switch (_lastAction)
            {
                case ClipboardContent.ProbeScan:
                    ((MainWindow) Application.Current.MainWindow)._probeScan.Show();
                    break;

                case ClipboardContent.Local:
                    ShowLocalWindow();
                    break;

                case ClipboardContent.DScan:
                    ShowDScanWindow();
                    break;
            }
        }

        private void UpdateCharacters(List<IEveCharacter> characters)
        {
            _lastLocal = characters;
            _lastAction = ClipboardContent.Local;
        }

        private void ShowDScanWindow()
        {
            var dscanControl = new DScanShipAggregator();
            dscanControl.SetScan(_lastDScan);

            CreateQuickActionWindow(dscanControl, "DScan");
        }

        private void ShowLocalWindow()
        {
            var localTableControl = new LocalTableControl();

            var stats = new EveLocalStatistics();
            stats.UpdateLocalStatistics(_lastLocal);
            localTableControl.Characters = _lastLocal.Select(x => new EveCharacterViewModel(x, stats))
                .ToList();

            CreateQuickActionWindow(localTableControl, "Local");
        }

        private static void CreateQuickActionWindow(FrameworkElement content, string title)
        {
            var quickActionWindow = new QuickActionWindow(content)
                                    {
                                        Title = title
                                    };
            quickActionWindow.Show();
        }

        private void ClipboardParserOnProbeScan(IList<IProbeScanItem> probeScanItems)
        {
            _lastAction = ClipboardContent.ProbeScan;
        }

        private void ClipboardParserOnDirectionalScan(IList<IDScanItem> dScanItems)
        {
            _lastDScan = dScanItems;
            _lastAction = ClipboardContent.DScan;
        }
    }
}
