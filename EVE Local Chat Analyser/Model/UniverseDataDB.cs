using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using EveLocalChatAnalyser.Services;
using EveLocalChatAnalyser.Ui.Map;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;
using EveLocalChatAnalyser.Utilities.PositionTracking;

namespace EveLocalChatAnalyser.Model
{
    //TODO das zusammensuchen der systeme muss aufgeraeumt werden, atm gibt es duplikate in zwischenschritten etc.
    //evtl. wird das durch die Trennung aber direkt besser
    public class UniverseDataDB
    {
        //TODO split sqlite/ravendb/eve-scout integrate through facade
        private static readonly SQLiteConnection DB_CONNECTION;
        private static SQLiteCommand _getAdjacentSystemsCommand;
        private static SQLiteParameter _getAdjacentSystemsSystemIDParameter;
        private static SQLiteCommand _getSystemByNameCommand;
        private static SQLiteParameter _getSystemByNameParameter;
        private static SQLiteCommand _getStationsCommand;
        private static SQLiteParameter _getStationsSystemIDParameter;
        private static SQLiteCommand _getSystemByIdCommand;
        private static SQLiteParameter _getSystemByIdIdParameter;
        private static SQLiteParameter _getAdjacentSystemsSystemMaxLevelParameter;
        private static SQLiteParameter _getAdjacentSystemsSystemSecurityTypeParameter;

        private static SQLiteCommand _getLowSecSystemsCommand;

        private static SQLiteCommand _getSystemsByRegionIdCommand;
        private static SQLiteParameter _getSystemByRegionIdRegionIdParameter;
        private static SQLiteParameter _getSystemByRegionIdSecurityLevelParameter;

        private static SQLiteCommand _getWormholeDataCommand;
        private static SQLiteParameter _getWormholeDataNameParameter;

        private static SQLiteCommand _getWormholeEffectsCommand;
        private static SQLiteParameter _getWormholeEffectsSystenNameParameter;
        private static readonly Lazy<string[]> ALL_SYSTEM_NAMES = new Lazy<string[]>(GetAllSystemNamesPrv);
        private static SQLiteCommand _areSystemsConnectedCommand;
        private static SQLiteParameter _areSystemsConnectedSystem1Parameter;
        private static SQLiteParameter _areSystemsConnectedSystem2Parameter;

        static UniverseDataDB()
        {
            DB_CONNECTION = new SQLiteConnection(@"data source=./Resources/plh_universe_data.sqlite3;Read Only=True");
            DB_CONNECTION.Open();

            InitGetSystemByNameCommand();
            InitGetSystemByIdCommand();

            InitGetSystemByRegionIdCommand();

            InitGetAdjacentSystemsCommand();

            InitGetStationCommand();

            InitGetWormholeDataCommand();

            InitGetWormholeEffectsCommand();

            InitAreSystemsConnectedCommand();


            _getLowSecSystemsCommand = DB_CONNECTION.CreateCommand();
            _getLowSecSystemsCommand.CommandText = "SELECT SystemID, SystemName, Security, RegionID FROM System WHERE Security < 0.45 AND SystemName <> 'Thera' AND NOT SystemName LIKE 'J0%'  AND NOT SystemName LIKE 'J1%'  AND NOT SystemName LIKE 'J2%'  AND NOT SystemName LIKE 'J3%'  AND NOT SystemName LIKE 'J4%'  AND NOT SystemName LIKE 'J5%'  AND NOT SystemName LIKE 'J6%'  AND NOT SystemName LIKE 'J7%'  AND NOT SystemName LIKE 'J8%'  AND NOT SystemName LIKE 'J9%' ";
            //TODO als service bereitstellen
        }

        public static string[] AllSystemNames
        {
            get { return ALL_SYSTEM_NAMES.Value.ToArray(); }
        }

        public static bool AreSystemsConnected(string system1, string system2)
        {
            if (system1 == system2)
            {
                return false;
            }

            _areSystemsConnectedSystem1Parameter.Value = system1;
            _areSystemsConnectedSystem2Parameter.Value = system2;

            var executeScalar = _areSystemsConnectedCommand.ExecuteScalar();
            var count = (long) executeScalar;
            if (count > 0)
            {
                return true;
            }

            var wormholeRepository = DIContainer.GetInstance<IWormholeConnectionRepository>();
            return wormholeRepository.GetWormholeConnection(system1, system2) != null;
        }

