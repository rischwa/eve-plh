using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Map.Statistics.Palettes;
using EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators;

namespace EveLocalChatAnalyser.Ui.Settings
{
    public sealed class SystemExampleModel : INotifyPropertyChanged, IDisposable
    {
        private readonly int _index;
        private Brush _innerCircleColor;
        private Brush _circleBorderColor;
        private Visibility _triStateVisibility;

        public SystemExampleModel(int index)
        {
            _index = index;
            Properties.Settings.Default.PropertyChanged += DefaultOnPropertyChanged;
            CalculateProperties();
        }

        private void DefaultOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            CalculateProperties();
        }

        private void CalculateProperties()
        {
            var circleBorderPalette = PaletteCollection.GetByName(Properties.Settings.Default.MapCircleBorderPalette)
                .Palette;
            var innerCirclePalette = PaletteCollection.GetByName(Properties.Settings.Default.MapInnerCirclePalette)
                .Palette;
            CircleBorderColor = circleBorderPalette[_index];
            InnerCircleColor = innerCirclePalette[_index];
            TriStateVisibility = Properties.Settings.Default.MapCircleMarkerType == typeof (NoTriStateCalculator).Name
                                     ? Visibility.Collapsed
                                     : Visibility.Visible;
        }

        public Visibility TriStateVisibility
        {
            get { return _triStateVisibility; }
            set
            {
                if (value == _triStateVisibility)
                {
                    return;
                }
                _triStateVisibility = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Properties.Settings.Default.PropertyChanged -= DefaultOnPropertyChanged;
        }
    }
}