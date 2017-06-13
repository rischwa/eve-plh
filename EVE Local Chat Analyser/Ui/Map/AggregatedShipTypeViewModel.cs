using System.Windows.Media;
using EveLocalChatAnalyser.Model;
using EveLocalChatAnalyser.Utilities;
using Brushes = EveLocalChatAnalyser.Ui.Models.Brushes;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class AggregatedShipTypeViewModel
    {
        private static readonly IShipClassifications SHIP_CLASSIFICATIONS =
            DIContainer.GetInstance<IShipClassifications>();

        //TODO category kann von aussen gesetzt werden, dann brauchen wir das hier nicht

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Category = SHIP_CLASSIFICATIONS.GetShipCategoryFor(value);
            }
        }

        public int Count { get; set; }
        public ShipCategory Category { get; private set; }

        public int MinDps
        {
            get { return Category.MinDps*Count; }
        }

        public int MaxDps
        {
            get { return Category.MaxDps*Count; }
        }

        public int Rep
        {
            get { return Category.Rep*Count; }
        }

        public string ShipGroup
        {
            get { return Category.Group; }
        }

        public string FleetRole
        {
            get { return Category.Category; }
        }

        public SolidColorBrush FleetRoleForeground
        {
            get
            {
                switch (FleetRole)
                {
                    case "Fancy":
                        return Brushes.SolidWhiteBrush;
                    default:
                        return Brushes.SolidBlackBrush;
                }
            }
        }

        public SolidColorBrush FleetRoleBackground
        {
            get
            {
                switch (FleetRole)
                {
                    case "Capitals":
                        return ShipTypePalette.CAPITALS;
                    case "DPS":
                        return ShipTypePalette.DPS;
                    case "Tackle":
                        return ShipTypePalette.TACKLE;
                    case "Logi":
                        return ShipTypePalette.LOGISTICS;
                    case "EWAR":
                        return ShipTypePalette.EWAR;
                    case "Fancy":
                        return ShipTypePalette.FANCY;
                    default:
                        return ShipTypePalette.CIVILIAN;
                }
            }
        }

        public int GroupCount { get; set; }
    }
}