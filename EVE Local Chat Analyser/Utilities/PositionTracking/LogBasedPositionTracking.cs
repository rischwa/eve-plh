using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EveLocalChatAnalyser.Properties;

namespace EveLocalChatAnalyser.Utilities.PositionTracking
{
    public class LogBasedPositionTracking : IDisposable
    {



        private const string LISTENER_PREFIX = "  Listener: ";
        private const string EMPFAENGER_PREFIX = "  Empfänger: ";
        public static readonly string LOG_LOCATION = string.Format(@"{0}\EVE\logs\Gamelogs", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        public static readonly Regex JUMP_REGEX =
            new Regex(@"^\[ (.{19,19}) \] \(None\) (Jumping|Springe) (from|von) .+ (to|nach) (New Caldari|Tash-Murkon Prime|Ardishapur Prime|Du Annes|Lower Debyl|Upper Debyl|Sarum Prime|Khanid Prime|Kador Prime|Serpentis Prime|Old Man Star|Kor-Azor Prime|Central Point|Promised Land|Dead End|New Eden|[^\s\*]+)\*?$");

        public static readonly Regex DOCKING_REGEX =
            new Regex(@"^\[ (.{19,19}) \] \(notify\) (Andockerlaubnis für Station|Requested to dock at) (New Caldari|Tash-Murkon Prime|Ardishapur Prime|Du Annes|Lower Debyl|Upper Debyl|Sarum Prime|Khanid Prime|Kador Prime|Serpentis Prime|Old Man Star|Kor-Azor Prime|Central Point|Promised Land|Dead End|New Eden|[^\s\*]+)");

        public static readonly Regex UNDOCKING_REGEX =
            new Regex(@"^\[ (.{19,19}) \] \(None\) (Undocking from|Abdocken von) .+ (zum Sonnensystem|to) (New Caldari|Tash-Murkon Prime|Ardishapur Prime|Du Annes|Lower Debyl|Upper Debyl|Sarum Prime|Khanid Prime|Kador Prime|Serpentis Prime|Old Man Star|Kor-Azor Prime|Central Point|Promised Land|Dead End|New Eden|[^\s\*]+)");

        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher(LOG_LOCATION);

        public LogBasedPositionTracking()
        {
            _fileSystemWatcher.Created += FileSystemWatcherChangeDetect;
            _fileSystemWatcher.Changed += FileSystemWatcherChangeDetect;
            _fileSystemWatcher.EnableRaisingEvents = true;

            //TODO koennte in nem extra thread gemacht werden
            foreach (var curCharacter in Settings.Default.PositionTrackingCharacters ?? new StringCollection())
            {
                var lastPosition = GetLastPositionForCharacter(curCharacter);

                var lastPositionFromSettings = Settings.Default.GetLastCharacterPosition(curCharacter);

                if (lastPosition != null && (lastPositionFromSettings == null || lastPosition.LastTimeSeen > lastPositionFromSettings.LastTimeSeen))
                {
                    Settings.Default.SetLastCharacterPosition(curCharacter, lastPosition);
                }
            }
        }

        private static IEnumerable<string> LogFilesByDescendingTime
        {
            get
            {
                return Directory.EnumerateFiles(LOG_LOCATION)
                    .Where(x => IsLogFile(Path.GetFileName(x)))
                    .OrderByDescending(x => x)
                    .ToList();
            }
        }

        public void Dispose()
        {
            _fileSystemWatcher.Created -= FileSystemWatcherChangeDetect;
            _fileSystemWatcher.Changed -= FileSystemWatcherChangeDetect;
            _fileSystemWatcher.Dispose();
        }

        public event SystemChanged SystemChanged;

        protected virtual void OnSystemChanged(string character, string oldSystem, string newsystem)
        {
            Settings.Default.SetLastCharacterPosition(
                                                      character,
                                                      new CharacterPosition
                                                      {
                                                          LastTimeSeen = DateTime.UtcNow,
                                                          System = newsystem
                                                      });

            var handler = SystemChanged;
            if (handler != null)
            {
                handler.
                Invoke(character, oldSystem, newsystem);
            }
        }

        public IList<string> GetCharacterNames()
        {
            var lastDate = DateTime.UtcNow - new TimeSpan(30, 0, 0, 0);
            return LogFilesByDescendingTime.Select(
                                                   x => new
                                                        {
                                                            Filename = Path.GetFileName(x),
                                                            Path = x
                                                        })
                .TakeWhile(x => TimeOfLastWrite(x.Filename) > lastDate)
                .Select(x => GetCharacterNameFromFile(x.Path))
                .Distinct()
                .Where(x => x != null)
                .ToList();
        }

        private static bool IsLogFile(string arg)
        {
            return arg.Length == 19 && arg.EndsWith(".txt");
        }

        private static DateTime TimeOfLastWrite(string curFileName)
        {
            return DateTime.ParseExact(curFileName.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        public CharacterPosition GetLastPositionForCharacter(string characterName)
        {
            if (string.IsNullOrEmpty(characterName))
            {
                return null;
            }

            var lastPositionFromSettings = Settings.Default.GetLastCharacterPosition(characterName);

            var date = lastPositionFromSettings != null ? lastPositionFromSettings.LastTimeSeen :DateTime.MinValue;

            foreach (
                var curFile in
                    LogFilesByDescendingTime.Where(
                                                   curFile =>
                                                   GetCharacterNameFromFile(curFile) == characterName
                                                   && DateTime.ParseExact(
                                                                          curFile.Substring(curFile.Length - 19,  15),
                                                                          "yyyyMMdd_HHmmss",
                                                                          CultureInfo.InvariantCulture) > date))
            {
                CharacterPosition s;
                if (TryGetLastSystemFromFile(curFile, out s))
                {
                    return s;
                }
            }
            return lastPositionFromSettings;
        }

        private static bool TryGetLastSystemFromFile(string curFile, out CharacterPosition lastSystem)
        {
            var lines = File.ReadLines(curFile)
                .Reverse();
            foreach (var curLine in lines)
            {
                CharacterPosition system;
                if (TryGetPositionFromLine(curLine, out system))
                {
                    lastSystem = system;
                    return true;
                }
            }
            lastSystem = null;
            return false;
        }

        private static bool TryGetPositionFromLine(string curLine, out CharacterPosition system)
        {
            var match = JUMP_REGEX.Match(curLine);
            if (match.Success)
            {
                system = GetCharacterPosition(match, 5);
                return true;
            }

            match = DOCKING_REGEX.Match(curLine);
            if (match.Success)
            {
                system = GetCharacterPosition(match, 3);
                return true;
            }

            match = UNDOCKING_REGEX.Match(curLine);
            if (match.Success)
            {
                system = GetCharacterPosition(match, 4);
                return true;
            }

            system = null;
            return false;
        }

        private static CharacterPosition GetCharacterPosition(Match match, int positionOfSystem)
        {
            return new CharacterPosition
                   {
                       LastTimeSeen = DateTime.ParseExact(match.Groups[1].Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture),
                       System = match.Groups[positionOfSystem].Value
                   };
        }

        private static string GetCharacterNameFromFile(string curFileName)
        {
            try
            {
                var thirdLine = File.ReadLines(curFileName)
                    .Skip(2)
                    .FirstOrDefault();

                if (thirdLine == null)
                {
                    return null;
                }

                if (thirdLine.StartsWith(LISTENER_PREFIX))
                {
                    return thirdLine.Substring(LISTENER_PREFIX.Length);
                }

                return thirdLine.StartsWith(EMPFAENGER_PREFIX) ? thirdLine.Substring(EMPFAENGER_PREFIX.Length) : null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        private void FileSystemWatcherChangeDetect(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (fileSystemEventArgs.ChangeType != WatcherChangeTypes.Changed && fileSystemEventArgs.ChangeType != WatcherChangeTypes.Created)
            {
                return;
            }

            var name = GetCharacterNameFromFile(fileSystemEventArgs.FullPath);
            if (Settings.Default.PositionTrackingCharacters == null || !Settings.Default.PositionTrackingCharacters.Contains(name))
            {
                return;
            }

            CharacterPosition curPosition;
            if (!TryGetLastSystemFromFile(fileSystemEventArgs.FullPath, out curPosition))
            {
                return;
            }

            var lastPositionFromSettings = Settings.Default.GetLastCharacterPosition(name);
            var lastSystem = lastPositionFromSettings != null ? lastPositionFromSettings.System : null;
            if (lastSystem == null || (lastSystem != curPosition.System && lastPositionFromSettings.LastTimeSeen < curPosition.LastTimeSeen))
            {
                OnSystemChanged(name, lastSystem, curPosition.System);
            }
        }
    }
}
