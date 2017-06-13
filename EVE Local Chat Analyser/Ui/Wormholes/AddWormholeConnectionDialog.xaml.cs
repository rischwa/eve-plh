using System.Windows;
using EveLocalChatAnalyser.Model;

namespace EveLocalChatAnalyser.Ui.Wormholes
{
    /// <summary>
    /// Interaction logic for AddWormholeConnectionDialog.xaml
    /// </summary>
    public partial class AddWormholeConnectionDialog : Window
    {
        public AddWormholeConnectionDialog()
        {
            InitializeComponent();
            var systems = UniverseDataDB.AllSystemNames;

            CboSystem.ItemsSource = systems;
        }

        public string SelectedSystem
        {
            get { return CboSystem.SelectedItem as string; }
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
