using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace EveLocalChatAnalyser.Utilities.RouteFinding
{
    public interface IStaticUniverseData
    {
        ILookup<int, int> AllConnections { get; }
        IEnumerable<StaticSolarSystemInfo> AllSystems { get; }

        StaticSolarSystemInfo GetSystemByName(string name);
    }

    //TODO extract stuff from UniverseDataDB into here
    public class StaticUniverseData : IStaticUniverseData
    {
        private static readonly SQLiteConnection DB_CONNECTION;

        private static readonly Lazy<IEnumerable<StaticSolarSystemInfo>> ALL_SYSTEMS;

        private static readonly MySQLiteCommand<StaticSolarSystemInfo> GET_SYSTEMS_COMMAND;
        private static readonly MySQLiteCommand<KeyValuePair<int, int>> GET_CONNECTIONS_COMMAND;
        private static readonly Lazy<ILookup<int, int>> ALL_CONNECTIONS;
        private static readonly Lazy<IDictionary<string, StaticSolarSystemInfo>> SYSTEMS_BY_NAME;

        static StaticUniverseData()
        {
            DB_CONNECTION = new SQLiteConnection(@"data source=./Resources/plh_universe_data.sqlite3;Read Only=True");
            DB_CONNECTION.Open();

            GET_SYSTEMS_COMMAND = new MySQLiteCommand<StaticSolarSystemInfo>(DB_CONNECTION,
                                                                             "SELECT SystemID, SystemName, Security FROM System",
                                                                             MapToSystem);

            ALL_SYSTEMS = new Lazy<IEnumerable<StaticSolarSystemInfo>>(GET_SYSTEMS_COMMAND.Execute);

            GET_CONNECTIONS_COMMAND = new MySQLiteCommand<KeyValuePair<int, int>>(DB_CONNECTION,
                                                                                  "SELECT * FROM SystemConnection",
                                                                                  MapToConnection);
            ALL_CONNECTIONS = new Lazy<ILookup<int, int>>(GetConnections);

            SYSTEMS_BY_NAME = new Lazy<IDictionary<string, StaticSolarSystemInfo>>(() => ALL_SYSTEMS.Value.ToDictionary(x => x.Name, x => x));
        }

        public ILookup<int, int> AllConnections
        {
            get { return ALL_CONNECTIONS.Value; }
        }

        public IEnumerable<StaticSolarSystemInfo> AllSystems
        {
            get { return ALL_SYSTEMS.Value; }
        }

        public StaticSolarSystemInfo GetSystemByName(string name)
        {
            return SYSTEMS_BY_NAME.Value[name];
        }

        private static ILookup<int, int> GetConnections()
        {
            return GET_CONNECTIONS_COMMAND.Execute()
                                          .ToLookup(x => x.Key, x => x.Value);
        }

        private static KeyValuePair<int, int> MapToConnection(SQLiteDataReader arg)
        {
            return new KeyValuePair<int, int>(arg.GetInt32(0), arg.GetInt32(1));
        }

        private static StaticSolarSystemInfo MapToSystem(SQLiteDataReader arg)
        {
            return new StaticSolarSystemInfo
                       {
                           Id = arg.GetInt32(0),
                           Name = arg.GetString(1),
                           SecurityStatus = arg.GetDouble(2)
                       };
        }
    }
}
