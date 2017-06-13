using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using EveLocalChatAnalyser.Model;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui.Models
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public abstract void Dispose();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string memberName = "") where T : class
        {
            if (field == value)
            {
                return;
            }
            field = value;
            OnPropertyChanged(memberName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => handler(this, new PropertyChangedEventArgs(propertyName))));
            }
        }
    }

    public static class Brushes
    {
        public static readonly Brush ButtonBrush = new SolidColorBrush(
            new Color
            {
                A = 255,
                R = 24,
                G = 86,
                B = 110
            }); // new LinearGradientBrush(Colors.White, Colors.LightBlue, 90);

        //public static readonly SolidColorBrush SolidLightCoralBrush = new SolidColorBrush(Colors.LightCoral);
        public static readonly SolidColorBrush SolidAntiqueWhiteBrush = new SolidColorBrush(Colors.AntiqueWhite);
        public static readonly SolidColorBrush SolidLightGreenBrush = new SolidColorBrush(Colors.LightGreen);
        public static readonly SolidColorBrush SolidLightRedBrush = new SolidColorBrush(Colors.LightPink);
        public static readonly SolidColorBrush SolidLightCoralBrush = new SolidColorBrush(Colors.LightCoral);
        public static readonly SolidColorBrush SolidSandyBrownBrush = new SolidColorBrush(Colors.SandyBrown);
        public static readonly SolidColorBrush SolidBlackBrush = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush SolidWhiteBrush = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);
        public static readonly SolidColorBrush SolidRedBrush = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush SolidGreenBrush = new SolidColorBrush(Colors.Green);
        public static readonly SolidColorBrush SolidBlueBrush = new SolidColorBrush(Colors.Blue);
        public static readonly SolidColorBrush SolidOrangeBrush = new SolidColorBrush(Colors.Orange);
        public static readonly SolidColorBrush SolidDarkOrangeBrush = new SolidColorBrush(Colors.DarkOrange);
        public static readonly SolidColorBrush SolidOrchidBrush = new SolidColorBrush(Colors.Orchid);
        public static readonly SolidColorBrush SolidTransparentBrush = new SolidColorBrush(Colors.Transparent);
        public static readonly SolidColorBrush SolidLightGrayBrush = new SolidColorBrush(Colors.LightGray);
        public static readonly SolidColorBrush SolidGrayBrush = new SolidColorBrush(Colors.Gray);
        public static readonly SolidColorBrush SolidSelectedBrush = new SolidColorBrush(Colors.CornflowerBlue);

        static Brushes()
        {
            ButtonBrush.Freeze();
            SolidAntiqueWhiteBrush.Freeze();
            SolidLightGreenBrush.Freeze();
            SolidLightRedBrush.Freeze();
            SolidSandyBrownBrush.Freeze();
            SolidBlackBrush.Freeze();
            SolidWhiteBrush.Freeze();
            TransparentBrush.Freeze();
            SolidRedBrush.Freeze();
            SolidGreenBrush.Freeze();
            SolidBlueBrush.Freeze();
            SolidOrangeBrush.Freeze();
            SolidOrchidBrush.Freeze();
            SolidDarkOrangeBrush.Freeze();
            SolidTransparentBrush.Freeze();
            SolidLightGrayBrush.Freeze();
            SolidGrayBrush.Freeze();
            SolidSelectedBrush.Freeze();
        }
    }

    public class EveCharacterViewModel : ViewModelBase
    {
        private static readonly string NAME_PROPERTY = NotifyUtils.GetPropertyName((IEveCharacter c) => c.Name);
        private static readonly string ALLIANCE_PROPERTY = NotifyUtils.GetPropertyName((IEveCharacter c) => c.Alliance);
        private static readonly string CORPORATION_PROPERTY = NotifyUtils.GetPropertyName((IEveCharacter c) => c.Corporation);
        private static readonly string FACTION_PROPERTY = NotifyUtils.GetPropertyName((IEveCharacter s) => s.FactionId);

        private static readonly string KILLBOARD_INFORMATION_PROPERTY =
            NotifyUtils.GetPropertyName((IEveCharacter c) => c.KillboardInformation);

        private static readonly string KNOWN_POSITIONS_PROPERTY = NotifyUtils.GetPropertyName((IEveCharacter c) => c.KnownPositions);
        private static readonly string LOCAL_CHANGE_STATUS_PROPERTY = NotifyUtils.GetPropertyName((IEveCharacter c) => c.LocalChangeStatus);

        private static readonly string BACKGROUND_COLOR_STATUS_SETTING =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.IsDrawingBackgroundDependingOnStatus);

        private static readonly string IS_SHOWING_EXITED_SETTING =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.IsShowingExitedCharacters);

        private static readonly string IS_HIDING_BLUES_SETTING = NotifyUtils.GetPropertyName((ActiveProfile s) => s.IsHidingBlues);
        private static readonly string STANDINGS_SETTING = NotifyUtils.GetPropertyName((ActiveProfile s) => s.Standings);

        private static readonly string IS_SHOWING_SECURITY_STATUS_SETTING =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.IsShowingSecurityStatus);

        private static readonly string IS_SHOWING_AMARR_SETTING = NotifyUtils.GetPropertyName((ActiveProfile s) => s.IsShowingAmarr);
        private static readonly string IS_SHOWING_CALDARI_SETTING = NotifyUtils.GetPropertyName((ActiveProfile s) => s.IsShowingCaldari);
        private static readonly string IS_SHOWING_GALLENTE_SETTING = NotifyUtils.GetPropertyName((ActiveProfile s) => s.IsShowingGallente);
        private static readonly string IS_SHOWING_MINMATAR_SETTING = NotifyUtils.GetPropertyName((ActiveProfile s) => s.IsShowingMinmatar);
        private static readonly string COALITIONS = NotifyUtils.GetPropertyName((IEveCharacter c) => c.Coalitions);
        private static readonly TimeSpan ONE_HOUR = new TimeSpan(1, 0, 0);

        private static readonly ICustomCharacterInfoRepository CUSTOM_CHARACTER_INFO_REPOSITORY =
            DIContainer.GetInstance<ICustomCharacterInfoRepository>();

        private readonly EveLocalStatistics _statistics;
        private string _allianceInfo;
        private string _associations;
        private Brush _backgroundColor;
        private string _coalitionInfo;
        private string _corporationInfo;
        private object _factionImage;
        private string _favouriteShips;
        private Brush _foregroundColor;
        private object _image;
        private bool _isHighlighted;
        private string _isk;
        private string _iskRatio;
        private string _killDeathRatio;
        private string _killsDeaths;
        private LocalChangeStatus _localChangeStatus;
        private string _name;
        private string _points;
        private string _pointsRatio;
        private string _sightings;
        private string _tags;
        private string _toolTipText;
        private Visibility _visibility;
        private CustomCharacterInfoViewModel _customCharacterInfo;

        public EveCharacterViewModel(IEveCharacter eveCharacter, EveLocalStatistics statistics)
        {
            EveCharacter = eveCharacter;
            _statistics = statistics;
            EveCharacter.PropertyChanged += OnPropertyChanged;
            Properties.Settings.Default.PropertyChanged += SettingsPropertyChanged;
            ActiveProfile.Default.PropertyChanged += SettingsPropertyChanged;

            _customCharacterInfo = CUSTOM_CHARACTER_INFO_REPOSITORY.GetCustomCharacterInfo(eveCharacter);
            _customCharacterInfo.PropertyChanged += CustomCharacterInfoOnPropertyChanged;

            InitValues();
        }

        private void CustomCharacterInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateKillboardInformation();
            Image = GetImage();
        }

        public CustomCharacterInfoViewModel CustomCharacterInfo => _customCharacterInfo;

        public IEveCharacter EveCharacter { get; }

        public String ToolTipText
        {
            get { return _toolTipText; }
            set
            {
                if (value == _toolTipText)
                {
                    return;
                }
                _toolTipText = value;
                OnPropertyChanged();
            }
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility)
                {
                    return;
                }
                _visibility = value;
                OnPropertyChanged();
            }
        }

        public LocalChangeStatus LocalChangeStatus
        {
            get { return _localChangeStatus; }
            set
            {
                if (value == _localChangeStatus)
                {
                    return;
                }
                _localChangeStatus = value;
                OnPropertyChanged();
            }
        }

        public String Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                {
                    return;
                }
                _name = value;
                OnPropertyChanged();
            }
        }

        public String AllianceInfo
        {
            get { return _allianceInfo; }
            set
            {
                if (value == _allianceInfo)
                {
                    return;
                }
                _allianceInfo = value;
                OnPropertyChanged();
            }
        }

        public String CorporationInfo
        {
            get { return _corporationInfo; }
            set
            {
                if (value == _corporationInfo)
                {
                    return;
                }
                _corporationInfo = value;
                OnPropertyChanged();
            }
        }

        public String CoalitionInfo
        {
            get { return _coalitionInfo; }
            set { SetProperty(ref _coalitionInfo, value); }
        }

        public string LastSeenShip { get; private set; }

        public string LastSeenWeapon { get; private set; }

        public string LastSeenType { get; private set; }

        public string LastSeenTime { get; private set; }

        public object Image
        {
            get { return _image; }
            set
            {
                if (Equals(value, _image))
                {
                    return;
                }
                _image = value;
                OnPropertyChanged();
            }
        }

        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                if (_isHighlighted == value)
                {
                    return;
                }
                _isHighlighted = value;
                UpdateColors();
            }
        }

        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (Equals(value, _backgroundColor))
                {
                    return;
                }
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        public Brush ForegroundColor
        {
            get { return _foregroundColor; }
            set
            {
                if (value.Equals(_foregroundColor))
                {
                    return;
                }
                _foregroundColor = value;
                OnPropertyChanged();
            }
        }

        public string Tags
        {
            get { return _tags; }
            set
            {
                if (value == _tags)
                {
                    return;
                }
                _tags = value;
                OnPropertyChanged();
            }
        }

        public string AverageNumberOfAttackers { get; set; }

        public string IskRatio
        {
            get { return _iskRatio; }
            set
            {
                if (value == _iskRatio)
                {
                    return;
                }
                _iskRatio = value;
                OnPropertyChanged();
            }
        }

        public string PointsRatio
        {
            get { return _pointsRatio; }
            set
            {
                if (value == _pointsRatio)
                {
                    return;
                }
                _pointsRatio = value;
                OnPropertyChanged();
            }
        }

        public string KillDeathRatio
        {
            get { return _killDeathRatio; }
            set
            {
                if (value == _killDeathRatio)
                {
                    return;
                }
                _killDeathRatio = value;
                OnPropertyChanged();
            }
        }

        public string Sightings
        {
            get { return _sightings; }
            set { SetProperty(ref _sightings, value); }
        }

        public string FavouriteShips
        {
            get { return _favouriteShips; }
            set { SetProperty(ref _favouriteShips, value); }
        }

        public string Points
        {
            get { return _points; }
            set { SetProperty(ref _points, value); }
        }

        public string Associations
        {
            get { return _associations; }
            set { SetProperty(ref _associations, value); }
        }

        public string Isk
        {
            get { return _isk; }
            set { SetProperty(ref _isk, value); }
        }

        public string KillsDeaths
        {
            get { return _killsDeaths; }
            set { SetProperty(ref _killsDeaths, value); }
        }

        public object FactionImage
        {
            get { return _factionImage; }
            set { SetProperty(ref _factionImage, value); }
        }

        public SolidColorBrush LastSeenTypeForeground { get; set; }

        public SolidColorBrush LastSeenTimeForeground { get; set; }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BACKGROUND_COLOR_STATUS_SETTING || e.PropertyName == IS_SHOWING_SECURITY_STATUS_SETTING)
            {
                UpdateColors();
                return;
            }
            if (e.PropertyName == IS_SHOWING_EXITED_SETTING || e.PropertyName == IS_HIDING_BLUES_SETTING
                || e.PropertyName == STANDINGS_SETTING || e.PropertyName == IS_SHOWING_AMARR_SETTING
                || e.PropertyName == IS_SHOWING_CALDARI_SETTING || e.PropertyName == IS_SHOWING_GALLENTE_SETTING
                || e.PropertyName == IS_SHOWING_MINMATAR_SETTING)
            {
                SetVisibility();
            }
        }

        private void UpdateColors()
        {
            if (IsHighlighted)
            {
                BackgroundColor = Brushes.ButtonBrush;
                ForegroundColor = GetSecurityStatusDependendColor(Brushes.SolidWhiteBrush);
                return;
            }

            var isDrawingBackgroundDependingOnStatus = Properties.Settings.Default.IsDrawingBackgroundDependingOnStatus;

            ForegroundColor =
                GetSecurityStatusDependendColor(isDrawingBackgroundDependingOnStatus ? Brushes.SolidBlackBrush : Brushes.SolidWhiteBrush);

            if (!isDrawingBackgroundDependingOnStatus)
            {
                BackgroundColor = Brushes.TransparentBrush;
                return;
            }

            switch (EveCharacter.LocalChangeStatus)
            {
                case LocalChangeStatus.Entered:
                    BackgroundColor = Brushes.SolidLightGreenBrush;
                    return;
                case LocalChangeStatus.Exited:
                    BackgroundColor = Brushes.SolidLightRedBrush;
                    return;
                case LocalChangeStatus.Stayed:
                    BackgroundColor = Brushes.SolidAntiqueWhiteBrush;
                    return;
            }
        }

        private Brush GetSecurityStatusDependendColor(SolidColorBrush defaultColor)
        {
            return (Properties.Settings.Default.IsShowingSecurityStatus && EveCharacter.SecurityStatus <= -5.0)
                       ? Brushes.SolidRedBrush
                       : defaultColor;
        }

        private void InitValues()
        {
            Image = GetImage();
            SetName();
            SetCorporationName();
            SetAllianceName();
            SetCoalitionName();
            UpdateColors();
            SetVisibility();
            SetFactionImage();
            UpdateKillboardInformation();
            UpdateSightings();
        }

        private void SetCoalitionName()
        {
            var coalitions = EveCharacter.Coalitions.ToList();

            CoalitionInfo = coalitions.Any()
                                ? string.Join(", ", coalitions.Select(c => c.Name + " (" + _statistics.CoalitionMembersCount(c.Name) + ")"))
                                : ("none (" + _statistics.MembersInNoCoalitionCount + ")");
        }

        private void SetName()
        {
            Name = String.Format("{0} ({1}y{2}m)", EveCharacter.Name, EveCharacter.Age.Years, EveCharacter.Age.Months);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == NAME_PROPERTY)
            {
                SetName();
                return;
            }

            if (e.PropertyName == ALLIANCE_PROPERTY)
            {
                SetAllianceName();
                return;
            }

            if (e.PropertyName == COALITIONS)
            {
                SetCoalitionName();
                return;
            }

            if (e.PropertyName == FACTION_PROPERTY)
            {
                SetFactionImage();
                SetVisibility();
                return;
            }

            if (e.PropertyName == CORPORATION_PROPERTY)
            {
                SetCorporationName();
                return;
            }

            if (e.PropertyName == KILLBOARD_INFORMATION_PROPERTY)
            {
                UpdateKillboardInformation();
                Image = GetImage();
                return;
            }
            if (e.PropertyName == KNOWN_POSITIONS_PROPERTY)
            {
                UpdateSightings();
                return;
            }
            if (e.PropertyName == LOCAL_CHANGE_STATUS_PROPERTY)
            {
                LocalChangeStatus = EveCharacter.LocalChangeStatus;
                UpdateColors();
                SetVisibility();
            }
        }

        private void SetFactionImage()
        {
            var mainWindow = Application.Current?.MainWindow;

            switch (EveCharacter.FactionId)
            {
                case 500001:
                    FactionImage = mainWindow?.FindResource("CaldariImage");
                    break;
                case 500002:
                    FactionImage = mainWindow?.FindResource("MinmatarImage");
                    break;
                case 500003:
                    FactionImage = mainWindow?.FindResource("AmarrImage");
                    break;
                case 500004:
                    FactionImage = mainWindow?.FindResource("GallenteImage");
                    break;
                default:
                    FactionImage = mainWindow?.FindResource("EmptyImage");
                    break;
            }
        }

        private void UpdateSightings()
        {
            Sightings = string.Join(
                                    "\n",
                                    EveCharacter.KnownPositions.Select(
                                                                       position =>
                                                                       position.LastTimeSeen.ToShortTimeString() + "\t" + position.System));
        }

        private void UpdateKillboardInformation()
        {

            
            var killboardInformation = EveCharacter.KillboardInformation;
            if (killboardInformation == null)
            {
                return;
            }
            else
            {
                Tags = _customCharacterInfo.Tags.Any() ? string.Join("\n", _customCharacterInfo.Tags) : "";
            }
            const string FORMAT = "{0}/{1}";

            LastSeenType = killboardInformation.LastSeen.Type.ToString()
                .ToLowerInvariant();
            LastSeenShip = killboardInformation.LastSeen.ShipName;
            LastSeenWeapon = killboardInformation.LastSeen.Weapon;
            LastSeenTime = killboardInformation.LastSeen.Occurrence.GetTimeDifference();
            LastSeenTimeForeground = DateTime.UtcNow - killboardInformation.LastSeen.Occurrence < ONE_HOUR
                                         ? Brushes.SolidGreenBrush
                                         : Brushes.SolidWhiteBrush;
            if (killboardInformation.LastSeen.Type == EveLocalChatAnalyser.LastSeenType.Unknown)
            {
                LastSeenTypeForeground = Brushes.SolidWhiteBrush;
            }
            else
            {
                LastSeenTypeForeground = killboardInformation.LastSeen.Type == EveLocalChatAnalyser.LastSeenType.Kill
                                             ? Brushes.SolidGreenBrush
                                             : Brushes.SolidRedBrush;
            }

            // can be null, if zkb doesn't answer, TODO eventuell unknown oder sowas eintragen
            var totalsStatistics = killboardInformation.TotalsStatistics ?? new StatisticsEntry {ShipsDestroyed = -1, ShipsLost = -1};
               

            KillsDeaths = string.Format(FORMAT, totalsStatistics.ShipsDestroyed, totalsStatistics.ShipsLost);
            KillDeathRatio = ToRatioString(totalsStatistics.ShipsDestroyed, totalsStatistics.ShipsLost);

            Points = string.Format(FORMAT, FormatPoints(totalsStatistics.PointsDestroyed), FormatPoints(totalsStatistics.PointsLost));
            PointsRatio = ToRatioString(totalsStatistics.PointsDestroyed, totalsStatistics.PointsLost);

            Isk = string.Format(FORMAT, FormatIsk(totalsStatistics.IskDestroyed), FormatIsk(totalsStatistics.IskLost));
            IskRatio = ToRatioString((long) totalsStatistics.IskDestroyed, (long) totalsStatistics.IskLost);

            AverageNumberOfAttackers = killboardInformation.AverageAttackerCount.ToString("0.00");

            var tagList = killboardInformation.Tags.Union(_customCharacterInfo.Tags)
                .ToArray();
            Tags = tagList.Any() ? string.Join("\n", tagList) : "";

            FavouriteShips = string.Join("\n", killboardInformation.FavouriteShips);

            var associations = "";
            if (killboardInformation.AssociatedAlliances.Any() || killboardInformation.AssociatedCorporations.Any())
            {
                if (killboardInformation.AssociatedAlliances.Any())
                {
                    associations = "Alliances:\n\t" + string.Join("\n\t", killboardInformation.AssociatedAlliances);
                }
                if (killboardInformation.AssociatedCorporations.Any())
                {
                    if (killboardInformation.AssociatedAlliances.Any())
                    {
                        associations += "\n";
                    }
                    associations += "Corporations:\n\t" + string.Join("\n\t", killboardInformation.AssociatedCorporations);
                }
            }
            Associations = associations;
        }

        private static string FormatPoints(int points)
        {
            return points > 1000 ? (points / 1000.0).ToString("#.#k") : points.ToString("0");
        }

        private static string FormatIsk(double iskValue)
        {
            const string MILLION_ISK_FORMAT = "0M";
            const string BILLION_ISK_FORMAT = "0.#B";
            const double _1_BILLION = 1000000000;
            const double _1_MILLION = 1000000;

            return iskValue > _1_BILLION
                       ? (iskValue / _1_BILLION).ToString(BILLION_ISK_FORMAT)
                       : (iskValue / _1_MILLION).ToString(MILLION_ISK_FORMAT);
        }

        private static string ToRatioString(long destroyed, long lost)
        {
            var ratio = lost == 0 ? (destroyed == 0 ? 0 : 100) : destroyed / (double) lost;
            return ratio.ToString("= #0.00");
        }

        private void SetVisibility()
        {
            if (EveCharacter.LocalChangeStatus == LocalChangeStatus.Exited && !Properties.Settings.Default.IsShowingExitedCharacters)
            {
                Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!ActiveProfile.Default.IsHidingBlues)
                {
                    SetVisibilityBasedOnFaction();
                }
                else
                {
                    double standings;
                    if (TryGetStandings(EveCharacter, out standings) && standings > 0)
                    {
                        Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        SetVisibilityBasedOnFaction();
                    }
                }
            }
        }

        private void SetVisibilityBasedOnFaction()
        {
            if (EveCharacter.FactionId == 500001)
            {
                Visibility = ActiveProfile.Default.IsShowingCaldari ? Visibility.Visible : Visibility.Collapsed;
                return;
            }
            if (EveCharacter.FactionId == 500002)
            {
                Visibility = ActiveProfile.Default.IsShowingMinmatar ? Visibility.Visible : Visibility.Collapsed;
                return;
            }
            if (EveCharacter.FactionId == 500003)
            {
                Visibility = ActiveProfile.Default.IsShowingAmarr ? Visibility.Visible : Visibility.Collapsed;
                return;
            }
            if (EveCharacter.FactionId == 500004)
            {
                Visibility = ActiveProfile.Default.IsShowingGallente ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            Visibility = Visibility.Visible;
        }

        //TODO standings service ->
        public static bool TryGetStandings(IEveCharacter eveChar, out double standings)
        {
            return TryGetStandings(eveChar.Name, out standings) || TryGetStandings(eveChar.Corporation, out standings)
                   || eveChar.Alliance != null && TryGetStandings(eveChar.Alliance, out standings);
        }

        private static bool TryGetStandings(string name, out double standings)
        {
            if (ActiveProfile.Default.Standings == null || !ActiveProfile.Default.Standings.ContainsKey(name))
            {
                standings = 0;
                return false;
            }

            standings = (double) ActiveProfile.Default.Standings[name];
            return true;
        }

        private void SetCorporationName()
        {
            CorporationInfo = string.Format(
                                            "{0} ({1})",
                                            EveCharacter.Corporation,
                                            _statistics.CorporationMembersCount(EveCharacter.Corporation));
        }

        private void SetAllianceName()
        {
            var charAlliance = EveCharacter.Alliance ?? "none";
            AllianceInfo = string.Format(
                                         "{0} ({1})",
                                         charAlliance,
                                         EveCharacter.Alliance != null
                                             ? _statistics.AllianceMembersCount(charAlliance)
                                             : _statistics.MembersInNoAllianceCount);
        }

        private object GetImage()
        {
            if (!string.IsNullOrEmpty(_customCharacterInfo.IconImage))
            {
                return _customCharacterInfo.IconImage;
            }
            var killboardInformation = EveCharacter.KillboardInformation;
            if (killboardInformation == null)
            {
                return "pack://application:,,,/Resources/loader.gif";
            }

            var mainWindow = Application.Current.MainWindow;

            if (killboardInformation.Tags.Any(x => x.StartsWith("Awesome")))
            {
                return mainWindow.FindResource("SupermanImage");
            }

            if (killboardInformation.Tags.Contains("Offgrid Booster"))
            {
                return mainWindow.FindResource("GanglinkImage");
            }

            if (killboardInformation.Tags.Contains("Cynochar"))
            {
                return mainWindow.FindResource("CynoImage");
            }

            if (killboardInformation.Tags.Contains("Ganker"))
            {
                return mainWindow.FindResource("GankImage");
            }

            if (killboardInformation.Tags.Any(tag => tag.Contains("ECM")))
            {
                return mainWindow.FindResource("ECMImage");
            }

            if (killboardInformation.Tags.Any(tag => tag.Contains("smartbombs")))
            {
                return mainWindow.FindResource("SmartbombImage");
            }

            if (killboardInformation.Tags.Contains("Nepal Earthquake Relief Donator"))
            {
                return mainWindow.FindResource("EarthquakeRelief");
            }

            if (killboardInformation.Tags.Contains("Carebear"))
            {
                return mainWindow.FindResource("CarebearImage");
            }

            return mainWindow.FindResource("EmptyImage");
        }

        public override void Dispose()
        {
            EveCharacter.PropertyChanged -= OnPropertyChanged;
            Properties.Settings.Default.PropertyChanged -= SettingsPropertyChanged;
        }
    }
}
