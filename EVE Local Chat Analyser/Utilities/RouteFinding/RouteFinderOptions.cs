using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EveLocalChatAnalyser.Properties;

namespace EveLocalChatAnalyser.Utilities.RouteFinding
{
    public interface IRouteFinderOptions
    {
        bool IsAvoidingSystems { get; set; }
        RouteType RouteType { get; set; }
        int SecurityPenality { get; set; }
        ISet<int> SystemIdsToAvoid { get; set; }
        bool IsIgnoringWormholes { get; set; }
    }

    public class RouteFinderOptions : IRouteFinderOptions
    {
        private int _securityPenality = 50;
        private ISet<int> _systemIdsToAvoid = new HashSet<int>();
        public bool IsAvoidingSystems { get; set; }

        public RouteType RouteType { get; set; }

        public int SecurityPenality
        {
            get { return _securityPenality; }
            set
            {
                if (value < 1 || value > 100)
                {
                    throw new ArgumentException("SecurityPenality has to be between 1 and 100, but you tried to set it to " + value);
                }
                _securityPenality = value;
            }
        }

        public ISet<int> SystemIdsToAvoid
        {
            get { return _systemIdsToAvoid; }
            set { _systemIdsToAvoid = value ?? new HashSet<int>(); }
        }

        public bool IsIgnoringWormholes { get; set; }
    }

    public class SettingsBasedRouteFinderOptions : IRouteFinderOptions, INotifyPropertyChanged
    {
        private RouteType? _routeType;

        public bool IsAvoidingSystems { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public RouteType RouteType
        {
            get
            {
                if (_routeType == null)
                {
                    _routeType = (RouteType) Enum.Parse(typeof(RouteType), Settings.Default.RouteFinderRouteType);    
                }
                
                return _routeType.Value;
            }
            set {
                if (value == _routeType)
                {
                    return;
                }
                _routeType = value;
                Settings.Default.RouteFinderRouteType = value.ToString();
                OnPropertyChanged();
            }
        }

        public int SecurityPenality { get { return Settings.Default.RouteFinderSecurityPenalty; } set { Settings.Default.RouteFinderSecurityPenalty = value; OnPropertyChanged(); } }

        public ISet<int> SystemIdsToAvoid { get; set; }
        public bool IsIgnoringWormholes { get { return Settings.Default.RouteFinderIsIgnoringWormholes; } set { Settings.Default.RouteFinderIsIgnoringWormholes = value; OnPropertyChanged(); } }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
