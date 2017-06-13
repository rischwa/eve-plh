#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using log4net;

#endregion

namespace EveLocalChatAnalyser
{
    public class LocalChatAnalyser
    {
        //public readonly ObservableCollection<EveCharacter> Characters = new ObservableCollection<EveCharacter>();

        //private readonly EveLocalClipboard _eveLocalClipboard = new EveLocalClipboard();

        private readonly BlockingCollection<IList<string>> _clipboardUpdates =
            new BlockingCollection<IList<string>>(new ConcurrentQueue<IList<string>>());

        private readonly Dictionary<string, IEveCharacter> _knownCharacters = new Dictionary<string, IEveCharacter>();

        private readonly object _mutex = new object();
        private readonly EveLocalStatistics _statistics = new EveLocalStatistics();

        public Error Error;

        public UpdateOfLocal UpdateCharacters;
        private IList<string> _characterNames;

        private string _currentSystem;
        private List<IEveCharacter> _currentlyInLocal = new List<IEveCharacter>();
        private bool _hasSystemChanged;
        private List<IEveCharacter> _previouslyInLocal = new List<IEveCharacter>();
        private static readonly ILog LOG = LogManager.GetLogger("LocalChatAnalyser");

        public LocalChatAnalyser(IPositionTracker positionTracker)
        {
            positionTracker.ActiveCharacterSystemChanged += SystemChange;
            Settings.Default.PropertyChanged += SettingsChanged;
        }

        public EveLocalStatistics Statistics
        {
            get { return _statistics; }
        }

        public void Clear()
        {
            Task.Factory.StartNew(() =>
                {
                    lock (_mutex)
                    {
                        _characterNames = new List<string>();
                        _currentlyInLocal.Clear();
                        _previouslyInLocal.Clear();
                    }
                });

            UpdateCharacters(new List<IEveCharacter>());
        }

        private void SystemChange(string characterName, string newSystem)
        {
            _currentSystem = newSystem;
            _hasSystemChanged = true;
        }

        public void OnUpdateOfClipboard(IList<string> characterNames)
        {
            _clipboardUpdates.Add(characterNames);
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    lock (_mutex)
                    {
                        _characterNames = _clipboardUpdates.Take();
                        if (!_characterNames.Any())
                        {
                            continue;
                        }
                        UpdateCurrentlyInLocal();
                        EvaluateChangesInLocal();
                    }
                }
                catch (Exception e)
                {
                    LOG.Warn(e);
                    FireError(e.Message);
                }
            }
// ReSharper disable FunctionNeverReturns
        }

// ReSharper restore FunctionNeverReturns

        private void AddToKnownCharacters(IList<IEveCharacter> newChars)
        {
            _knownCharacters.AddAll(newChars, character => character.Name);
        }

        private static void ChangeCharactersStatus(IEnumerable<IEveCharacter> characters, LocalChangeStatus status)
        {
            foreach (var curCharacter in characters)
            {
                curCharacter.LocalChangeStatus = status;
            }
        }

        private void EvaluateChangesInLocal()
        {
            var enteredCharacters = _currentlyInLocal.Except(_previouslyInLocal).ToList();
            ChangeCharactersStatus(enteredCharacters, LocalChangeStatus.Entered);

            var stayedCharacters = _currentlyInLocal.Intersect(_previouslyInLocal).ToList();
            ChangeCharactersStatus(stayedCharacters, LocalChangeStatus.Stayed);

            var extitedCharacters = _previouslyInLocal.Except(_currentlyInLocal).ToList();
            ChangeCharactersStatus(extitedCharacters, LocalChangeStatus.Exited);

            _statistics.UpdateLocalStatistics(_currentlyInLocal);
            UpdatePositions();

            FireUpdateOfLocal(enteredCharacters.Union(stayedCharacters).Union(extitedCharacters));
        }

        private void UpdatePositions()
        {
            if (_hasSystemChanged)
            {
                UpdatePositionsOfCharacters(_currentlyInLocal, _currentSystem);
                _hasSystemChanged = false;
            }
            else
            {
                if (_currentSystem != null)
                {
                    UpdateLastSeenTimestamp(_currentlyInLocal);
                }
            }
        }

        private void UpdateLastSeenTimestamp(IEnumerable<IEveCharacter> currentlyInLocal)
        {
            var lastSeen = DateTime.UtcNow;
            foreach (
                var lastPosition in
                    currentlyInLocal.Select(curChar => curChar.KnownPositions.FirstOrDefault())
                                    .Where(lastPosition => lastPosition != null))
            {
                lastPosition.LastTimeSeen = lastSeen;
            }
        }

        private void UpdatePositionsOfCharacters(IEnumerable<IEveCharacter> currentlyInLocal, string system)
        {
            var lastSeen = DateTime.UtcNow;
            foreach (var curChar in currentlyInLocal)
            {
                curChar.AddKnownPosition(new CharacterPosition {LastTimeSeen = lastSeen, System = system});
            }
        }

        private IEnumerable<IEveCharacter> ExtractCharacters(IEnumerable<string> characterNamesInClipboard)
        {
            var unknownNames = new List<string>();
            var knownCharactersInCurrentClipboard = new List<IEveCharacter>();

            foreach (var curName in characterNamesInClipboard)
            {
                IEveCharacter curChar;
                if (_knownCharacters.TryGetValue(curName, out curChar))
                {
                    knownCharactersInCurrentClipboard.Add(curChar);
                }
                else
                {
                    unknownNames.Add(curName);
                }
            }

            var unknownCharacters = RetrieveUnknownCharacters(unknownNames);

            return knownCharactersInCurrentClipboard.Union(unknownCharacters);
        }

        private void FireError(string message)
        {
            if (Error != null)
            {
                Error(message);
            }
        }

        private void FireUpdateOfLocal(IEnumerable<IEveCharacter> updatedCharacters)
        {
            if (UpdateCharacters != null)
            {
                UpdateCharacters(updatedCharacters.OrderBy(character => character.Name).ToList());
            }
        }

        private IEnumerable<IEveCharacter> ParseClipboard()
        {
            return ExtractCharacters(_characterNames);
        }


        private IEnumerable<IEveCharacter> RetrieveUnknownCharacters(IList<string> unknownNames)
        {
            var newCharacters = EveCharacterLoader.RetrieveCharactersByName(unknownNames);
            Task.Factory.StartNew(() => EveAffiliationsService.LoadAffiliations(newCharacters), TaskCreationOptions.LongRunning);
            if (Settings.Default.IsUsingKosInformation)
            {
                SetKosStatusOn(newCharacters);
            }

            AddToKnownCharacters(newCharacters);

            return newCharacters;
        }

        private static void SetKosStatusOn(IList<IEveCharacter> newCharacters)
        {
            //Task.Factory.StartNew(() => KosStatusLoader.SetKosStatusOn(newCharacters));
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (Settings.Default.IsUsingKosInformation)
            //{
            //    KosStatusLoader.SetKosStatusOn(_knownCharacters.Values.ToList());
            //}
        }

        private void UpdateCurrentlyInLocal()
        {
            _previouslyInLocal = _currentlyInLocal.ToList();
            _currentlyInLocal = ParseClipboard().ToList();
        }
    }


    //TODO update mit statistik machen
    public delegate void UpdateOfLocal(List<IEveCharacter> characters);

    public delegate void Error(string message);
}