using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Ui.Map;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using GraphX.PCL.Common.Models;
using PLHLib;

namespace EveLocalChatAnalyser.Ui.Models
{
    public class Station
    {
        public string Name { get; set; }
        public bool HasRepairFacility { get; set; }
    }

    public enum SovStatus
    {
        Npc,
        Player,
        None
    }

    public class SovereignityData
    {
        public long FactionWarefareOccupyingFactionId { get; set; }
        public bool IsFactionWarefareSystem { get; set; }
        public string SovereignityHolderTicker { get; set; }
        public string SovereignityHolder { get; set; }
        public long OwningFactionId { get; set; }
        public SovStatus SovStatus { get; set; }
    }

    public class SystemSovDataRepository
    {
        //TODO handle exceptions getting api data
        public static async Task<SovereignityData> GetSovDataForSystem(long systemId)
        {
            var sovT = new EveSovereignityService().GetSovereignityBySystemId();
            var allianceT = new EveAllianceListService().GetAlliancesByAllianceId();

            var sov = await sovT;
            var alliances = await allianceT;

            SovEntry systemSov;
            if (!sov.TryGetValue(systemId, out systemSov))
            {
                return new SovereignityData {SovStatus = SovStatus.None};
            }

            var result = new SovereignityData {SovStatus = systemSov.IsNpcSov ? SovStatus.Npc : SovStatus.Player};
            if (systemSov.IsNpcSov)
            {
                result.OwningFactionId = systemSov.FactionID;
                result.SovereignityHolder = systemSov.Faction;
                result.SovereignityHolderTicker = "";
            }
            else
            {
                var alliance = alliances[systemSov.AllianceID];
                result.SovereignityHolder = alliance.Name;
                result.SovereignityHolderTicker = alliance.Ticker;
            }

            return result;
        }
    }

    //TODO tooltip control

    public class SolarSystemViewModel : VertexBase, INotifyPropertyChanged, IEquatable<SolarSystemViewModel>
    {
        public SolarSystemViewModel()
        {
            InnerCircleColor = Brushes.SolidTransparentBrush;
            CircleBorderColor = Brushes.SolidWhiteBrush;
            Killboard = new SolarSystemKills();
        }

