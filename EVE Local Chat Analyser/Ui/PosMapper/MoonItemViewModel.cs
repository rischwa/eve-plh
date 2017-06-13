using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using EveLocalChatAnalyser.Utilities.PosMapper;
using Brushes = EveLocalChatAnalyser.Ui.Models.Brushes;

namespace EveLocalChatAnalyser.Ui.PosMapper
{
    public class MoonItemViewModel
    {
        private static readonly string[] EWAR_BATTERY_ENDINGS = new[]
            {
                "Energy Neutralizing Battery",
                "Ion Field Projection Battery",
                "Phase Inversion Battery",
                "Sensor Dampening Battery",
                "Spatial Destabilization Battery",
                "Stasis Webification Battery",
                "Warp Disruption Battery",
                "Warp Scrambling Battery",
                "White Noise Generation Battery"
            };

        private static readonly ISet<String> LOOT_TYPES = new HashSet<string>
            {
                "Equipment Assembly Array",
                "Rapid Equipment Assembly Array",
                "Starbase Major Assembly Array",
                "Starbase Minor Assembly Array",
                "Small Ship Assembly Array",
                "Supercapital Ship Assembly Array",
                "Advanced Small Ship Assembly Array",
                "Medium Ship Assembly Array",
                "Advanced Medium Ship Assembly Array",
                "Capital Ship Assembly Array",
                "Advanced Large Ship Assembly Array",
                "Ammunition Assembly Array",
                "Drone Assembly Array",
                "Component Assembly Array",
                "Starbase Major Assembly Array",
                "Starbase Major Assembly Array",
                "Large Ship Assembly Array",
                "Subsystem Assembly Array",
                "Thukker Component Assembly Array",
                "Corporate Hangar Array",
                "Personal Hangar Array",
                "Research Laboratory",
                "Station Laboratory",
                "Experimental Laboratory",
                "Amarr Advanced Outpost Laboratory",
                "Amarr Basic Outpost Laboratory",
                "Amarr Outpost Laboratory",
                "Caldari Advanced Outpost Laboratory",
                "Caldari Basic Outpost Laboratory",
                "Caldari Outpost Laboratory",
                "Gallente Advanced Outpost Laboratory",
                "Gallente Basic Outpost Laboratory",
                "Gallente Outpost Laboratory",
                "Minmatar Advanced Outpost Laboratory",
                "Minmatar Basic Outpost Laboratory",
                "Minmatar Outpost Laboratory",
                "Design Laboratory",
                "Hyasyoda Research Laboratory",
                "Ship Maintenance Array",
                "X-Large Ship Maintenance Array",
                "Gas/Storage Silo",
                "Silo",
                "Pressure Silo",
                "Ultra Fast Silo",
                "Gas/Storage Silo",
                "Storage Silo",
                "Storage Silo",
                "Storage Silo",
                "Storage Silo",
                "COSMOS Storage Silo",
                "Gas/Storage Silo",
                "Storage Silo",
                "Biochemical Silo",
                "Catalyst Silo",
                "Hazardous Chemical Silo",
                "Starbase Silo",
                "Starbase Ultra-Fast Silo",
                "Starbase Moon Mining Silo",
                "Expanded Silo",
                "Talocan Extraction Silo",
                "Hollow Talocan Extraction Silo",
                "Hybrid Polymer Silo"
            };

        private static readonly ISet<String> EXCLUDED_TYPES = new HashSet<string>
            {
                "Asteroid Belt",
                "Asteroidengürtel",
                "Interbus Customs Office",
                "Customs Office", //TODO german
                "Moon",
                "Mond",
                "Sun",
                "Sonne"
            };

        public MoonItemViewModel(MoonItem item)
        {
            MoonName = item.Moon.Name;
            HasForceField = item.ItemsOnScan.Any(PosMappingUtils.IsForceField);
            TowerStatusBackground = item.Tower == null
                                        ? Brushes.SolidWhiteBrush
                                        : (HasForceField ? Brushes.SolidGreenBrush : Brushes.SolidRedBrush);
            TowerName = item.Tower != null ? item.Tower.Name : "none";


            //TODO da sind jetzt schon welche gespeichert, die eigentlich excluded sind, entweder hier nochmal excluden, oder halt pech gehabt ... planet auch checken
            var aggregatedItemsOnScan =
                item.ItemsOnScan.Where(x => !EXCLUDED_TYPES.Contains(x.Type) && !x.IsStargate() && !x.IsTower())
                    .GroupBy(x => x.Type)
                    .Select(x => new AggregatedItem {Amount = x.Count(), Name = x.Key})
                    .OrderBy(x => x.Name).ToList();

            AggregatedItemsOnScan = aggregatedItemsOnScan; //
            TestAggregatedItems = new ListCollectionView(aggregatedItemsOnScan);
// ReSharper disable PossibleNullReferenceException
            TestAggregatedItems.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

// ReSharper restore PossibleNullReferenceException

            TowerStatus = item.Tower == null ? "" : (HasForceField ? "on" : "off");
            TowerType = item.Tower != null ? item.Tower.Type : "none";
            ScanTime = item.ScanTime;

            LootableModuleCount = aggregatedItemsOnScan.Where(x => LOOT_TYPES.Contains(x.Name)).Sum(x=>x.Amount);
        }

        public ListCollectionView TestAggregatedItems { get; set; }

        public int LootableModuleCount { get; private set; }

        public DateTime ScanTime { get; set; }

        public String ScanTimeString
        {
            get { return ScanTime.ToString("yyyy-MM-dd HH:mm"); }
        }

        public string TowerStatus { get; private set; }

        public Brush TowerStatusBackground { get; private set; }

        public string TowerName { get; private set; }

        public string TowerType { get; private set; }

        public string MoonName { get; private set; }

        public bool HasForceField { get; private set; }

        public List<AggregatedItem> AggregatedItemsOnScan { get; private set; }

        public class AggregatedItem
        {
            private string _name;
            public String Group { get; set; }

            public String Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    if (LOOT_TYPES.Contains(_name))
                    {
                        Group = "Lootable";
                        return;
                    }

                    Group =
                        Name.EndsWith("Battery")
                            ? (EWAR_BATTERY_ENDINGS.Any(x => Name.EndsWith(x)) ? "EWAR" : "Weapons")
                            : "General";
                }
            }

            public int Amount { get; set; }
        }
    }
}