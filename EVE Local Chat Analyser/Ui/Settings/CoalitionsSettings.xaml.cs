using System.ComponentModel;
using System.Linq;
using System.Windows;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui.Settings
{
    /// <summary>
    /// Interaction logic for CoalitionsSettings.xaml
    /// </summary>
    public partial class CoalitionsSettings : Window
    {
        public CoalitionsSettings()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
                      //otherwise window content is pure white (controls are there)
                      //after update to windows 8.1 -> problem with .net 4.5.1???
                      this.ActivateSoftwareRendering();

            Properties.Settings.Default.PropertyChanged += SettingsChanged;

            InitCoalitions();
        }

        private static readonly string COALITIONS_JSON_SETTING =
            NotifyUtils.GetPropertyName((Properties.Settings x) => x.CoalitionsJson);

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == COALITIONS_JSON_SETTING)
            {
                InitCoalitions();
            }
        }

        private void InitCoalitions()
        {
            LstCoalitions.ItemsSource = Properties.Settings.Default.Coalitions;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Coalition coalition;
            if (!EditCoalitionDialog.TryCreateCoalition(out coalition))
            {
                return;
            }

            var existingCoalitions = Properties.Settings.Default.Coalitions;
            if (existingCoalitions.Any(x => x.Name == coalition.Name))
            {
                MessageBox.Show(
                    string.Format("A coalition named \"{0}\" already exists, please choose another name", coalition.Name),
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                BtnAdd_Click(sender, e);
                return;
            }

            Properties.Settings.Default.Coalitions = existingCoalitions.Concat(new[] {coalition}).ToList();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = (Coalition)LstCoalitions.SelectedItem;
            if (item == null)
            {
                return;
            }

            Coalition result;
            if (!EditCoalitionDialog.TryEditCoalition(item, out result))
            {
                return;
            }

            var existingCoalitions = Properties.Settings.Default.Coalitions.Where(x=>x.Name != item.Name).ToList();

            if (existingCoalitions.Any(x => x.Name == result.Name))
            {
                MessageBox.Show(
                    string.Format("Another coalition named \"{0}\" already exists, please choose another name", result.Name),
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                BtnEdit_Click(sender, e);
                return;
            }

            Properties.Settings.Default.Coalitions = existingCoalitions.Concat(new[] { result }).ToList();

        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var item = (Coalition)LstCoalitions.SelectedItem;
            if (item == null)
            {
                return;
            }

            var newCoalitions = Properties.Settings.Default.Coalitions.Where(x => x.Name != item.Name).ToList();
            Properties.Settings.Default.Coalitions = newCoalitions;
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                                         this,
                                         "Do you want to reset the coalition definition to its default value?\nAll custom changes are lost!",
                                         "Reset?",
                                         MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            Properties.Settings.Default.CoalitionsJson = (string)Properties.Settings.Default.Properties["CoalitionsJson"].DefaultValue;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
