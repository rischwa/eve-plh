using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace EveLocalChatAnalyser.Utilities
{
    internal static class KeyExtensions
    {
        public static bool IsModifier(this Key key)
        {
            return key == Key.LeftCtrl || key == Key.LeftShift || key == Key.LeftAlt || key == Key.RightCtrl ||
                   key == Key.RightAlt || key == Key.RightShift || key == Key.LWin || key == Key.RWin;
        }

        public static KeyPress ToKeyPress(string keys)
        {
            var mods = ModifierKeys.None;
            mods = mods | (keys.Contains("Ctrl") ? ModifierKeys.Control : ModifierKeys.None);
            mods = mods | (keys.Contains("Alt") ? ModifierKeys.Alt : ModifierKeys.None);
            mods = mods | (keys.Contains("Shift") ? ModifierKeys.Shift : ModifierKeys.None);
            mods = mods | (keys.Contains("Win") ? ModifierKeys.Windows : ModifierKeys.None);

            var key = (Keys) Enum.Parse(typeof (Keys), keys.Last().ToString(CultureInfo.InvariantCulture));

            return new KeyPress(mods, key);
        }
    }
}