using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Ui.Models;
using GraphX.Controls;
using GraphX.PCL.Common.Models;
using Brushes = EveLocalChatAnalyser.Ui.Models.Brushes;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class SolarSystemConnection : EdgeBase<SolarSystemViewModel>, INotifyPropertyChanged
    {
        private bool _isPartOfRoute;
        private Brush _finalStrokeColor;
        private double _gateCampIndex;

        public SolarSystemConnection() : base(null, null, -1)
        {
        }

        public SolarSystemConnection(SolarSystemViewModel source, SolarSystemViewModel target, double weight = 1)
            : base(source, target, source.RegionID != target.RegionID ? 0.2 : 1)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual SystemConnectionType SystemConnectionType
        {
            get { return SystemConnectionType.Normal; }
        }

        public string SystemConnectionTypeStr
        {
            get { return SystemConnectionType.ToString(); }
        }

        public virtual string ToolTip
        {
            get { return "Gate Camp Index: " + GateCampIndex.ToString("F"); }
        }

        public double GateCampIndex
        {
            get { return _gateCampIndex; }
            set
            {
                _gateCampIndex = value;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(ToolTip));
            }
        }

        public bool IsPartOfRoute
        {
            get { return _isPartOfRoute; }
            set
            {
                if (value.Equals(_isPartOfRoute))
                    return;
                _isPartOfRoute = value;
                OnPropertyChanged();
            }
        }

        public bool IsInterRegional => Source.RegionID != Target.RegionID;

        public EdgeDashStyle DashStyle => IsInterRegional && !Source.IsWormholeSystem && !Target.IsWormholeSystem ? EdgeDashStyle.Dash : EdgeDashStyle.Solid;

        public Brush FinalStrokeColor
        {
            get { return _finalStrokeColor; }
            set {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (_finalStrokeColor == value)
                {
                    return;

                }
                _finalStrokeColor = value;
                OnPropertyChanged();
            }
        }

        public virtual Brush StrokeColor => IsPartOfRoute ? Brushes.SolidBlueBrush : Brushes.SolidLightGrayBrush;

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