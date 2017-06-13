using System;
using System.ComponentModel;
using System.Windows;
using EveLocalChatAnalyser.Utilities;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace EveLocalChatAnalyser.Ui
{
    /// <summary>
    ///     Interaction logic for DScanRangeFinder.xaml
    /// </summary>
    public partial class DScanRangeFinder : Window
    {
        public DScanRangeFinder(MainWindow parent)
        {
            InitializeComponent();
            this.SanitizeWindowSizeAndPosition();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.DScanLocatorSize = new Size((int)Width, (int)Height);
            Properties.Settings.Default.DScanLocatorPosition = new Point((int)Left, (int)Top);
            DScanLocatorCtrl.Dispose();
        }

        protected override void OnActivated(EventArgs e)
        {
            DScanLocatorCtrl.Activate();
        }
    }

    public class CannotOpenWindowException : Exception
    {
    }
}