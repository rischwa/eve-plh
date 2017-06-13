using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace EveLocalChatAnalyser.Model
{
    public class ShipCategory
    {
        public string Group { get; set; }
        public string Category { get; set; }
        public int GroupPriority { get; set; }
        public int Rep { get; set; }
        public int MinDps { get; set; }
        public int MaxDps { get; set; }
    }

    public interface IShipClassifications
    {
        ShipCategory GetShipCategoryFor(string shipTypeName);
    }

    public class ShipClassifications : IShipClassifications
    {
        private static readonly Dictionary<string, ShipCategory> SHIP_NAME_TO_CATEGORY;

        public static readonly ShipCategory UNKNOWN_CATEGORY = new ShipCategory
            {
                Category = "unknown",
                Group = "unknown",
                MinDps = 0,
                MaxDps = 0,
                Rep = 0,
                GroupPriority = 100
            };

        static ShipClassifications()
        {
            SHIP_NAME_TO_CATEGORY = new Dictionary<string, ShipCategory>();
            using (
                var dbConnection =
                    new SQLiteConnection(@"data source=./Resources/plh_ship_categorization.sqlite3;Read Only=True"))
            {
                dbConnection.Open();
                var command = dbConnection.CreateCommand();
                command.CommandText =
                    ("SELECT TypeName, GroupName, CategoryName, MinDPS, MaxDPS, Rep, Priority FROM Ship JOIN [Group] ON Ship.GroupID = [Group].GroupID JOIN Category ON [Group].CategoryID = Category.CategoryID");

                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        SHIP_NAME_TO_CATEGORY[reader.GetString(0)] = new ShipCategory
                            {
                                Group = reader.GetString(1),
                                Category = reader.GetString(2),
                                MinDps = reader.GetInt32(3),
                                MaxDps = reader.GetInt32(4),
                                Rep = reader.GetInt32(5),
                                GroupPriority = reader.GetInt32(6)
                            };
                    }
                }
            }
        }

        public ShipCategory GetShipCategoryFor(string shipTypeName)
        {
            if (shipTypeName.EndsWith(" Edition"))
            {
                shipTypeName = shipTypeName.TakeWhile(x => x != ' ').ToString();
            }
            ShipCategory result;
            return  SHIP_NAME_TO_CATEGORY.TryGetValue(shipTypeName, out result) ? result : UNKNOWN_CATEGORY;
        }
    }
}
