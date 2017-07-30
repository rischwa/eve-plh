using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    ///     Interaction logic for News.xaml
    /// </summary>
    public partial class News : Window
    {

        public const int CURRENT_NEWS_VERSION = 91;
        public News()
        {
            InitializeComponent();
            
            //Loaded += (sender, args) => 
            //this.ActivateSoftwareRendering();
        }

        private void BtnDoNotShowAgain_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.NewsVersionToHide = CURRENT_NEWS_VERSION;
            Properties.Settings.Default.Save();
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    

        private void Hyperlink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var x = sender as Hyperlink;
            if (x == null)
            {
                return;
            }
            Process.Start(new ProcessStartInfo(x.NavigateUri.AbsoluteUri));
            e.Handled = true;
        }

    }
}