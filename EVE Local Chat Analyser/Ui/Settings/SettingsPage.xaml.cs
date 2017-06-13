#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using EveLocalChatAnalyser.Exceptions;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Ui.Map.Statistics;
using EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators;
using EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators;
using EveLocalChatAnalyser.Utilities.PositionTracking;

#endregion

namespace EveLocalChatAnalyser.Ui.Settings
{
    /// <summary>
    ///     Interaction logic for Page1.xaml
    /// </summary>
    public partial class SettingsPage : Window
    {
        private static readonly SolidColorBrush NON_DEFAULT_PROFILE_BACKGROUND =
            new SolidColorBrush(Colors.LightGoldenrodYellow);

        private static readonly SolidColorBrush DEFAULT_PROFILE_BACKGROUND = new SolidColorBrush(Colors.LightCyan);
        private bool _wasOk;

        public SettingsPage()
        {
            _wasOk = false;

            InitMapViewModels();
            InitializeComponent();

            Closing += OnClosing;

            LoadSettings();
        }

        private void InitMapViewModels()
        {

            SystemMinValueExampleModel = new SystemExampleModel(0);
            SystemMidValueExampleModel = new SystemExampleModel(3);
            SystemMaxValueExampleModel = new SystemExampleModel(6);
        }

        public SystemExampleModel SystemMaxValueExampleModel { get; set; }

        public SystemExampleModel SystemMidValueExampleModel { get; set; }

        public SystemExampleModel SystemMinValueExampleModel { get; set; }
        
        

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {

            SystemMinValueExampleModel.Dispose();
            SystemMidValueExampleModel.Dispose();
            SystemMaxValueExampleModel.Dispose();

            if (!_wasOk)
            {
                //TODO das hier ist doch kaese, oder?
                var standings = ActiveProfile.Default.Standings;

                ActiveProfile.Default.Reload();
                Properties.Settings.Default.Reload();
                ActiveProfile.Default.Standings = standings;
            }
        }

        private void LoadSettings()
        {
            InitProfiles();

            sliderFontSize.Value = Properties.Settings.Default.FontSize;
            sliderFontSize.ValueChanged += SliderFontSizeOnValueChanged;
            
            InitExternalServiceSelection();

            DockProfileCharacters.Visibility = Properties.Settings.Default.ActiveProfile ==
                                               Properties.Settings.DEFAULT_PROFILE_NAME
                                                   ? Visibility.Collapsed
                                                   : Visibility.Visible;

            InitShortcuts();
        }

        private void InitShortcuts()
        {
            ShortcutEditorMinimizeAll.DefaultValue = Properties.Settings.Default.ShortcutToggleMinimizeAll;
        }

        private void InitProfiles()
        {
            CboProfile.Items.Clear();

            CboProfile.ItemsSource = null;
            foreach (
                var curProfile in
                    Properties.Settings.Default.Profiles.OrderBy(x => x.Name,
                                                                 new ProfileComparerWithDefaultProfileFirst()))
            {
                CboProfile.Items.Add(curProfile);
            }

            CboProfile.SelectedItem =
                CboProfile.Items.Cast<Profile>()
                          .FirstOrDefault(x => x.Name == Properties.Settings.Default.ActiveProfile);
        }

        private static void SliderFontSizeOnValueChanged(object sender,
                                                         RoutedPropertyChangedEventArgs<double>
                                                             routedPropertyChangedEventArgs)
        {
            Properties.Settings.Default.FontSize = routedPropertyChangedEventArgs.NewValue;
        }


        private void InitExternalServiceSelection()
        {
            ExternalServiceType externalServiceType;
            var isValid = Enum.TryParse(Properties.Settings.Default.ExternalService, out externalServiceType);
            if (!isValid)
            {
                RdoZKillboard.IsChecked = true;
                return;
            }

            switch (externalServiceType)
            {
                case ExternalServiceType.EveKill:
                    RdoEveKill.IsChecked = true;
                    break;
                default:
                    RdoZKillboard.IsChecked = true;
                    break;
            }
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            ActiveProfile.Default.IsShowingFaction = ShowFaction.IsChecked.GetValueOrDefault();
            SaveExternalServiceSettings();

            SaveUISettings();
            Properties.Settings.Default.Save();

            try
            {
                CheckStandingInformation();
                SaveStandingInformation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(
                    "If you want to utilize standing information, you have to supply a valid API key id and verification code: {0}",
                    ex.Message));
            }


            Properties.Settings.Default.UpdateProfiles();

            ActiveProfile.Default.Save();
            Properties.Settings.Default.Save();

            _wasOk = true;
            Close();
        }