        public bool Equals(SolarSystemViewModel other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as SolarSystemViewModel;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        private bool _isCurrentSystem;
        private bool _isSelected;
        private string _name;
        private Brush _innerCircleColor;
        private Brush _circleBorderColor;
        private Visibility _crossLine1Visibility;
        private Visibility _crossLine2Visibility;

        public static IEqualityComparer<SolarSystemViewModel> SolarSystemComparer { get; } = new SolarSystemEqualityComparer();

        public double SecurityStatus { get; set; }

        public String SecurityStr => SecurityStatus.ToString("0.0");

        public int RegionID { get; set; }

        public string StationsWithRepairFacilityTooltip
        {
            get
            {
                return $"{string.Join("\n", StationsWithRepairFacility.Select(x => x.Name))}";
            }
        }

        public KillsBySystem Kills { get; set; }

        public SolarSystemKills Killboard { get; set; }

        public int PodKillCount => Math.Max(Kills.PodKillCount, Killboard?.SmartbombPoddingCountVeryRecent ?? 0);

        public bool IsCurrentSystem
        {
            get { return _isCurrentSystem; }
            set
            {
                _isCurrentSystem = value;
                UpdateColors();
            }
        }

        public Visibility CrossLine1Visibility
        {
            get { return _crossLine1Visibility; }
            set
            {
                if (value == _crossLine1Visibility)
                {
                    return;
                }
                _crossLine1Visibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility CrossLine2Visibility
        {
            get { return _crossLine2Visibility; }
            set
            {
                if (value == _crossLine2Visibility)
                {
                    return;
                }
                _crossLine2Visibility = value;
                OnPropertyChanged();
            }
        }

        public Brush BorderHighlightColor
        {
            get
            {
                if (Kills.ShipKillCount > 0)
                {
                    return Brushes.SolidBlueBrush;
                }
                return Kills.NpcKillCount > 50 ? Brushes.SolidLightGreenBrush : Brushes.TransparentBrush;
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                IsWormholeSystem = WormholeConnectionTracker.WH_REGEX.IsMatch(_name);
            }
        }

        public int StationCount => Stations.Count;

        public bool IsHighSec => SecurityStatus >= 0.45;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                UpdateColors();
            }
        }

        //public Brush BackgroundColor
        //{
        //    get
        //    {
        //        if (IsCurrentSystem)
        //        {
        //            return Brushes.SolidOrchidBrush;
        //        }
        //        if (
        //            IsSelected)
        //        {
        //            return Brushes.ButtonBrush;
        //        }
        //        if (IsWormholeSystem)
        //        {
        //            return Brushes.SolidLightGrayBrush;
        //        }

        //        if (SecurityStatus <= 0)
        //        {
        //            return Brushes.SolidLightCoralBrush;
        //        }
        //        return SecurityStatus < 0.45 ? Brushes.SolidOrangeBrush : Brushes.SolidLightGreenBrush;
        //    }
        //}

        //public Brush ForegroundColor => IsCurrentSystem || IsSelected ? Brushes.SolidAntiqueWhiteBrush : Brushes.SolidBlackBrush;


        public Brush BackgroundColor
        {
            get
            {
                if (IsCurrentSystem)
                {
                    return Brushes.SolidOrchidBrush;
                }
                if (
                    IsSelected)
                {
                    return Brushes.SolidSelectedBrush;
                }
                return Brushes.SolidTransparentBrush;
            }
        }

        public Brush ForegroundColor
        {
            get
            {
                if (IsCurrentSystem || IsSelected)
                {
                    return Brushes.SolidAntiqueWhiteBrush;
                }
                
                if (IsWormholeSystem)
                {
                    return Brushes.SolidLightGrayBrush;
                }

                if (SecurityStatus <= 0)
                {
                    return Brushes.SolidLightCoralBrush;
                }
                return SecurityStatus < 0.45 ? Brushes.SolidOrangeBrush : Brushes.SolidLightGreenBrush;
            }
        }

        public WormholeInfo WormholeInfo { get; set; }

        public string SystemTypeText => IsWormholeSystem ? WormholeInfo.Class : Sovereignity.SovereignityHolderTicker;

        public string SystemTypeTooltip => IsWormholeSystem ? WormholeInfoTooltip : Sovereignity.SovereignityHolder;

        public string WormholeInfoTooltip
        {
            get
            {
                if (!IsWormholeSystem)
                {
                    return "";
                }

                var builder = new StringBuilder();

                if (WormholeInfo.Effects.Any())
                {
                    builder.Append(WormholeInfo.Anomaly);
                    builder.Append(":\n\t").Append(string.Join("\n\t",
                                                             WormholeInfo.Effects.Select(
                                                                 x =>
                                                                 string.Format("{0}: {1}", x.EffectedStat, x.EffectValue))));
                }
                else
                {
                    builder.Append("Effects: none");
                }
                builder.Append("\n\n");
                if (!string.IsNullOrEmpty(WormholeInfo.Static1))
                {
                    builder.Append("Static 1: ");
                    builder.Append(WormholeInfo.Static1);
                    builder.Append("\n");
                }

                if (!string.IsNullOrEmpty(WormholeInfo.Static1))
                {
                    builder.Append("Static 2: ");
                    builder.Append(WormholeInfo.Static2);
                    builder.Append("\n");
                }

                return builder.ToString();
            }
        }

        public int StationWithRepairFacilityCount
        {
            get { return Stations.Count(x => x.HasRepairFacility); }
        }

        public IList<Station> Stations { get; set; }

        public IList<Station> StationsWithRepairFacility
        {
            get { return Stations.Where(x => x.HasRepairFacility).ToList(); }
        }

        public object FactionImage
        {
            get
            {
                if (Sovereignity.SovStatus == SovStatus.Npc)
                {
                    if (Sovereignity.IsFactionWarefareSystem)
                    {
                        return GetFactionImage(Sovereignity.FactionWarefareOccupyingFactionId);
                    }
                    //return GetFactionImage(Sovereignity.OwningFactionId);
                }
                return null;
            }
        }

        //public object FactionWarfareImage => Sovereignity.IsFactionWarefareSystem
        //                                         ? Application.Current.MainWindow.FindResource("FactionWarfareImage")
        //                                         : null;

        public Visibility FactionwareImageVisibility => Sovereignity.IsFactionWarefareSystem ? Visibility.Visible : Visibility.Collapsed;

        public int JumpCount { get; set; }

        public SovereignityData Sovereignity { get; set; }

        public bool IsWormholeSystem { get; set; }

        public bool IsLowSec => SecurityStatus < 0.45 && SecurityStatus > 0;

        public bool IsNullSec => SecurityStatus <= 0;

        public Brush CircleBorderColor
        {
            get { return _circleBorderColor; }
            set
            {
                if (Equals(value, _circleBorderColor))
                {
                    return;
                }
                _circleBorderColor = value;
                OnPropertyChanged();
            }
        }

        public Brush InnerCircleColor
        {
            get { return _innerCircleColor; }
            set
            {
                if (Equals(value, _innerCircleColor))
                {
                    return;
                }
                _innerCircleColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int GetSecurityType()
        {
            return SecurityStatus <= 0.0 ? 0 : (SecurityStatus < 0.5 ? 1 : 2);
        }

        public override string ToString()
        {
            return Name + " (" + ID + ")";
        }

        private void UpdateColors()
        {
            OnPropertyChanged(NotifyUtils.GetPropertyName((SolarSystemViewModel x) => x.ForegroundColor));
            OnPropertyChanged(NotifyUtils.GetPropertyName((SolarSystemViewModel x) => x.BackgroundColor));
        }

        private object GetFactionImage(long factionID)
        {
            var mainWindow = Application.Current.MainWindow;
            switch (factionID)
            {
                case 500001:
                    return mainWindow.FindResource("Caldari32Image");
                case 500002:
                    return mainWindow.FindResource("Minmatar32Image");
                case 500003:
                    return mainWindow.FindResource("Amarr32Image");
                case 500004:
                    return mainWindow.FindResource("Gallente32Image");
                default:
                    return null;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class SolarSystemEqualityComparer : IEqualityComparer<SolarSystemViewModel>
        {
            public bool Equals(SolarSystemViewModel x, SolarSystemViewModel y)
            {
                return x.ID == y.ID;
            }

            public int GetHashCode(SolarSystemViewModel obj)
            {
                return obj.ID;
            }
        }
    }

    public class WormholeEffect
    {
        public string EffectedStat { get; set; }
        public string EffectValue { get; set; }
    }

    public class WormholeInfo
    {
        public WormholeInfo()
        {
            Effects = new List<WormholeEffect>();
        }

        public string Class { get; set; }
        public string Anomaly { get; set; }
        public string Static1 { get; set; }
        public string Static2 { get; set; }
        public IList<WormholeEffect> Effects { get; set; }
    }
}