using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    ///     Interaction logic for MessageOfTheDayDialog.xaml
    /// </summary>
    public partial class MessageOfTheDayDialog : Window
    {
        private int _number;

        public MessageOfTheDayDialog()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
                      //otherwise window content is pure white (controls are there)
                      //after update to windows 8.1 -> problem with .net 4.5.1???
                      //problem disappeared, maybe driver update ...
                      this.ActivateSoftwareRendering();
        }

        public MessageOfTheDay MessageOfTheDay
        {
            set
            {
                _number = value.MessageNumber;
                TextBox.Selection.Load(new MemoryStream(Encoding.Default.GetBytes(value.Text)), DataFormats.Rtf);
            }
        }

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hyperlink = (Hyperlink) sender;
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DontShowAgainButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MessageOfTheDayNumber = _number;
            Properties.Settings.Default.Save();

            Close();
        }
    }
}