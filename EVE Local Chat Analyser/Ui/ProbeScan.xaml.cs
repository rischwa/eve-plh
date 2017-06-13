using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    ///     Interaction logic for ProbeScan.xaml
    /// </summary>
    public partial class ProbeScan : Window
    {
      
        //TODO per di machen
        private readonly IPositionTracker _positionTracker = DIContainer.GetInstance<IPositionTracker>();

        public ProbeScan(MainWindow window)
        {
            var currentSystemOfActiveCharacter = _positionTracker.CurrentSystemOfActiveCharacter;
            if (currentSystemOfActiveCharacter == null)
            {
                MessageBox.Show(
                                this,
                                "This feature allows you to store previous probe scans, so you don't need to scan signatures again, if you already had them scanned.\n\nYou need to have position tracking enabled for this to work.\nYou can activate it in the settings window.",
                                "WARNING");
                throw new CannotOpenWindowException();
            }

            InitializeComponent();

            ScanningStorage.ScannedSignaturesUpdate += ScanningStorageOnScannedSignaturesUpdate;

            ScanningStorageOnScannedSignaturesUpdate(currentSystemOfActiveCharacter, ScanningStorage.GetScannedSignaturesForSystem(currentSystemOfActiveCharacter));

            _positionTracker.ActiveCharacterSystemChanged += PositionTrackerOnActiveCharacterSystemChanged;

          //  this.SanitizeWindowSizeAndPosition();
        }

        private void PositionTrackerOnActiveCharacterSystemChanged(string character, string newSystem)
        {
            ScanningStorageOnScannedSignaturesUpdate(newSystem, ScanningStorage.GetScannedSignaturesForSystem(newSystem));
        }

        private void ScanningStorageOnScannedSignaturesUpdate(string system, IEnumerable<ScannedSignature> scanItems)
        {
            var scannedSignatures = scanItems as ScannedSignature[] ?? scanItems.ToArray();
            
            var identifiedItems = scannedSignatures.Where(x => !x.IsPotentiallyClosed && !string.IsNullOrEmpty(x.Group)).OrderBy(x=>x.Name).ToArray();
            DataGridIdentified.ItemsSource = identifiedItems;

            var unidentifiedItems =
                scannedSignatures.Where(x => !x.IsPotentiallyClosed && string.IsNullOrEmpty(x.Group)).OrderBy(x => x.Name).ToArray();
            DataGridUnknown.ItemsSource = unidentifiedItems;
        }

      

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.ProbeScanSize = new Size((int) Width, (int) Height);
            Properties.Settings.Default.ProbeScanPosition = new Point((int) Left, (int) Top);
            ScanningStorage.ScannedSignaturesUpdate -= ScanningStorageOnScannedSignaturesUpdate;
            _positionTracker.ActiveCharacterSystemChanged -= PositionTrackerOnActiveCharacterSystemChanged;
        }

        private void BtnHelp_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                            this,
                            "This feature allows you to store previous probe scans, so you don't need to scan signatures again.\n\nIf you have position tracking active, everytime you copy the probe scanner result list to your clipboard, all your scanned signatures even without 100% strength get stored.\n\nIf you later open this window and copy the probe scan list again,you can see the group/type of your previously scanned sigantures, without needing to scan them down again.",
                            "Help");
        }


        private class ScannedItemVM : ScannedSignature
        {
            public ScannedItemVM(ScannedSignature scannedSignature)
            {
                Name = scannedSignature.Name;
                Type = scannedSignature.Type;
                Group = scannedSignature.Group;
                ScanTime = scannedSignature.ScanTime;
                ScanTimeStr = ScanTime.ToString("yyyy-MM-dd HH:mm");
            }

            public new string ScanTimeStr { get; private set; }
        }
    }
}
