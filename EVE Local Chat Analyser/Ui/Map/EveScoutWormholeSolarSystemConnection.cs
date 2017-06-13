using System;
using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities.PositionTracking;
using Humanizer;
using Brushes = EveLocalChatAnalyser.Ui.Models.Brushes;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class EveScoutWormholeSolarSystemConnection : SolarSystemConnection
    {
        private EveScoutWormholeConnection _whConnection;

        public EveScoutWormholeSolarSystemConnection(SolarSystemViewModel source,
                                                     SolarSystemViewModel target,
                                                     EveScoutWormholeConnection whConnection) : base(source, target)
        {
            _whConnection = whConnection;
        }

        public override Brush StrokeColor
        {
            get { return  IsCritical ? Brushes.SolidRedBrush : (IsPartOfRoute ? Brushes.SolidBlueBrush : Brushes.SolidOrchidBrush); }
        }

        public override SystemConnectionType SystemConnectionType
        {
            get { return SystemConnectionType.EveScoutWormhole; }
        }

        public override string ToolTip
        {
            get
            {
                return string.Format("{0}: {1}\n{2}: {3}\n\nExpected EOL: {4}\n\nStatus: {5}\nLast update: {6}\n\nprovided through http://eve-scout.com",
                                     WormholeConnection.FirstSystem,
                                     WormholeConnection.FirstToSecondSignature,
                                     WormholeConnection.SecondSystem,
                                     WormholeConnection.SecondToFirstSignature,
                                     WormholeConnection.MaxEndOfLife.Humanize(),
                                     WormholeConnection.Status,
                                     WormholeConnection.LastStatusUpdateStr);
            }
        }

        public EveScoutWormholeConnection WormholeConnection
        {
            get { return _whConnection; }
            set
            {
                _whConnection = value;
                OnPropertyChanged();
            }
        }

        private bool IsCritical
        {
            get { return DateTime.UtcNow.Add(new TimeSpan(1, 0, 0)) > _whConnection.MaxEndOfLife; }
        }
    }
}