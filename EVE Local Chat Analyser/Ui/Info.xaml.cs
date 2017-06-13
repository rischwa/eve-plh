using System.IO;
using System.Windows;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    ///     Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : Window
    {
        private const string LOG_FILENAME = "log\\errorlog.txt";

        public Info()
        {
            InitializeComponent();
        }

        private void BtnViewErrorLog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TextBoxDialog {Title = "Error Log"};

            var file = new FileInfo(LOG_FILENAME);
            if (file.Exists)
            {
                var text = File.ReadAllText(LOG_FILENAME);
                dialog.TextBox.Text = text;
            }
            else
            {
                dialog.TextBox.Text = "No errors logged";
            }

            dialog.ShowDialog();
        }
    }
}