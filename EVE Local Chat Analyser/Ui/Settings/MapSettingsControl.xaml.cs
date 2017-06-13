using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EveLocalChatAnalyser.Ui.Map.Statistics;
using EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators;
using EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators;

namespace EveLocalChatAnalyser.Ui.Settings
{
    public class MapSettingsViewModel
    {
        
    }

    /// <summary>
    /// Interaction logic for MapSettingsControl.xaml
    /// </summary>
    public partial class MapSettingsControl : UserControl
    {
        public MapSettingsControl()
        {

            InnerCirclePaletteModel =
                new PaletteSelectionViewModel(Utilities.NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapInnerCirclePalette));
            OuterCirclePaletteModel =
                new PaletteSelectionViewModel(Utilities.NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapCircleBorderPalette));
            InnerCircleIndexModel =
                new SettingsViewModel(
                    Utilities.NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapInnerCircleColorType),
                    IndexCalculatorCollection.GetViewModel);
            OuterCircleIndexModel = new SettingsViewModel(
                Utilities.NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapCircleBorderType),
                IndexCalculatorCollection.GetViewModel);

            TriStateSettingsViewModel =
                new SettingsViewModel(
                    Utilities.NotifyUtils.GetPropertyName((Properties.Settings s) => s.MapCircleMarkerType),
                    TriStateCalculatorCollection.GetViewModel);

            InitializeComponent();

            DataContext = this;
        }

        public SettingsViewModel TriStateSettingsViewModel { get; set; }

        public SettingsViewModel OuterCircleIndexModel { get; set; }

        public SettingsViewModel InnerCircleIndexModel { get; set; }

        public PaletteSelectionViewModel OuterCirclePaletteModel { get; set; }

        public PaletteSelectionViewModel InnerCirclePaletteModel { get; set; }
    }
}
