using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;
using Common.Logging;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators;
using EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators;
using EveLocalChatAnalyser.Ui.Settings;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.QuickAction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EveLocalChatAnalyser.Properties
{
    internal class Profile
    {
        public string Name;
        public Dictionary<string, object> Settings = new Dictionary<string, object>();

        public override string ToString()
        {
            return Name;
        }
    }

    internal partial class Settings
    {
        private const string TOGGLE_MINIMIZE = "Toggle Minimize";
        private const string QUICK_ACTION = "Quick Action";
        public const string DEFAULT_PROFILE_NAME = "Default";
        public const int VCODE_LENGTH = 64;
        private static readonly ILog LOG = LogManager.GetLogger("Settings");
        private IDictionary<string, CharacterPosition> _characterPositions;
        private bool _isNeedingNewCoalitionMerge = true;
        private List<Coalition> _lastCoalitions;
        private string _lastCoalitionString;
        private List<Coalition> _localCoalitions;
        private Dictionary<string, Profile> _profileForCharacter;

        public Settings()
        {
            if (IndexCalculatorCollection.ViewModels.All(x => x.Name != MapInnerCircleColorType))
            {
                MapInnerCircleColorType = typeof (AbsolutePvpOrPveActivityIndexCalculator).Name;
            }

            if (IndexCalculatorCollection.ViewModels.All(x => x.Name != MapCircleBorderType))
            {
                MapCircleBorderType = typeof(AbsolutePodKillActivityIndexCalculator).Name;
            }

            if (TriStateCalculatorCollection.ViewModels.All(x => x.Name != MapCircleMarkerType))
            {
                MapCircleMarkerType = typeof (StationTriStateCalculator).Name;
            }
        }

        public IEnumerable<Profile> Profiles
        {
            get { return JsonConvert.DeserializeObject<IList<Profile>>(ProfilesSerialized); }
            set { ProfilesSerialized = JsonConvert.SerializeObject(value); }
        }

        public Profile DefaultProfile
        {
            get { return Profiles.First(x => x.Name == DEFAULT_PROFILE_NAME); }
        }

        private IDictionary<string, CharacterPosition> LastCharacterPositions
        {
            get { return GetCachedJson(LastCharacterPositionsJson, ref _characterPositions); }
            set
            {
                LastCharacterPositionsJson = JsonConvert.SerializeObject(value);
                _characterPositions = value; //TODO should be a copy
            }
        }

        public List<Coalition> Coalitions
        {
            get
            {
                if (_lastCoalitionString != CoalitionsJson)
                {
                    _localCoalitions = JsonConvert.DeserializeObject<List<Coalition>>(CoalitionsJson);
                    _lastCoalitionString = CoalitionsJson;
                }
                return _localCoalitions;
            }
            set
            {
                CoalitionsJson = JsonConvert.SerializeObject(value);
                _isNeedingNewCoalitionMerge = true;
            }
        }

        //TODO nicht schoen, sollte extern passieren
        public List<Coalition> MergedCoalitions
        {
            get
            {
                if (_lastCoalitions == null || _isNeedingNewCoalitionMerge)
                {
                    IList<Coalition> remoteCoalitions;
                    try
                    {
                        var coalitionService = DIContainer.GetInstance<ICoalitionService>();
                        remoteCoalitions = coalitionService.GetCoalitions()
                            .Result;
                    }
                    catch (Exception e)
                    {
                        LOG.Error("could not load coaltions", e);
                        remoteCoalitions = new Coalition[0];
                    }

                    _lastCoalitions = Coalitions.Union(remoteCoalitions)
                        .ToList();
                    _isNeedingNewCoalitionMerge = false;
                }

                return _lastCoalitions;
            }
        }

        public Dictionary<string, Profile> ProfileForCharacter
        {
            get
            {
                if (_profileForCharacter == null)
                {
                    InitProfilesForCharacters(Profiles);
                }
                return _profileForCharacter;
            }
        }

        public CharacterPosition GetLastCharacterPosition(string name)
        {
            CharacterPosition result;
            return LastCharacterPositions.TryGetValue(name, out result) ? result : null;
        }

        public void SetLastCharacterPosition(string name, CharacterPosition position)
        {
            var positions = LastCharacterPositions;
            positions[name] = position;

            LastCharacterPositions = positions;
        }

        private T GetCachedJson<T>(string jsonValue, ref T var) where T : class
        {
            if (var != null)
            {
                return var;
            }

            var = JsonConvert.DeserializeObject<T>(jsonValue);

            return var;
        }

        private static Dictionary<string, object> GetActiveProfileSettings()
        {
            return EveLocalChatAnalyser.Properties.ActiveProfile.Default.PropertyValues.Cast<SettingsPropertyValue>()
                .ToDictionary(k => k.Name, v => v.PropertyValue);
        }

        public void CreateProfile(string profileName)
        {
            var profiles = Profiles;
            var profileList = profiles as IList<Profile> ?? profiles.ToList();
            if (profileList.Any(x => x.Name == profileName) || profileName == DEFAULT_PROFILE_NAME)
            {
                throw new ArgumentException("Duplicate profile name", "profileName");
            }

            var newProfile = new Profile
                             {
                                 Name = profileName,
                                 Settings = new Dictionary<string, object>(DefaultProfile.Settings)
                             };
            Profiles = profileList.Concat(new[] {newProfile});
        }

        public void RemoveProfile(string profileName)
        {
            if (profileName == DEFAULT_PROFILE_NAME)
            {
                throw new ArgumentException("Cannot remove Default profile", profileName);
            }

            Profiles = Profiles.Where(x => x.Name != profileName);
        }

        public void ActivateProfile(Profile profile)
        {
            SetActiveProfileSettings(profile.Settings);
            ActiveProfile = profile.Name;

            EnsureUpdatedStandings();
        }

        public void EnsureUpdatedStandings()
        {
            var activeProfile = EveLocalChatAnalyser.Properties.ActiveProfile.Default;

            var hasAPIKey = activeProfile.KeyId > 0 && activeProfile.VCode.Length == VCODE_LENGTH;
            if (hasAPIKey && (activeProfile.KeyId != activeProfile.LastUpdateKeyId || AreStandingsOutdated(activeProfile)))
            {
                if (String.IsNullOrEmpty(activeProfile.CharacterId) || activeProfile.KeyId != activeProfile.LastUpdateKeyId)
                {
                    var keyIdStr = activeProfile.KeyId.ToString(CultureInfo.InvariantCulture);
                    activeProfile.CharacterId = ApiKeyInfoService.GetCharacterId(keyIdStr, activeProfile.VCode);
                }

                DateTime cachedUntil;
                activeProfile.Standings =
                    EveStandingsApiService.GetStandings(
                                                        activeProfile.CharacterId.ToString(CultureInfo.InvariantCulture),
                                                        activeProfile.KeyId,
                                                        activeProfile.VCode,
                                                        out cachedUntil);

                activeProfile.LastUpdateKeyId = activeProfile.KeyId;
                activeProfile.StandingsCachedUntil = cachedUntil;
            }
        }

        private static bool AreStandingsOutdated(ActiveProfile activeProfile)
        {
            return activeProfile.StandingsCachedUntil == default(DateTime) || activeProfile.StandingsCachedUntil < DateTime.UtcNow;
        }

        public void ActivateProfile(string profileName)
        {
            if (profileName == ActiveProfile)
            {
                return;
            }

            var profile = Profiles.First(x => x.Name == profileName);
            ActivateProfile(profile);
        }

        private static void SetActiveProfileSettings(Dictionary<string, object> settings)
        {
            var profile = EveLocalChatAnalyser.Properties.ActiveProfile.Default;
            foreach (var curSetting in settings) //TODO standings name per code
            {
                if (curSetting.Key == "Standings")
                {
                    var table = new Hashtable();
                    var obj = (JObject) curSetting.Value;
                    if (obj != null)
                    {
                        foreach (var entry in obj)
                        {
                            table[entry.Key] = entry.Value.Value<double>();
                        }
                    }

                    profile[curSetting.Key] = table;
                    continue;
                }
                profile[curSetting.Key] = curSetting.Value;
            }
        }

        public void UpdateProfiles()
        {
            var profiles = Profiles.ToList();

            profiles.Remove(profiles.First(x => x.Name == ActiveProfile));

            Profiles = profiles.Concat(
                                       new[]
                                       {
                                           new Profile
                                           {
                                               Name = ActiveProfile,
                                               Settings = GetActiveProfileSettings()
                                           }
                                       })
                .ToList();

            //we need to use Profiles property for Init, so the standings hashtable setting is already converted
            InitProfilesForCharacters(Profiles);
        }

        private void InitProfilesForCharacters(IEnumerable<Profile> newProfiles)
        {
            _profileForCharacter = new Dictionary<string, Profile>();
            foreach (var curProfile in newProfiles)
            {
                Object charSetting;
                if (!curProfile.Settings.TryGetValue("CharactersToActivateProfileFor", out charSetting))
                {
                    charSetting = "";
                }
                var characters = charSetting.ToString()
                    .Split(';', ',', ':')
                    .Select(x => x.Trim());
                foreach (var curChar in characters)
                {
                    //TODO multiple definition for same char exception?
                    _profileForCharacter[curChar] = curProfile;
                }
            }
        }

        public void ActivateProfileForCharacter(string charName)
        {
            Profile profile;

            if (ProfileForCharacter.TryGetValue(charName, out profile))
            {
                ActivateProfile(profile);
            }
            else
            {
                ActivateProfile(DEFAULT_PROFILE_NAME);
            }
        }

        public static void RegisterGlobalShortcut()
        {
            //TODO das muss hier an das property change event gebunden werden, damit cancel auch beruecksichtig wird
            var hook = DIContainer.GetInstance<IKeyboardHook>();
            var quickAction = DIContainer.GetInstance<IQuickAction>();
            SetupShortcut(hook, TOGGLE_MINIMIZE, Default.ShortcutToggleMinimizeAll, ((App) Application.Current).OnToggleMinimize);
            SetupShortcut(hook, QUICK_ACTION, Default.ShortcutQuickAction, x => quickAction.Run());
        }

        private static void SetupShortcut(IKeyboardHook hook, string shortcutId, string shortcutKeys, ShortcutPressed action)
        {
            if (string.IsNullOrEmpty(shortcutKeys))
            {
                hook.UnregisterGlobalShortcut(shortcutId);
                return;
            }

            var keyPress = KeyExtensions.ToKeyPress(shortcutKeys);
            try
            {
                hook.RegisterGlobalShortcut(shortcutId, keyPress.Modifier, keyPress.Key);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                                Application.Current.MainWindow,
                                string.Format("Could not register global shortcut: {0}", e.Message),
                                "WARNING");
                return;
            }
            if (hook[shortcutId] == null)
            {
                hook[shortcutId] += action;
            }
        }
    }
}
