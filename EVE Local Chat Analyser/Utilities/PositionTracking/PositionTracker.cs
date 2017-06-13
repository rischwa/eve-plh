using System;
using System.ComponentModel;
using System.Windows;
using EveLocalChatAnalyser.Properties;

namespace EveLocalChatAnalyser.Utilities.PositionTracking
{
    //TODO das funzt weil zweimal string, nicht gut
    public delegate void SystemChanged(string character, string oldSystem, string newSystem);

    public delegate void ActiveCharacterSystemChanged(string character, string newSystem);

    public interface IPositionTracker
    {
        string this[string charName] { get; }

        string CurrentSystemOfActiveCharacter { get; }

        event ActiveCharacterSystemChanged ActiveCharacterSystemChanged;

        event SystemChanged SystemChanged;
    }

    //TODO per character position tracker

    public class PositionTracker : IPositionTracker
    {
        private readonly IActiveCharacterTracker _activeCharacterTracker;
        private readonly LogBasedPositionTracking _positionTracking = DIContainer.GetInstance<LogBasedPositionTracking>();
        private string _curActiveCharacter;

        public PositionTracker(IActiveCharacterTracker activeCharacterTracker)
        {
            _activeCharacterTracker = activeCharacterTracker;
            WebServer.Instance.SystemChanged += OnSystemChanged;
            _positionTracking.SystemChanged += OnSystemChanged;
            _activeCharacterTracker.ActiveCharacterChanged += ActiveCharacterTrackerOnActiveCharacterChanged;
            Settings.Default.PropertyChanged += DefaultOnPropertyChanged;

            if (_activeCharacterTracker.LastActiveCharacter != null)
            {
                ActiveCharacterTrackerOnActiveCharacterChanged(_activeCharacterTracker.LastActiveCharacter);
            }
        }

        private void DefaultOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName !=
                NotifyUtils.GetPropertyName((Settings s) => s.PositionTrackingCharacters))
            {
                return;
            }

            var lastActiveCharacter = _activeCharacterTracker.LastActiveCharacter;
            if (lastActiveCharacter == null || _curActiveCharacter == lastActiveCharacter)
            {
                return;
            }

            ActiveCharacterTrackerOnActiveCharacterChanged(lastActiveCharacter);
        }

        private void ActiveCharacterTrackerOnActiveCharacterChanged(string activecharacter)
        {
            if (Settings.Default.PositionTrackingCharacters == null || !Settings.Default.PositionTrackingCharacters.Contains(activecharacter))
            {
                return;
            }

            _curActiveCharacter = activecharacter;
            var activeCharHandler = ActiveCharacterSystemChanged;
            if (activeCharHandler == null || Application.Current == null)
            {
                return;
            }

            var sys = Settings.Default.GetLastCharacterPosition(activecharacter);
           
            var lastLogBasedSys = _positionTracking.GetLastPositionForCharacter(activecharacter);
            if (sys == null || (lastLogBasedSys != null && sys.LastTimeSeen < lastLogBasedSys.LastTimeSeen))
            {
                Settings.Default.SetLastCharacterPosition(activecharacter, lastLogBasedSys);
                sys = lastLogBasedSys;
            }

            if (sys != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { activeCharHandler(activecharacter, sys.System); }));
            }
        }

        public string this[string charName]
        {
            get
            {
                var lastCharacterPosition = Settings.Default.GetLastCharacterPosition(charName);
                return lastCharacterPosition != null ? lastCharacterPosition.System : null;
            }
        }

        public string CurrentSystemOfActiveCharacter
        {
            get { return _curActiveCharacter != null ? this[_curActiveCharacter] : null; }
        }

        public event ActiveCharacterSystemChanged ActiveCharacterSystemChanged;

        public event SystemChanged SystemChanged;

        private void OnSystemChanged(string character, string oldSystem, string newsystem)
        {
            Settings.Default.SetLastCharacterPosition(character, new CharacterPosition {LastTimeSeen = DateTime.UtcNow, System = newsystem});

            var handler = SystemChanged;
            if (handler != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { handler(character, oldSystem, newsystem); }));
            }

            var activeCharHandler = ActiveCharacterSystemChanged;
            if (activeCharHandler != null && _curActiveCharacter == character)
            {
                //TODO evtl. auch SystemChanged delegate fuer activechange benutzen?
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { activeCharHandler(character, newsystem); }));
            }
        }
    }
}