        public static ICollection<SolarSystemConnection> GetConnectionsBetweenSystems(IList<SolarSystemViewModel> allSystemsIn)
        {
            var allDistinctSystems = allSystemsIn.Distinct()
                                         .ToArray();
            var ids = string.Join(",", allDistinctSystems.Select(x => x.ID.ToString(CultureInfo.InvariantCulture)));

            var modelById = allDistinctSystems.ToDictionary(x => x.ID, x => x);

            var command = DB_CONNECTION.CreateCommand();
            command.CommandText =
                string.Format(
                              "SELECT FromSystem, ToSystem FROM SystemConnection WHERE ToSystem IN ({0}) AND FromSystem IN ({0}) AND ToSystem < FromSystem",
                              ids); // ToSystem < FromSystem to not get duplicates like A->B and B->A

            IList<SolarSystemConnection> connections = new List<SolarSystemConnection>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var curConnection = new SolarSystemConnection(modelById[reader.GetInt32(0)], modelById[reader.GetInt32(1)]);
                    connections.Add(curConnection);
                }
            }

            var whConnectionList = GetWormholeConnections(allDistinctSystems);
            var theraList = GetEveScoutWormholeConnections(allDistinctSystems);
             return connections.Concat(whConnectionList)
                              .Concat(theraList)
                              .ToList();

