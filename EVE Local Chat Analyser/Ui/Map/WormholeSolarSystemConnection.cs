using System;
using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using Humanizer;
using Brushes = EveLocalChatAnalyser.Ui.Models.Brushes;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class WormholeSolarSystemConnection : SolarSystemConnection
    {
        private WormholeConnection _wormholeConnection;

        public WormholeSolarSystemConnection(SolarSystemViewModel first, SolarSystemViewModel second, WormholeConnection whConnection)
            : base(first, second)
        {
            _wormholeConnection = whConnection;
        }

        public string MaxEndOfLifeStr
        {
            get
            {
                var maxEOL = _wormholeConnection.TimeOfFirstSighting + new TimeSpan(24, 0, 0);
                return IsCritical
                           ? DateTimeUtilities.Min(_wormholeConnection.LastLifetimeUpdate.Time, maxEOL)
                                              .Humanize()
                           : maxEOL.Humanize();
            }
        }

        public override Brush StrokeColor
        {
            get { return IsCritical ? Brushes.SolidRedBrush : (IsPartOfRoute ? Brushes.SolidBlueBrush : Brushes.SolidGrayBrush); }
        }

        public override SystemConnectionType SystemConnectionType
        {
            get { return SystemConnectionType.Wormhole; }
        }

        public override string ToolTip
        {
            get
            {
                return string.Format("{0}: {1}\n{2}: {3}\n\nExpected EOL: {4}\nFirst sighting: {5}",
                                     WormholeConnection.FirstSystem,
                                     WormholeConnection.FirstToSecondSignature,
                                     WormholeConnection.SecondSystem,
                                     WormholeConnection.SecondToFirstSignature,
                                     WormholeConnection.MaxEndOfLife.Humanize(),
                                     WormholeConnection.TimeOfFirstSighting.ToStringDefault());
            }
        }

        public WormholeConnection WormholeConnection
        {
            get { return _wormholeConnection; }
            set
            {
                _wormholeConnection = value;
                OnPropertyChanged();
            }
        }

        private bool IsCritical
        {
            get { return _wormholeConnection.LastLifetimeUpdate.LifetimeStatus == WormholeLifetimeStatus.Critical; }
        }
    }
}