using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using EveLocalChatAnalyser.Utilities.Win32;

namespace EveLocalChatAnalyser.Utilities
{
    public delegate void ShortcutPressed(string name);

    public delegate void MessageReceived(Message msg);

    public class HotkeyRegistration
    {
        public readonly int Id;
        public readonly string Name;
        public ShortcutPressed Delegate;

        public HotkeyRegistration(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void InvokeDelegate()
        {
            if (Delegate != null)
            {
                Delegate(Name);
            }
        }
    }

    public interface IKeyboardHook
    {
        void RegisterGlobalShortcut(string name, ModifierKeys modifier, Keys key);
        void UnregisterGlobalShortcut(string name);

        ShortcutPressed this[string name] { get; set; }
    }

    public sealed class KeyboardHook : IKeyboardHook
    {
        private const int WM_HOTKEY = 0x0312;

        private readonly Dictionary<KeyPress, HotkeyRegistration> _shortcutsByKeyPress =
            new Dictionary<KeyPress, HotkeyRegistration>();

        private readonly IHwndSource _window;
        private int _currentId;

        public KeyboardHook(IHwndSource window)
        {
            _window = window;
            _window[WM_HOTKEY] += OnHotkeyMessageReceived;
        }

        public ShortcutPressed this[string name]
        {
            get { return _shortcutsByKeyPress.Values.First(x => x.Name == name).Delegate; }
            set { _shortcutsByKeyPress.Values.First(x => x.Name == name).Delegate += value; }
        }

        public void RegisterGlobalShortcut(string name, ModifierKeys modifier, Keys key)
        {
            HotkeyRegistration registration;
            var keyPress = new KeyPress(modifier, key);

            var entry = GetShortcutEntryByName(name);
            if (entry.Value != null)
            {
                registration = entry.Value;
                RemoveHotKey(registration, keyPress);

                TryRegisterHotKey(name, modifier, key, registration.Id);
            }
            else
            {
                TryRegisterHotKey(name, modifier, key, ++_currentId);

                registration = new HotkeyRegistration(_currentId, name);
            }

            _shortcutsByKeyPress[keyPress] = registration;
        }

        public void UnregisterGlobalShortcut(string name)
        {
            var shortCut = GetShortcutEntryByName(name);
            if (shortCut.Value != null)
            {
                RemoveHotKey(shortCut.Value, shortCut.Key);
            }
        }

        private KeyValuePair<KeyPress, HotkeyRegistration> GetShortcutEntryByName(string name)
        {
            return _shortcutsByKeyPress.FirstOrDefault(shortcut => shortcut.Value.Name == name);
        }

        private void OnHotkeyMessageReceived(Message msg)
        {
            var key = (Keys) (((int) msg.LParam >> 16) & 0xFFFF);
            var modifier = (ModifierKeys) ((int) msg.LParam & 0xFFFF);

            var keyPress = new KeyPress(modifier, key);
            HotkeyRegistration hotkeyRegistration;
            if (_shortcutsByKeyPress.TryGetValue(keyPress, out hotkeyRegistration))
            {
                hotkeyRegistration.InvokeDelegate();
            }
        }

        ~KeyboardHook()
        {
            UnregisterAllHotKeys();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void RemoveHotKey(HotkeyRegistration registration, KeyPress keyPress)
        {
            UnregisterHotKey(_window.Handle, registration.Id);
            _shortcutsByKeyPress.Remove(keyPress);
        }

        private void TryRegisterHotKey(string name, ModifierKeys modifier, Keys key, int id)
        {
            if (!RegisterHotKey(_window.Handle, id, (uint) modifier, (uint) key))
            {
                throw new InvalidOperationException(string.Format("Couldn't register the hot key {0}.", name));
            }
        }

        private void UnregisterAllHotKeys()
        {
            foreach (var curRegistration in _shortcutsByKeyPress.Values)
            {
                UnregisterHotKey(_window.Handle, curRegistration.Id);
            }
        }
    }

    public class KeyPress
    {
        private readonly Keys _key;
        private readonly ModifierKeys _modifier;

        internal KeyPress(ModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }

        protected bool Equals(KeyPress other)
        {
            return _key == other._key && _modifier == other._modifier;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KeyPress) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) _key*397) ^ (int) _modifier;
            }
        }
    }

    public sealed class Shortcut : KeyPress
    {
        private readonly string _name;

        internal Shortcut(string name, ModifierKeys modifier, Keys key) : base(modifier, key)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}