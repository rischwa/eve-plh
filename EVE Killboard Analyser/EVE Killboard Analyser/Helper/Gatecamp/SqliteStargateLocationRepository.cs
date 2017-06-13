using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using log4net;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.Gatecamp
{
    public class SqliteStargateLocationRepository : IStargateLocationRepository
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (SqliteStargateLocationRepository));
        private readonly ILookup<int, StargateLocation> _stargateLoations;

        public SqliteStargateLocationRepository()
        {
            _stargateLoations = LoadStargateLocations().ToLookup(x => x.SolarSystemID1, x => x);
        }

        private static IEnumerable<StargateLocation> LoadStargateLocations()
        {
            var stargateLocations = new List<StargateLocation>(14000);
            using (var dbConnection = new SQLiteConnection(@"data source=c:/inetpub/wwwroot/killboard/stargatelocations.sqlite;Read Only=True"))
            {
                dbConnection.Open();
                var cmd = dbConnection.CreateCommand();
                cmd.CommandText = "SELECT SolarSystemID1, SolarSystemID2, X, Y, Z FROM StargateLocations";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stargateLocations.Add(
                                              new StargateLocation
                                              {
                                                  SolarSystemID1 = reader.GetInt32(0),
                                                  SolarSystemID2 = reader.GetInt32(1),
                                                  Position = new Vector3(reader.GetDouble(2), reader.GetDouble(3), reader.GetDouble(4))
                                              });
                    }
                }
            }
            LOGGER.Debug($"Loaded {stargateLocations.Count} locations");
            return stargateLocations;
        }

        public bool TryGetStargateLocation(int solarSystemID, Position pos, out StargateLocation location)
        {
            // ReSharper disable PossibleInvalidOperationException
            if (pos?.X == null)
            {
                location = null;
                return false;
            }
            var posAsVector = new Vector3((float) pos.X, (float) pos.Y, (float) pos.Z);
            var locationsOfSystem = _stargateLoations[solarSystemID];
            foreach (var curLocation in locationsOfSystem)
            {
                if (curLocation.IsInRange(posAsVector))
                {
                    location = curLocation;
                    return true;
                }
            }

            location = null;
            return false;
        }
    }
}