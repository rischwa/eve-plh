using System;
using System.Windows;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    /// Interaction logic for Prompt.xaml
    /// </summary>
    public partial class Prompt : Window
    {
        private Prompt()
        {
            InitializeComponent();
            TxtInput.Focus();
        }

        protected override void OnActivated(EventArgs e)
        {
            this.ActivateSoftwareRendering();
        }


        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        
        public static bool TryGetInput(string title, string lable, out string input)
        {
            var prompt = new Prompt {Title = title, LblInput = {Content = lable}};
            prompt.UpdateLayout();

            if (prompt.ShowDialog().GetValueOrDefault())
            {
                input = prompt.TxtInput.Text;
                return true;
            }

            input = null;
            return false;
        }
    }
}