        private void SaveUISettings()
        {
            Properties.Settings.Default.FontSize = sliderFontSize.Value;
        }

        private void SaveExternalServiceSettings()
        {
            var isEveKill = RdoEveKill.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.ExternalService = isEveKill
                                                              ? ExternalServiceType.EveKill.ToString()
                                                              : ExternalServiceType.ZKillboard.ToString();
        }

        private void SaveStandingInformation()
        {
            ActiveProfile.Default.IsHidingBlues = HideBlues.IsChecked.GetValueOrDefault();

            EnsureUpdatedStandings();
        }

        private void CheckStandingInformation()
        {
            var isShowingBlues = !HideBlues.IsChecked.GetValueOrDefault();
            if (isShowingBlues)
            {
                return;
            }

            ActiveProfile.Default.CharacterId = ApiKeyInfoService.GetCharacterId(TxtKeyId.Text, TxtVCode.Text);
        }

        private void DrawBackgroundDependingOnStatus_Click(object sender, RoutedEventArgs e)
        {
            if (!DrawBackgroundDependingOnStatus.IsChecked.GetValueOrDefault())
            {
                ShowExited.IsChecked = false;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }

        private void BtnCopyTrackingUrlToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(WebServer.Url);
        }

        private void BtnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            string profileName;
            if (!Prompt.TryGetInput("Create new profile", "Profile name:", out profileName))
            {
                return;
            }

            var profiles = Properties.Settings.Default.Profiles.ToList();
            if (profiles.Any(x => x.Name == profileName))
            {
                MessageBox.Show(string.Format("A profile named '{0}' already exists, please select another name",
                                              profileName));
                BtnNewProfile_Click(sender, e);
                return;
            }

            EnsureUpdatedStandings();

            Properties.Settings.Default.UpdateProfiles();

            Properties.Settings.Default.CreateProfile(profileName);
            ActivateProfile(profileName);
        }

        private static void EnsureUpdatedStandings()
        {
            //var activeProfile = profiles.First(x => x.Name == Properties.Settings.Default.ActiveProfile);

            try
            {
                Properties.Settings.Default.EnsureUpdatedStandings();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    string.Format(
                        "Could not update standings for profile {0}, please make sure, your API info is correct:\n{1}",
                        Properties.Settings.Default.ActiveProfile, e.Message), "ERROR", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ActivateProfile(string profileName)
        {
            Properties.Settings.Default.ActivateProfile(profileName);
            InitProfiles();
            CboProfile.SelectedItem = CboProfile.Items.Cast<Profile>().First(x => x.Name == profileName);
        }

        private void CboProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            EnsureUpdatedStandings();

            Properties.Settings.Default.UpdateProfiles();
            var profile = e.AddedItems.Cast<Profile>().First();

            DockProfileCharacters.Visibility = profile.Name == Properties.Settings.DEFAULT_PROFILE_NAME
                                                   ? Visibility.Collapsed
                                                   : Visibility.Visible;

            Properties.Settings.Default.ActivateProfile(profile.Name);
            var isDefaultProfile = profile.Name == Properties.Settings.DEFAULT_PROFILE_NAME;
            BtnRemoveProfile.IsEnabled = !isDefaultProfile;

            CboProfile.Background = isDefaultProfile ? DEFAULT_PROFILE_BACKGROUND : NON_DEFAULT_PROFILE_BACKGROUND;
        }

        private void BtnRemoveProfile_Click(object sender, RoutedEventArgs e)
        {
            var profileName = ((Profile) CboProfile.SelectedItem).Name;

            Properties.Settings.Default.RemoveProfile(profileName);

            ActivateProfile(Properties.Settings.DEFAULT_PROFILE_NAME);
        }

        private void BtnEditCoalitions_Click(object sender, RoutedEventArgs e)
        {
            var oldCoalitions = Properties.Settings.Default.CoalitionsJson;

            var settings = new CoalitionsSettings();
            if (!settings.ShowDialog().GetValueOrDefault())
            {
                Properties.Settings.Default.CoalitionsJson = oldCoalitions;
            }
        }

        private void BtnSelectPositionTrackingCharacters_Click(object sender, RoutedEventArgs e)
        {
            new CharacterSelector().ShowDialog();
        }

        private class ProfileComparerWithDefaultProfileFirst : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == Properties.Settings.DEFAULT_PROFILE_NAME)
                {
                    return y == Properties.Settings.DEFAULT_PROFILE_NAME ? 0 : -1;
                }

                return y == Properties.Settings.DEFAULT_PROFILE_NAME
                           ? 1
                           : String.Compare(x, y, StringComparison.Ordinal);
            }
        }
    }
}