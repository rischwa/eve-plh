using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui.QuickAction
{
    /// <summary>
    /// Interaction logic for QuickActionWindow.xaml
    /// </summary>
    public partial class QuickActionWindow : Window
    {
        private readonly CancellationTokenSource _cancellationTokenScource;
        private Grid _titleBar;

        public QuickActionWindow(FrameworkElement child)
        {
            InitializeComponent();

            Content = child;

            _cancellationTokenScource = new CancellationTokenSource();

            var fiveSeconds = new TimeSpan(0,0,4);
            TaskEx.Delay(fiveSeconds, _cancellationTokenScource.Token)
                .ContinueInDispatcher(Exit, _cancellationTokenScource.Token);

            MouseDown += OnMouseDown;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;

            Activated += OnActivated;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            _cancellationTokenScource.Cancel();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            Unbind();

            Close();
        }

        private void Unbind()
        {
            _cancellationTokenScource.Cancel();

            MouseDown -= OnMouseDown;
            MouseEnter -= OnMouseEnter;
            MouseLeave -= OnMouseLeave;

            Activated -= OnActivated;
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            Activated -= OnActivated;
            _titleBar = (Grid)GetTemplateChild("TitleBar");
            _titleBar.Visibility = Visibility.Collapsed;
            this.PlaceNearCursor();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Unbind();
            _titleBar.Visibility = Visibility.Visible;
        }
    }
}
