using System;
using System.ComponentModel;
using System.Windows;
using EveLocalChatAnalyser.Utilities;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace EveLocalChatAnalyser.Ui.PosMapper
{
    public partial class PosMapperDialog : Window
    {
        public PosMapperDialog(MainWindow parent)
        {
            InitializeComponent();
            this.SanitizeWindowSizeAndPosition();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            MyPosMapperControl.Activate();
        }
       
        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.POSMapperSize = new Size((int)Width, (int)Height);
            Properties.Settings.Default.POSMapperPosition = new Point((int)Left, (int)Top);

            MyPosMapperControl.Dispose(); 

            base.OnClosing(e);
        }
    }
}