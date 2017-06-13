using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;

namespace EveLocalChatAnalyser.Ui.Wormholes
{
    /// <summary>
    ///     Interaction logic for WormholeConnectionEditDialog.xaml
    /// </summary>
    public partial class WormholeConnectionEditDialog : Window
    {
        private readonly bool _isShowingFirstSystemOnTop;
        private readonly WormholeConnection _whConnection;
        private readonly IWormholeConnectionTracker _whConnectionTracker = DIContainer.GetInstance<IWormholeConnectionTracker>();

        public WormholeConnectionEditDialog(WormholeConnection whConnection, bool isShowingFirstSystemOnTop = true)
        {
            _whConnection = whConnection;
            _isShowingFirstSystemOnTop = isShowingFirstSystemOnTop;
            DataContext = _whConnection;
            InitializeComponent();
            var cboFirstSystem = isShowingFirstSystemOnTop ? CboOldSystem : CboNewSystem;
            var cboSecondSystem = isShowingFirstSystemOnTop ? CboNewSystem : CboOldSystem;

            var firstSystem = isShowingFirstSystemOnTop ? whConnection.FirstSystem : whConnection.SecondSystem;
            var secondSystem = isShowingFirstSystemOnTop ? whConnection.SecondSystem : whConnection.FirstSystem;

            var firstToSecond = isShowingFirstSystemOnTop ? whConnection.FirstToSecondSignature : whConnection.SecondToFirstSignature;
            var secondToFirst = isShowingFirstSystemOnTop ? whConnection.SecondToFirstSignature : whConnection.FirstToSecondSignature;

            TxtOldSystem.Text = firstSystem;
            TxtNewSystem.Text = secondSystem;

            ScanningStorage.ScannedSignaturesUpdate += ScanningStorageOnScannedSignaturesUpdate;

            InitSignatureComboBox(firstSystem, cboFirstSystem, firstToSecond);

            InitSignatureComboBox(secondSystem, cboSecondSystem, secondToFirst);
        }

        private static void InitSignatureComboBox(string systemName, ComboBox cboSignature, string signatureName)
        {
            var scannedSignaturesFirstSystem = ScanningStorage.GetScannedSignaturesForSystem(systemName)
                                                              .Where(x => x.IsUnknown() || x.IsWormhole())
                                                              .ToArray();
            cboSignature.ItemsSource = scannedSignaturesFirstSystem;

            if (signatureName == null)
            {
                return;
            }

            var signature = scannedSignaturesFirstSystem.FirstOrDefault(x => x.Name == signatureName);
            if (signature == null)
            {
                cboSignature.Text = signatureName;
            }
            else
            {
                cboSignature.SelectedItem = signature;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ScanningStorage.ScannedSignaturesUpdate -= ScanningStorageOnScannedSignaturesUpdate;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            _whConnection.FirstToSecondSignature = _isShowingFirstSystemOnTop ? CboOldSystem.Text : CboNewSystem.Text;
            _whConnection.SecondToFirstSignature = _isShowingFirstSystemOnTop ? CboNewSystem.Text : CboOldSystem.Text;

            _whConnection.LastLifetimeUpdate.Time = DateTime.UtcNow;

            _whConnectionTracker.UpdateWormholeConnection(_whConnection);

            Close();
        }

        private void ScanningStorageOnScannedSignaturesUpdate(string system, IEnumerable<ScannedSignature> scanItems)
        {
            var scannedSignatures = scanItems.ToArray();
            ComboBox cbo;
            if (system == _whConnection.FirstSystem)
            {
                cbo = _isShowingFirstSystemOnTop ? CboOldSystem : CboNewSystem;
            }
            else
            {
                if (system == _whConnection.SecondSystem)
                {
                    cbo = _isShowingFirstSystemOnTop ? CboNewSystem : CboOldSystem;
                }
                else
                {
                    return;
                }
            }

            var selectedItem = cbo.SelectedItem as ScannedSignature;

            cbo.ItemsSource = scannedSignatures.Where(x => x.IsUnknown() || x.IsWormhole()).ToArray();

            if (selectedItem != null)
            {
                cbo.SelectedItem = scannedSignatures.FirstOrDefault(x => x.Name == selectedItem.Name);
            }
        }

        private void BtnDeleteConnection_OnClick(object sender, RoutedEventArgs e)
        {
            _whConnectionTracker.CloseWormholeConnection(_whConnection);
            Close();
        }
    }
}
