using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui.Settings
{
    public delegate void ShortcutChanged(string name, KeyPress keyPress);

    /// <summary>
    ///     Interaction logic for ShortcutEditor.xaml
    /// </summary>
    public partial class ShortcutEditor : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SHORTCUT_TEXT_PROPERTY =
            DependencyProperty.Register("ShortcutText", typeof (string), typeof (ShortcutEditor),
                                        new PropertyMetadata(default(string)));

        public static readonly DependencyProperty LABEL_TEXT_PROPERTY =
            DependencyProperty.Register("LabelText", typeof (string), typeof (ShortcutEditor),
                                        new PropertyMetadata(default(string)));

        private bool _isValid;

        public ShortcutEditor()
        {
            InitializeComponent();
        }

        public string DefaultValue { get; set; }

        public string ShortcutText
        {
            get { return (string) GetValue(SHORTCUT_TEXT_PROPERTY); }
            set { SetValue(SHORTCUT_TEXT_PROPERTY, value); }
        }

        public string LabelText
        {
            get { return (string) GetValue(LABEL_TEXT_PROPERTY); }
            set { SetValue(LABEL_TEXT_PROPERTY, value); }
        }

        private bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (value.Equals(_isValid))
                {
                    return;
                }

                TxtShortcut.Foreground = value ? Brushes.SolidBlackBrush : Brushes.SolidRedBrush;
                TxtShortcut.ToolTip = null;

                _isValid = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            TxtShortcut.Text = "";
            IsValid = true;
        }


        public void SetError(string message)
        {
            IsValid = false;
            TxtShortcut.ToolTip = message;
        }

        private void TxtShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            var text = "";
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows))
            {
                text += "Win+";
            }
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                text += "Alt+";
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                text += "Ctrl+";
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                text += "Shift+";
            }

            var isValid = (text != "" || ((int) e.Key >= (int) Key.F1 && (int) e.Key <= (int) Key.F12)) &&
                          !e.Key.IsModifier();

            if (!e.Key.IsModifier())
            {
                text += e.Key;
            }

            if (isValid)
            {
                IsValid = true;
                ShortcutText = text;
            }
            else
            {
                TxtShortcut.Text = text;
                SetError("Hotkey has to contain a modifier (shift, ctrl, alt) or a F1-F12 key");
            }

            e.Handled = true;
        }

        private void TxtShortcutPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void TxtShortcut_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None && !IsValid)
            {
                Clear();
            }
        }

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            IsValid = true;
            ShortcutText = DefaultValue;
        }
    }
}