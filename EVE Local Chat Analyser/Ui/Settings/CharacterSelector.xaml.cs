using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using log4net;

namespace EveLocalChatAnalyser.Ui.Settings
{

    /// <summary>
    /// Interaction logic for CharacterSelector.xaml
    /// </summary>
    public partial class CharacterSelector : Window
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static readonly ILog LOGGER = LogManager.GetLogger("Character selection");
        public CharacterSelector()
        {
            InitializeComponent();
            
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            TaskEx.Run(() =>
            {
                try
                {
                    var tracking = DIContainer.GetInstance<LogBasedPositionTracking>();
                    return tracking.GetCharacterNames().OrderBy(x => x).ToArray();
                }
                catch (Exception e)
                {
                    LOGGER.Error("could not load character names", e);
                    Application.Current.Dispatcher.Invoke(new Func<MessageBoxResult>(() => MessageBox.Show("Could not load character names: " + e.Message, "ERROR")));
                    return new string[0];
                }
            }, _cancellationTokenSource.Token).ContinueWith(chars =>
                {
                    var selectedChars = Properties.Settings.Default.PositionTrackingCharacters ?? new StringCollection();
                    LstCharacters.ItemsSource = chars.Result.Select(x => new CheckBox {Content = x, IsChecked = selectedChars.Contains(x)}).ToList();
                BusyLoading.IsBusy = false;
            }, _cancellationTokenSource.Token, TaskContinuationOptions.None, scheduler);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            var chars = LstCharacters.Items.Cast<CheckBox>()
                         .Where(x => x.IsChecked.GetValueOrDefault())
                         .Select(x => x.Content).Cast<string>()
                         .ToArray();

            var selectedCharacters = new StringCollection();
            selectedCharacters.AddRange(chars);

            Properties.Settings.Default.PositionTrackingCharacters = selectedCharacters;

            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _cancellationTokenSource.Cancel();
        }
    }
}
