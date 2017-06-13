using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVE_Killboard_Analyser.Helper;
using EVE_Killboard_Analyser.Helper.Gatecamp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PLHLib;

namespace EVE_Killboard_Analyser_Test
{

    internal class TestStargateLocationRepository : IStargateLocationRepository
    {
        private bool _returnValue;
        private StargateLocation _location;

        public void SetNextStargateLocation(bool returnValue, StargateLocation location = null)
        {
            _returnValue = returnValue;
            _location = location;

        }

        public bool TryGetStargateLocation(int solarSystemID, Position pos, out StargateLocation location)
        {
            location = _location;
            return _returnValue;
        }
    }

    [TestClass]
    public class GateCampDetectorTest
    {
        private const int SVIPUL_ID = 34562;
        private const int ARTY_ID = 492;
        private TestStargateLocationRepository _locationRepository;
        private GateCampDetector _gateCampDetector;

        [TestInitialize]
        public void Setup()
        {
            _locationRepository = new TestStargateLocationRepository();
            _gateCampDetector = new GateCampDetector(_locationRepository);
        }

        [TestMethod]
        public void TestStargates()
        {
            var sql = new SqliteStargateLocationRepository();
            StargateLocation location;
           Assert.IsTrue(sql.TryGetStargateLocation(
                                       30002813,
                                       new Position
                                       {
                                           X = 4386820552358.3,
                                           Y = 1099574841173.3,
                                           Z = 426254394664.27
                                       },
                                       out location));
        }
        [TestMethod]
        public void TestRecentGC()
        {
            _locationRepository.SetNextStargateLocation(
                                                        true,
                                                        new StargateLocation()
                                                        {
                                                            SolarSystemID1 = 1,
                                                            SolarSystemID2 = 2
                                                        });
            _gateCampDetector.AddRange(new[] {GetArtySvipulKill(),});

            _gateCampDetector.DetectGateCamps();

            Assert.AreEqual(1,_gateCampDetector.GateCamps.Count);
            Assert.AreEqual(0.6d,_gateCampDetector.GateCamps.First().GateCampIndex);

        }

        [TestMethod]
        public void TestRecentMultipleGC()
        {
            _locationRepository.SetNextStargateLocation(
                                                        true,
                                                        new StargateLocation()
                                                        {
                                                            SolarSystemID1 = 1,
                                                            SolarSystemID2 = 2
                                                        });
            _gateCampDetector.AddRange(new[] { GetArtySvipulKill(), GetArtySvipulKill(isOld:true) });

            _gateCampDetector.DetectGateCamps();

            Assert.AreEqual(1, _gateCampDetector.GateCamps.Count);
            Assert.AreEqual(1d, _gateCampDetector.GateCamps.First().GateCampIndex);

        }

        [TestMethod]
        public void TestGateCampMoving()
        {
            _locationRepository.SetNextStargateLocation(
                                                      true,
                                                      new StargateLocation()
                                                      {
                                                          SolarSystemID1 = 1,
                                                          SolarSystemID2 = 2
                                                      });

            _gateCampDetector.AddRange(new[] { GetArtySvipulKill(isOld:true)});

            _locationRepository.SetNextStargateLocation(false);
            _gateCampDetector.AddRange(new [] {GetArtySvipulKill(isOld:false)});

            _gateCampDetector.DetectGateCamps();
            Assert.AreEqual(0, _gateCampDetector.GateCamps.Count);
        }

        private static KillResult GetArtySvipulKill(bool isOld = false)
        {
            return new KillResult(
                new Kill
                {
                    KillTime = isOld ? DateTime.UtcNow : DateTime.UtcNow.Subtract(new TimeSpan(0,50,0)),
                    Attackers = new List<Attacker>
                                {
                                    new Attacker()
                                    {
                                        CharacterID = 1,
                                        ShipTypeID = SVIPUL_ID,
                                        WeaponTypeID = ARTY_ID
                                    }
                                },
                    Victim = new Victim { }
                });
        }
    }
}
