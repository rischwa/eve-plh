using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators;
using EveLocalChatAnalyser.Ui.Map.Statistics.Palettes;
using EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    public class MapStatistics : IDisposable
    {
        private static readonly string MAP_CIRCLE_BORDER_PALETTE_PROPERTY_NAME =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapCircleBorderPalette);

        private static readonly string MAP_INNVER_CIRCLE_COLOR_PROPERTY_NAME =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapInnerCircleColorType);

        private static readonly string MAP_INNER_CIRCLE_PALETTE_PROPERTY_NAME =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapInnerCirclePalette);

        private static readonly string MAP_CIRCLE_BORDER_TYPE_PROPERTY_NAME =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapCircleBorderType);

        private static readonly string MAP_TRI_STATE_TYPE_PROPERTY_NAME =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapCircleMarkerType);

        private static readonly string MAP_TRI_STATE_COLOR_PROPERTY_NAME =
            NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapTriStateColor);

        private readonly List<SolarSystemViewModel> _solarSystems = new List<SolarSystemViewModel>();

        public MapStatistics()
        {
            Properties.Settings.Default.PropertyChanged += DefaultOnPropertyChanged;
        }

        public void Dispose()
        {
            Properties.Settings.Default.PropertyChanged -= DefaultOnPropertyChanged;
        }

        private void DefaultOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var propertyName = propertyChangedEventArgs.PropertyName;
            if (propertyName == MAP_CIRCLE_BORDER_PALETTE_PROPERTY_NAME || propertyName == MAP_INNVER_CIRCLE_COLOR_PROPERTY_NAME
                || propertyName == MAP_INNER_CIRCLE_PALETTE_PROPERTY_NAME || propertyName == MAP_CIRCLE_BORDER_TYPE_PROPERTY_NAME
                || propertyName == MAP_TRI_STATE_COLOR_PROPERTY_NAME || propertyName == MAP_TRI_STATE_TYPE_PROPERTY_NAME)
            {
                UpdateBySettings();
            }
        }

        public void AddSystems(IEnumerable<SolarSystemViewModel> solarSystems)
        {
            //TODO update only new systems
            _solarSystems.AddRange(solarSystems);
            UpdateStatistics();
        }

        public void Clear()
        {
            _solarSystems.Clear();
        }

        private void UpdateStatistics()
        {
            UpdateBySettings();
        }

        private void UpdateBySettings()
        {
            UpdateInnerCircleColor();
            UpdateCircleBorderColor();
            UpdateCircleMarker();
        }

        private void UpdateCircleMarker()
        {
            var markerCalculator = TriStateCalculatorCollection.CreateByName(Properties.Settings.Default.MapCircleMarkerType);
            foreach (var curSystem in _solarSystems)
            {
                var state = markerCalculator.GetState(curSystem);
                switch (state)
                {
                    case 2:
                        curSystem.CrossLine1Visibility = Visibility.Visible;
                        curSystem.CrossLine2Visibility = Visibility.Visible;
                        break;
                    case 1:
                        curSystem.CrossLine1Visibility = Visibility.Visible;
                        curSystem.CrossLine2Visibility = Visibility.Collapsed;
                        break;
                    default:
                        curSystem.CrossLine1Visibility = Visibility.Collapsed;
                        curSystem.CrossLine2Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void UpdateCircleBorderColor()
        {
            var indexCalculator = IndexCalculatorCollection.CreateByName(Properties.Settings.Default.MapCircleBorderType);
            var palette = PaletteCollection.GetByName(Properties.Settings.Default.MapCircleBorderPalette)
                .Palette;
            var colorCalculator = new IndexColorConverter(indexCalculator, palette);

            foreach (var curSystem in _solarSystems)
            {
                curSystem.CircleBorderColor = colorCalculator.GetBrush(curSystem);
            }
        }

        private void UpdateInnerCircleColor()
        {
            var indexCalculator = IndexCalculatorCollection.CreateByName(Properties.Settings.Default.MapInnerCircleColorType);
            var palette = PaletteCollection.GetByName(Properties.Settings.Default.MapInnerCirclePalette)
                .Palette;
            var colorCalculator = new IndexColorConverter(indexCalculator, palette);

            foreach (var curSystem in _solarSystems)
            {
                curSystem.InnerCircleColor = colorCalculator.GetBrush(curSystem);
            }
        }

        public void Update()
        {
            UpdateStatistics();
        }
    }
}
