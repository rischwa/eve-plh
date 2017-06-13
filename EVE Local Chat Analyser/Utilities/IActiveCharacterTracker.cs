using System;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Utilities.Win32;

namespace EveLocalChatAnalyser.Utilities
{
    public delegate void ActiveCharacterChanged(string activeCharacter);
    public interface IActiveCharacterTracker
    {
        string LastActiveCharacter { get; }
        event ActiveCharacterChanged ActiveCharacterChanged;
    }

    public class ActiveCharacterTracker : IActiveCharacterTracker
    {
        private readonly Win32Hook _windowHook;
        public event ActiveCharacterChanged ActiveCharacterChanged;
        private string _lastActiveCharacter;

        protected virtual void OnActiveCharacterChanged(string activecharacter)
        {
            var handler = ActiveCharacterChanged;
            if (handler != null) handler(activecharacter);
        }

        public ActiveCharacterTracker()
        {
            _windowHook = new Win32Hook(Win32Hook.EVENT_SYSTEM_FOREGROUND, Win32Hook.EVENT_SYSTEM_FOREGROUND,
                                     Win32Hook.WINEVENT_OUTOFCONTEXT);

            _windowHook.Event += OnWindowSwitch;

            TryGetActiveEveCharacter(out _lastActiveCharacter);
        }

        public static bool TryGetActiveEveCharacter(out string characterName)
        {
            var title = Win32ActiveWindow.GetActiveWindowTitle();
            if (title.StartsWith("EVE - "))
            {
                characterName = title.Substring("EVE - ".Length);
                return true;
            }

            characterName = null;
            return false;
        }

        //TODO oder tryget?
        public string LastActiveCharacter { get { return _lastActiveCharacter; } }

        private void OnWindowSwitch(IntPtr hwineventhook, uint eventtype, IntPtr hwnd, int idobject, int idchild,
                                    uint dweventthread, uint dwmseventtime)
        {
            var title = Win32ActiveWindow.GetActiveWindowTitle();
            if (title.StartsWith("EVE - "))
            {
                var charName = title.Substring("EVE - ".Length);
                if (charName == _lastActiveCharacter)
                {
                    return;
                }
                _lastActiveCharacter = charName;
                ActiveCharacterChanged(charName);
                //TODO das muss woanders passieren
                if (Settings.Default.IsSwitchingActiveProfileAutomatically)
                {
                    Settings.Default.ActivateProfileForCharacter(charName);
                }
                
            }
        }
    }
}
