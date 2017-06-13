using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using EveLocalChatAnalyser.Utilities.GateCampDetection;
using EVE_Killboard_Analyser.Helper.Gatecamp;
using PLHLib;
using Brushes = EveLocalChatAnalyser.Ui.Models.Brushes;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    public interface IMapGateCamps : IDisposable
    {
        void AddConnections(IEnumerable<SolarSystemConnection> connections);

        void Clear();
    }

    public class MapGateCamps : IMapGateCamps
    {
        private readonly IGateCampDetectionService _gateCampDetectionService;
        private readonly List<SolarSystemConnection> _connections = new List<SolarSystemConnection>();
        private static readonly GateCampPalette PALETTE = new GateCampPalette();

        public MapGateCamps(IGateCampDetectionService gateCampDetectionService)
        {
            _gateCampDetectionService = gateCampDetectionService;
            _gateCampDetectionService.GateCampAdded += GateCampDetectionServiceOnGateCampAdded;
            _gateCampDetectionService.GateCampRemoved += GateCampDetectionServiceOnGateCampRemoved;
            _gateCampDetectionService.GateCampIndexChanged += GateCampDetectionServiceOnGateCampIndexChanged;
        }

        private void GateCampDetectionServiceOnGateCampIndexChanged(GateCampMessageModel gateCamp)
        {
            
          
                UpdateValues();
        }

        private void GateCampDetectionServiceOnGateCampRemoved(GateCampMessageModel gateCamp)
        {
           
                UpdateValues();
        }

        private void GateCampDetectionServiceOnGateCampAdded(GateCampMessageModel gateCamp)
        {
            UpdateValues();
        }

        public void AddConnections(IEnumerable<SolarSystemConnection> connections)
        {
            _connections.AddRange(connections);
            UpdateValues();
        }

        private void UpdateValues()
        {
            var gateCamps = _gateCampDetectionService.GateCamps;
            //TODO approach is naiv and slow, just index connections by id
            foreach (var solarSystemConnection in _connections)
            {
                var gateCampIndex = GetGateCampIndex(solarSystemConnection, gateCamps);
                solarSystemConnection.GateCampIndex = gateCampIndex;
                solarSystemConnection.FinalStrokeColor = gateCampIndex > 0.3 ? PALETTE[gateCampIndex] : solarSystemConnection.StrokeColor;
            }
        }

        private double GetGateCampIndex(SolarSystemConnection solarSystemConnection, IEnumerable<GateCampMessageModel> gateCamps)
        {
            var locations = new[] {new StargateLocation
                                   {
                                       SolarSystemID1 = solarSystemConnection.Source.ID,
                                       SolarSystemID2 = solarSystemConnection.Target.ID
                                   }};
            var gateCamp = gateCamps.FirstOrDefault(x => x.StargateLocations.HasIntersection(locations));
            return gateCamp != null ? gateCamp.GateCampIndex : 0;
        }

        public void Clear()
        {
            _connections.Clear();
        }

        public void Dispose()
        {
            _gateCampDetectionService.GateCampAdded -= GateCampDetectionServiceOnGateCampAdded;
            _gateCampDetectionService.GateCampRemoved -= GateCampDetectionServiceOnGateCampRemoved;
            _gateCampDetectionService.GateCampIndexChanged -= GateCampDetectionServiceOnGateCampIndexChanged;
        }
    }

    public static class ColorConversion
    {
        public static Color FromHex(string hexValue)
        {
            var startIndex = hexValue.StartsWith("#") ? 1 : 0;
            var r = Convert.ToByte(hexValue.Substring(startIndex, 2), 16);
            var g = Convert.ToByte(hexValue.Substring(startIndex + 2, 2), 16);
            var b = Convert.ToByte(hexValue.Substring(startIndex + 4, 2), 16);
            return new Color
                   {
                       R = r,
                       G = g,
                       B = b,
                       A = 255
                   };
        }
    }

    public sealed class GateCampPalette
    {
        private static readonly SolidColorBrush[] BRUSHES =
        {
            Brushes.SolidLightGrayBrush,
            new SolidColorBrush(ColorConversion.FromHex("8C7373")),
            new SolidColorBrush(ColorConversion.FromHex("996666")),
            new SolidColorBrush(ColorConversion.FromHex("A65959")),
            new SolidColorBrush(ColorConversion.FromHex("BF4040")),
            new SolidColorBrush(ColorConversion.FromHex("CC3333")),
            new SolidColorBrush(ColorConversion.FromHex("D92626")),
            new SolidColorBrush(ColorConversion.FromHex("E61919")),
            new SolidColorBrush(ColorConversion.FromHex("F20D0D")),
            new SolidColorBrush(ColorConversion.FromHex("D92626")),
            new SolidColorBrush(ColorConversion.FromHex("FF0000"))
        };

        static GateCampPalette()
        {
            foreach (var solidColorBrush in BRUSHES)
            {
                solidColorBrush.Freeze();
            }
        }

        public Brush this[double gateCampIndex]
        {
            get
            {
                if (gateCampIndex > 0.9)
                {
                    return BRUSHES[9];
                }
                if (gateCampIndex > 0.8)
                {
                    return BRUSHES[8];
                }
                if (gateCampIndex > 0.7)
                {
                    return BRUSHES[7];
                }
                if (gateCampIndex > 0.6)
                {
                    return BRUSHES[6];
                }
                if (gateCampIndex > 0.5)
                {
                    return BRUSHES[3];
                }
                if (gateCampIndex > 0.4)
                {
                    return BRUSHES[2];
                }
                if (gateCampIndex > 0.3)
                {
                    return BRUSHES[1];
                }
                return BRUSHES[0];
            }
        }
    }
}
