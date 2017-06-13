using System.Collections.Generic;
using System.Windows.Controls;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    ///     Interaction logic for KillsTable.xaml
    /// </summary>
    public partial class KillsTable : UserControl
    {
        public KillsTable()
        {
            InitializeComponent();
        }

        public IList<RecentKillModel> Kills
        {
            set { DataKills.ItemsSource = value; }
        }
    }
}