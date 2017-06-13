using System;
using System.ComponentModel;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;

namespace EveLocalChatAnalyser.Ui
{
    public class WindowInstanceManager<T> : IDisposable where T : Window
    {
        private readonly MainWindow _mainWindow;
        private T _instance;
        private readonly Func<MainWindow, T> _ctorCaller;

        public WindowInstanceManager(MainWindow mainWindow)
        {
            //TODO diesen ganzen scheiss mit services und DI machen
            _mainWindow = mainWindow;

            mainWindow.Closing += MainWindowOnClosing;
            var constructorInfo = typeof(T).GetConstructor(new[] { typeof(MainWindow) });

            var parameterExpression = Expression.Parameter(typeof(MainWindow), "arg1");
            var creationExpression = Expression.New(constructorInfo, parameterExpression);
            _ctorCaller =
                Expression.Lambda<Func<MainWindow, T>>(creationExpression, parameterExpression).Compile();
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Close();
        }

        public void Show()
        {
            if (_instance != null)
            {
                _instance.WindowState = WindowState.Normal;
                _instance.Show();
            }
            else
            {
                try
                {
                    _instance = _ctorCaller(_mainWindow);
                }
                catch (CannotOpenWindowException)
                {
                    _instance = null;
                    return;
                }
                _instance.Show();
                _instance.Closed += WindowClosed;
            }
        }

        public bool IsOpen
        {
            get { return _instance != null; }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            _instance.Closed -= WindowClosed;
            _instance = null;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (_instance != null)
            {
                _instance.Close();
            }
        }
    }
}