           // return connections;
        }

        public static IList<SolarSystemConnection> GetConnectionsBetweenSystemsWithout(List<SolarSystemViewModel> allSystemsIn,
                                                                                       IEnumerable<SolarSystemViewModel> without)
        {
            var allDistinctSystems = allSystemsIn.Distinct()
                                         .ToArray();
            var withoutList = without.ToArray();
            var ids = string.Join(",", allDistinctSystems.Select(x => x.ID.ToString(CultureInfo.InvariantCulture)));

            var modelById = allDistinctSystems.ToDictionary(x => x.ID, x => x);

            var command = DB_CONNECTION.CreateCommand();
            command.CommandText =
                string.Format(
                              "SELECT FromSystem, ToSystem FROM SystemConnection WHERE ToSystem IN ({0}) AND FromSystem IN ({0}) AND ToSystem < FromSystem AND NOT(ToSystem IN ({1}) AND FromSystem IN ({1}))",
                              ids,
                              string.Join(",", withoutList.Select(x => x.ID)));
            // ToSystem < FromSystem to not get duplicates like A->B and B->A

            IList<SolarSystemConnection> connections = new List<SolarSystemConnection>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var curConnection = new SolarSystemConnection(modelById[reader.GetInt32(0)], modelById[reader.GetInt32(1)]);
                    connections.Add(curConnection);
                }
            }

            var whConnectionList = GetWormholeConnectionsWithout(allDistinctSystems, withoutList);
            var theraList = GetEveScoutWormholeConnectionsWithout(allDistinctSystems, withoutList);

            return connections.Concat(whConnectionList)
                              .Concat(theraList)
                              .ToList();
        }
        //TODO alle methoden thread safe machen
        public static IList<SolarSystemViewModel> GetSurroundingSystemsFor(SolarSystemViewModel solarSystem, int level = 3)
        {
            if (solarSystem.IsWormholeSystem)
            {
                return GetSurroundingSystemsStartingFromWormhole(solarSystem);
            }

            var result = new List<SolarSystemViewModel>();
            _getAdjacentSystemsSystemSecurityTypeParameter.Value = solarSystem.GetSecurityType();
            _getAdjacentSystemsSystemMaxLevelParameter.Value = level;
            _getAdjacentSystemsSystemIDParameter.Value = solarSystem.ID;
            using (var reader = _getAdjacentSystemsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var curSystem = new SolarSystemViewModel
                                        {
                                            ID = reader.GetInt32(0),
                                            Name = reader.GetString(1),
                                            SecurityStatus = Convert.ToDouble(reader.GetDecimal(2)),
                                            RegionID = reader.GetInt32(3)
                };
                    curSystem.Stations = GetStationsForSystem(curSystem.ID);

                    result.Add(curSystem);
                }
            }

            var systems = result.Concat(GetSurroundingWormholeSystems(result.Concat(new[] {solarSystem})))
                                .Distinct()
                                .ToArray();

            return systems.Concat(GetSurroundingTheraSystems(systems))
                          .Distinct()
                          .ToArray();
        }

        public static IList<SolarSystemViewModel> GetRegionalSystemsFor(SolarSystemViewModel solarSystem)
        {
            if (solarSystem.IsWormholeSystem)
            {
                return new List<SolarSystemViewModel>();
            }
           
            _getSystemByRegionIdRegionIdParameter.Value = solarSystem.RegionID;
            //TODO allgemeine security conversion einfuehren
            _getSystemByRegionIdSecurityLevelParameter.Value = solarSystem.SecurityStatus > 0.45 ? 2 : (solarSystem.SecurityStatus > 0 ? 1 : 0);

            var result = new List<SolarSystemViewModel>();
            using (var reader = _getSystemsByRegionIdCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var curSystem = new SolarSystemViewModel
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        SecurityStatus = Convert.ToDouble(reader.GetDecimal(2)),
                        RegionID = reader.GetInt32(3)
                    };
                    curSystem.Stations = GetStationsForSystem(curSystem.ID);

                    result.Add(curSystem);
                }
            }

            return result;
        }

        public static SolarSystemViewModel GetSystemViewModelFor(string systemName)
        {
            if (systemName == null)
            {
                throw new ArgumentNullException("systemName", "systemName must not be null in GetSystemViewModelFor");
            }
            var result = new SolarSystemViewModel
                             {
                                 Name = systemName
                             };

            _getSystemByNameParameter.Value = systemName;
            using (var reader = _getSystemByNameCommand.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new Exception("Could not load system '" + systemName + "'");
                }
                result.SecurityStatus = Convert.ToDouble(reader.GetDecimal(1));

                var systemId = reader.GetInt32(0);
                result.ID = systemId;
                result.Stations = GetStationsForSystem(systemId);
                result.RegionID = reader.GetInt32(2);
            }

            if (WormholeConnectionTracker.WH_REGEX.IsMatch(systemName))
            {
                result.WormholeInfo = GetWormholeInfo(systemName);
            }

            return result;
        }

        private static SolarSystemConnection CreateEveScoutWormholeSystemConnection(IList<SolarSystemViewModel> allSystems,
                                                                                    EveScoutWormholeConnection eveScoutWormholeConnection)
        {
            var first = allSystems.FirstOrDefault(x => x.Name == eveScoutWormholeConnection.FirstSystem)
                        ?? GetSystemViewModelFor(eveScoutWormholeConnection.FirstSystem);

            var second = allSystems.FirstOrDefault(x => x.Name == eveScoutWormholeConnection.SecondSystem)
                         ?? GetSystemViewModelFor(eveScoutWormholeConnection.SecondSystem);

            return new EveScoutWormholeSolarSystemConnection(first, second, eveScoutWormholeConnection);
        }

        private static string[] GetAllSystemNamesPrv()
        {
            var result = new List<string>();
            using (var command = DB_CONNECTION.CreateCommand())
            {
                command.CommandText = "SELECT SystemName FROM System";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }
            return result.ToArray();
        }

        private static IEnumerable<SolarSystemConnection> GetEveScoutWormholeConnections(IEnumerable<SolarSystemViewModel> allSystems)
        {
            var service = DIContainer.GetInstance<IEveScoutService>();
            var connections = service.GetConnectionsToThera();

            var systemsWithThera = new HashSet<string>(connections.Where(x => x.FirstSystem != "Thera")
                                                                  .Select(x => x.FirstSystem)
                                                                  .Concat(connections.Where(x => x.SecondSystem != "Thera")
                                                                                     .Select(x => x.SecondSystem))
                                                                  .Concat(new[] {"Thera"}));

            var allSystemsWithTheraAdded = allSystems.ToList();
            if (allSystemsWithTheraAdded.Any(x => systemsWithThera.Contains(x.Name)))
            {
                var theraViewModel = GetSystemViewModelFor("Thera");

                var isMissingThera = allSystemsWithTheraAdded.All(x => x.Name != "Thera");
                if (isMissingThera)
                {
                    allSystemsWithTheraAdded.Add(theraViewModel);
                }

                return connections.Select(x => CreateEveScoutWormholeSystemConnection(allSystemsWithTheraAdded, x))
                                  .ToArray();
            }

            return new SolarSystemConnection[0];
        }

        private static IEnumerable<SolarSystemConnection> GetEveScoutWormholeConnectionsWithout(IEnumerable<SolarSystemViewModel> allSystems,
                                                                                                SolarSystemViewModel[] withoutList)
        {
            var withoutNames = new HashSet<string>(withoutList.Select(x => x.Name));
            return GetEveScoutWormholeConnections(allSystems)
                .Where(x => !withoutNames.Contains(x.Source.Name) || !withoutNames.Contains(x.Target.Name));
        }

        private static IList<Station> GetStationsForSystem(long systemId)
        {
            var stations = new List<Station>();
            _getStationsSystemIDParameter.Value = systemId;
            using (var reader = _getStationsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var curStation = new Station
                                         {
                                             Name = reader.GetString(0),
                                             HasRepairFacility = reader.GetBoolean(1)
                                         };

                    stations.Add(curStation);
                }
            }

            return stations;
        }

        private static IList<SolarSystemViewModel> GetSurroundingSystemsStartingFromWormhole(SolarSystemViewModel solarSystem)
        {
            var surroundingSystems = GetSurroundingWormholeSystems(new[] {solarSystem});
            const int ONLY_ONE_LEVEL_FOR_THERA = 0;
            var solarSystemViewModels = surroundingSystems as SolarSystemViewModel[] ?? surroundingSystems.ToArray();
            var result = solarSystemViewModels.Where(x => !x.IsWormholeSystem)
                .SelectMany(x => GetSurroundingSystemsFor(x, solarSystem.Name == "Thera" ? ONLY_ONE_LEVEL_FOR_THERA : 2)
                                                                .Concat(new[] {x})).Union(solarSystemViewModels.Where(x=>x.IsWormholeSystem))
                                           .Without(new[] {solarSystem})
                                           .Distinct()
                                           .ToList();

            var newSys = GetSurroundingTheraSystems(result.Concat(new[] {solarSystem}));

            return result.Concat(newSys)
                         .Without(new[] {solarSystem})
                         .Distinct()
                         .ToList();
        }

        private static IEnumerable<SolarSystemViewModel> GetSurroundingTheraSystems(IEnumerable<SolarSystemViewModel> systems)
        {
            var connections = GetEveScoutWormholeConnections(systems);
            var models = connections.Select(x => GetSystemViewModelFor(x.Source.Name == "Thera" ? x.Target.Name : x.Source.Name))
                                    .ToList();
            if (models.Any() && models.All(x => x.Name != "Thera"))
            {
                models.Add(GetSystemViewModelFor("Thera"));
            }

            return models;
        }

        private static IEnumerable<SolarSystemViewModel> GetSurroundingWormholeSystems(IEnumerable<SolarSystemViewModel> systems)
        {
            //TODO die klasse hier als service und das als dependency
            var systemsCopy = systems.ToList();
            var wormholeRepository = DIContainer.GetInstance<IWormholeConnectionRepository>();
            var result = new List<SolarSystemViewModel>();
            var systemStack = new Stack<SolarSystemViewModel>(systemsCopy);
            while (systemStack.Any())
            {
                var solarSystemViewModel = systemStack.Pop();

                var connections = wormholeRepository.GetWormholeConnectionsForSystem(solarSystemViewModel.Name);
                var curSystemName = solarSystemViewModel.Name;
                foreach (var curConnection in connections)
                {
                    var newSystemName = curConnection.FirstSystem == curSystemName ? curConnection.SecondSystem : curConnection.FirstSystem;

                    if (newSystemName == null || systemsCopy.Any(x => x.Name == newSystemName) || result.Any(x => x.Name == newSystemName))
                    {
                        continue;
                    }
                    
                    var newSystemViewModel = GetSystemViewModelFor(newSystemName);
                    result.Add(newSystemViewModel);

                    if (newSystemViewModel.IsWormholeSystem)
                    {
                        systemStack.Push(newSystemViewModel);
                    }
                }
            }

            var theraWhs = GetSurroundingTheraSystems(result.Concat(systems)
                                                            .Distinct()
                                                            .ToArray());

            return result.Concat(theraWhs)
                         .Distinct()
                         .ToArray();
        }

        private static IEnumerable<SolarSystemConnection> GetWormholeConnections(IList<SolarSystemViewModel> allSystems)
        {
            var wormholeRepository = DIContainer.GetInstance<IWormholeConnectionRepository>();
            var whConnections = wormholeRepository.GetWormholeConnectionsBetweenSystems(allSystems);
            return GetWormholeConnections(allSystems, whConnections);
        }

        private static IEnumerable<SolarSystemConnection> GetWormholeConnections(IList<SolarSystemViewModel> allSystems,
                                                                                 IEnumerable<WormholeConnection> whConnections)
        {
            return
                whConnections.Select(
                                     x =>
                                     new WormholeSolarSystemConnection(allSystems.First(y => y.Name == x.FirstSystem),
                                                                       allSystems.First(y => y.Name == x.SecondSystem),
                                                                       x));
        }

        private static IEnumerable<SolarSystemConnection> GetWormholeConnectionsWithout(IList<SolarSystemViewModel> allSystems,
                                                                                        SolarSystemViewModel[] withoutList)
        {
            var wormholeRepository = DIContainer.GetInstance<IWormholeConnectionRepository>();
            var whConnections = wormholeRepository.GetWormholeConnectionsBetweenSystemsWithout(allSystems, withoutList);
            return GetWormholeConnections(allSystems, whConnections);
        }

        private static WormholeInfo GetWormholeInfo(string systemName)
        {
            var result = new WormholeInfo();
            _getWormholeDataNameParameter.Value = systemName;
            using (var reader = _getWormholeDataCommand.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return result;
                }

                result.Class = "C" + reader.GetInt32(0);
                result.Anomaly = reader.GetString(1);
                result.Static1 = reader.GetString(2);
                result.Static2 = reader.GetString(3);
            }

            _getWormholeEffectsSystenNameParameter.Value = systemName;
            using (var reader = _getWormholeEffectsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Effects.Add(new WormholeEffect
                                           {
                                               EffectedStat = reader.GetString(0),
                                               EffectValue = Convert.ToDouble(reader.GetDecimal(1))
                                                                    .ToString("P0")
                                           });
                }
            }

            return result;
        }

        private static void InitAreSystemsConnectedCommand()
        {
            _areSystemsConnectedCommand = DB_CONNECTION.CreateCommand();

            _areSystemsConnectedCommand.CommandText =
                "SELECT COUNT(*) FROM System as s1 JOIN SystemConnection as sc ON s1.SystemID = sc.FromSystem JOIN System as s2 ON s2.SystemID = sc.ToSystem WHERE s1.SystemName=@system1 AND s2.SystemName=@system2";
            _areSystemsConnectedSystem1Parameter = _areSystemsConnectedCommand.CreateParameter();
            _areSystemsConnectedSystem1Parameter.ParameterName = "system1";

            _areSystemsConnectedSystem2Parameter = _areSystemsConnectedCommand.CreateParameter();
            _areSystemsConnectedSystem2Parameter.ParameterName = "system2";

            _areSystemsConnectedCommand.Parameters.Add(_areSystemsConnectedSystem1Parameter);
            _areSystemsConnectedCommand.Parameters.Add(_areSystemsConnectedSystem2Parameter);
        }

        private static void InitGetAdjacentSystemsCommand()
        {
            _getAdjacentSystemsCommand = DB_CONNECTION.CreateCommand();
            _getAdjacentSystemsSystemMaxLevelParameter = new SQLiteParameter("maxLevel", DbType.Int32);
            _getAdjacentSystemsSystemSecurityTypeParameter = new SQLiteParameter("securityType", DbType.Int32);
            _getAdjacentSystemsSystemIDParameter = new SQLiteParameter("solarSystemID", DbType.Int64);
            _getAdjacentSystemsCommand.CommandText = @"
                WITH RECURSIVE adjacentSystem(SystemID, SystemName, Security, SecurityType, RegionID, Level) AS(
                    SELECT SystemID, SystemName, Security, SecurityType, RegionID, 0 AS LEVEL FROM System WHERE SystemID = @solarSystemID

                    UNION

                    SELECT toS.SystemID, toS.SystemName, toS.Security, tos.SecurityType, toS.RegionID, Level + 1
                    FROM adjacentSystem
                        JOIN SystemConnection AS c ON c.FromSystem = adjacentSystem.SystemID
                        JOIN System AS toS ON toS.SystemID = c.ToSystem AND adjacentSystem.SecurityType = @securityType
                    WHERE Level < @maxLevel
                )
                SELECT DISTINCT SystemID, SystemName, Security, RegionID FROM adjacentSystem WHERE SystemID <> @solarSystemID";

            _getAdjacentSystemsCommand.Parameters.Add(_getAdjacentSystemsSystemSecurityTypeParameter);
            _getAdjacentSystemsCommand.Parameters.Add(_getAdjacentSystemsSystemMaxLevelParameter);
            _getAdjacentSystemsCommand.Parameters.Add(_getAdjacentSystemsSystemIDParameter);
        }

        private static void InitGetStationCommand()
        {
            _getStationsCommand = DB_CONNECTION.CreateCommand();
            _getStationsSystemIDParameter = new SQLiteParameter("systemID", DbType.Int64);
            _getStationsCommand.CommandText = "SELECT StationName, HasRepairFacility FROM Station WHERE SolarSystemID=@systemID";
            _getStationsCommand.Parameters.Add(_getStationsSystemIDParameter);
        }

        private static void InitGetSystemByIdCommand()
        {
            _getSystemByIdCommand = DB_CONNECTION.CreateCommand();
            _getSystemByIdIdParameter = new SQLiteParameter("systemId", DbType.Int64);
            _getSystemByIdCommand.CommandText = "SELECT SystemName, Security, RegionID FROM System WHERE SystemID = @systemId";
            _getSystemByIdCommand.Parameters.Add(_getSystemByIdIdParameter);
        }

        private static void InitGetSystemByRegionIdCommand()
        {
            _getSystemsByRegionIdCommand = DB_CONNECTION.CreateCommand();
            _getSystemByRegionIdRegionIdParameter = new SQLiteParameter("regionID", DbType.Int32);
            _getSystemByRegionIdSecurityLevelParameter = new SQLiteParameter("securityLevel", DbType.Int32);
            _getSystemsByRegionIdCommand.CommandText = "SELECT SystemID, SystemName, Security, RegionID FROM System WHERE RegionID = @regionId AND (SecurityType = @securityLevel)";
            _getSystemsByRegionIdCommand.Parameters.Add(_getSystemByRegionIdRegionIdParameter);
            _getSystemsByRegionIdCommand.Parameters.Add(_getSystemByRegionIdSecurityLevelParameter);
        }

        private static void InitGetSystemByNameCommand()
        {
            _getSystemByNameCommand = DB_CONNECTION.CreateCommand();
            _getSystemByNameParameter = new SQLiteParameter("systemName", DbType.String);
            _getSystemByNameCommand.CommandText = "SELECT SystemID, Security, RegionID FROM System WHERE SystemName = @systemName";
            _getSystemByNameCommand.Parameters.Add(_getSystemByNameParameter);
        }

        private static void InitGetWormholeDataCommand()
        {
            _getWormholeDataCommand = DB_CONNECTION.CreateCommand();
            _getWormholeDataNameParameter = new SQLiteParameter("systemName", DbType.String);
            _getWormholeDataCommand.CommandText = "SELECT Class, Anomaly, Static1, Static2 FROM Wormhole WHERE SystemName=@systemName";
            _getWormholeDataCommand.Parameters.Add(_getWormholeDataNameParameter);
        }

        private static void InitGetWormholeEffectsCommand()
        {
            _getWormholeEffectsCommand = DB_CONNECTION.CreateCommand();
            _getWormholeEffectsSystenNameParameter = new SQLiteParameter("systemName", DbType.String);
            _getWormholeEffectsCommand.CommandText =
                "SELECT EffectedStat, EffectValue FROM Wormhole JOIN WormholeEffect ON Wormhole.Anomaly = WormholeEffect.Anomaly AND Wormhole.Class = WormholeEffect.Class WHERE SystemName=@systemName";
            _getWormholeEffectsCommand.Parameters.Add(_getWormholeEffectsSystenNameParameter);
        }

        public static IList<SolarSystemViewModel> GetLowSecSystems()
        {
            var result = new List<SolarSystemViewModel>();
            using (var reader = _getLowSecSystemsCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var curSystem = new SolarSystemViewModel
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        SecurityStatus = Convert.ToDouble(reader.GetDecimal(2)),
                        RegionID = reader.GetInt32(3)
                    };
                    curSystem.Stations = GetStationsForSystem(curSystem.ID);

                    result.Add(curSystem);
                }
            }

            return result;
        }
    }
}
