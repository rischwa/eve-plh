using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;
using Microsoft.Win32;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    ///     Interaction logic for LocalTableControl.xaml
    /// </summary>
    public partial class LocalTableControl : UserControl
    {
        public LocalTableControl()
        {
            InitializeComponent();
            Characters = new List<EveCharacterViewModel>();
        }

        public ICollection<EveCharacterViewModel> Characters
        {
            get { return (ICollection<EveCharacterViewModel>) DataMain.ItemsSource; }
            set { DataMain.ItemsSource = value; }
        }

        private void lstMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataMain.SelectedIndex = -1;
        }

        private void lstMain_MouseMove(object sender, MouseEventArgs e)
        {
            var eveChar = GetEveCharacter(e);
            if (eveChar == null)
            {
                ResetHighlighting();
                return;
            }

            SetHighlighting(eveChar);
        }

        private static EveCharacterViewModel GetEveCharacter(RoutedEventArgs e)
        {
            var element = (FrameworkElement) e.OriginalSource;
            return element.DataContext as EveCharacterViewModel;
        }

        private void SetHighlighting(EveCharacterViewModel eveChar)
        {
            eveChar.IsHighlighted = true;
            for (var i = 0; i < DataMain.Items.Count; ++i)
            {
                var curChar = (EveCharacterViewModel) DataMain.Items[i];
                SetHighlighting(eveChar, curChar);
            }
        }

        private static void SetHighlighting(EveCharacterViewModel eveChar, EveCharacterViewModel curChar)
        {
            curChar.IsHighlighted = AreConnected(eveChar.EveCharacter, curChar.EveCharacter);
        }

        private static bool AreConnected(IEveCharacter eveChar, IEveCharacter curChar)
        {
            if (Properties.Settings.Default.IsShowingCoalitionsColumn && eveChar.Coalitions.Any(c => curChar.Coalitions.Contains(c)))
            {
                return true;
            }

            return (!string.IsNullOrEmpty(eveChar.Alliance)
                        ? eveChar.Alliance == curChar.Alliance
                        : !string.IsNullOrEmpty(eveChar.Corporation) && curChar.Corporation == eveChar.Corporation)
                   || IsAssociatedTo(eveChar, curChar) || IsAssociatedTo(curChar, eveChar);
        }

        private static bool IsAssociatedTo(IEveCharacter eveChar, IEveCharacter curChar)
        {
            return eveChar.KillboardInformation != null
                   && (eveChar.KillboardInformation.AssociatedAlliances.Contains(curChar.Alliance)
                       || eveChar.KillboardInformation.AssociatedCorporations.Contains(curChar.Corporation));
        }

        private void ResetHighlighting()
        {
            foreach (var curChar in Characters)
            {
                curChar.IsHighlighted = false;
            }
        }

        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var curChar = ((DataGridRow) sender).Item as EveCharacterViewModel;
            if (curChar == null)
            {
                return;
            }

            var service = DIContainer.GetInstance<IExternalKillboardService>();
            service.OpenForCharacter(curChar.EveCharacter);
        }

        private void ReloadKillboardInformation_Clicked(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;
            var item = (DataGrid) contextMenu.PlacementTarget;
            foreach (var curChar in item.SelectedCells.Reverse().Select(x => x.Item)
                .Cast<EveCharacterViewModel>())
            {
                curChar.EveCharacter.KillboardInformation = null;
                KillboardAnalysisService.AddFirst(curChar.EveCharacter);
            }
        }

        private void AddOwnTag_Clicked(object sender, RoutedEventArgs e)
        {
            
        }

        private void RemoveOwnTag_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void SetCustomIcon_Clicked(object sender, RoutedEventArgs e)
        {
            EveCharacterViewModel viewModel;
            if (!TryGetFirstSelectedCharacter(sender, out viewModel))
            {
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter =
                                                         "Image Files (*.jpg,*.jpeg,*.png,*.gif)|*.jpg;*.jpeg;*.png;*.gif"
            };


            var result = dlg.ShowDialog();

            if (result != true)
            {
                return;
            }

            var filename = dlg.FileName;

            viewModel.CustomCharacterInfo.IconImage = filename;

        }

        //public bool HasOwnIcon => !DataMain.SelectedCells.Any() && !string.IsNullOrEmpty(((EveCharacterViewModel)DataMain.SelectedCells[0].Item).CustomCharacterInfo.IconImage);

        private void RemoveCustomIcon_Clicked(object sender, RoutedEventArgs e)
        {
            EveCharacterViewModel viewModel;
            if (!TryGetFirstSelectedCharacter(sender, out viewModel))
            {
                return;
            }
            
            viewModel.CustomCharacterInfo.IconImage = null;
        }

        private static bool TryGetFirstSelectedCharacter(object sender, out EveCharacterViewModel viewModel)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;
            var item = (DataGrid) contextMenu.PlacementTarget;
            if (!item.SelectedCells.Any())
            {
                viewModel = null;
                return false;
            }
            viewModel = (EveCharacterViewModel) item.SelectedCells[0].Item;
            return true;
        }
    }
}
