using System;
using System.Collections.Generic;
using System.Linq;
using EVE_Killboard_Analyser.Helper.DatabaseWriter;
using EVE_Killboard_Analyser.Helper.Gatecamp;
using log4net;
using Newtonsoft.Json;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper
{
    //TODO services per DI injecten
    public class ZKillboardRedisqClient
    {
        public const string URL = "http://redisq.zkillboard.com/listen.php";
        private static readonly ILog LOG = LogManager.GetLogger(typeof (ZKillboardRedisqClient));
        //private static readonly BlockingCollection<Kill> KILLS = new BlockingCollection<Kill>();
        public static void Start(GateCampDetectionService gateCampDetectionService)
        {
            while (true)
            {
                try
                {
                    var kill = ReadKillFromRedisq();
                    if (kill == null)
                    {
                        continue;
                    }
                    KillEntryWriter.Instance.ForceAdd(
                                                      new KillEntries
                                                      {
                                                          CharacterId = -1,
                                                          Kills = new List<Kill>
                                                                  {
                                                                      kill
                                                                  }
                                                      });

                    gateCampDetectionService.AddKill(kill);
                }
                catch (Exception e)
                {
                    LOG.Warn("Error in redisq connection: " + e.Message, e);
                }
            }
        }

        private static Kill ReadKillFromRedisq()
        {
            var crestKill = ReadNextCrestKill();

            return crestKill == null ? null : ConvertCrestToOldSchoolKill(crestKill);
        }

        private static Kill ConvertCrestToOldSchoolKill(CrestKillmail crestKill)
        {
            try
            {
                return new Kill
                       {
                           Attackers = crestKill.Attackers.Select(
                                                                  x => new Attacker
                                                                       {
                                                                           AllianceID = (x.Alliance?.Id).GetValueOrDefault(),
                                                                           AllianceName = x.Alliance?.Name ?? "",
                                                                           CharacterID = (x.Character?.Id).GetValueOrDefault(),
                                                                           CharacterName = x.Character?.Name ?? "",
                                                                           CorporationID = (x.Corporation?.Id).GetValueOrDefault(),
                                                                           CorporationName = x.Corporation?.Name ?? "",
                                                                           FactionID = (x.Faction?.Id).GetValueOrDefault(),
                                                                           FactionName = x.Faction?.Name ?? "",
                                                                           DamageDone = x.DamageDone,
                                                                           FinalBlow = x.FinalBlow,
                                                                           WeaponTypeID = (x.WeaponType?.Id).GetValueOrDefault(),
                                                                           ShipTypeID = (x.ShipType?.Id).GetValueOrDefault(),
                                                                           SecurityStatus = (x.SecurityStatus).GetValueOrDefault()
                                                                       })
                               .ToArray(),
                           Victim = new Victim
                                    {
                                        AllianceID = (crestKill.Victim.Alliance?.Id).GetValueOrDefault(),
                                        AllianceName = crestKill.Victim.Alliance?.Name ?? "",
                                        CorporationID = (crestKill.Victim.Corporation?.Id).GetValueOrDefault(),
                                        CorporationName = crestKill.Victim.Corporation?.Name ?? "",
                                        CharacterID = (crestKill.Victim.Character?.Id).GetValueOrDefault(),
                                        CharacterName = crestKill.Victim.Character?.Name ?? "",
                                        FactionID = (crestKill.Victim.Faction?.Id).GetValueOrDefault(),
                                        FactionName = crestKill.Victim.Faction?.Name ?? "",
                                        DamageTaken = crestKill.Victim.DamageTaken,
                                        ShipTypeID = (crestKill.Victim?.ShipType?.Id).GetValueOrDefault(),
                                        X = crestKill.Victim.Position?.X,
                                        Y = crestKill.Victim.Position?.Y,
                                        Z = crestKill.Victim.Position?.Z
                                    },
                           KillID = crestKill.KillID,
                           KillTime = crestKill.KillTime,
                           SolarSystemID = crestKill.SolarSystem.Id,
                           Items = crestKill.Victim.Items.Select(
                                                                 x => new Item
                                                                      {
                                                                          Flag = x.Flag,
                                                                          QtyDestroyed = x.QuantityDestroyed,
                                                                          QtyDropped = x.QuantityDropped,
                                                                          Singleton = x.Singleton,
                                                                          TypeID = x.ItemType.Id
                                                                      })
                               .ToArray()
                       };
            }
            catch (Exception e)
            {
                throw new Exception("redisq conversion failed from:\n" + JsonConvert.SerializeObject(crestKill, Formatting.Indented), e);
            }
        }

        private static CrestKillmail ReadNextCrestKill()
        {
            var response = WebUtilities.GetHttpGetResponseFrom(URL);
            var message = JsonConvert.DeserializeObject<ZkbRedisqPage>(response);

            return message.Package?.Killmail;
        }
    }

    public class CrestSolarSystem
    {
        public int Id { get; set; }
    }

    public class CrestIdName
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }

    public class CrestItem
    {
        public CrestIdName ItemType { get; set; }

        public int QuantityDestroyed { get; set; }

        public int Flag { get; set; }

        public int QuantityDropped { get; set; }

        public int Singleton { get; set; }
    }

    public class CrestVictim
    {
        public CrestIdName Character { get; set; }

        public CrestIdName Alliance { get; set; }

        public CrestIdName Corporation { get; set; }

        public CrestIdName Faction { get; set; }

        public CrestIdName ShipType { get; set; }

        public int DamageTaken { get; set; }

        public Position Position { get; set; }

        public IList<CrestItem> Items { get; set; }
    }

    public class CrestAttacker
    {
        public CrestIdName Character { get; set; }

        public CrestIdName Alliance { get; set; }

        public CrestIdName Corporation { get; set; }

        public CrestIdName Faction { get; set; }

        public CrestIdName WeaponType { get; set; }

        public CrestIdName ShipType { get; set; }

        public int DamageDone { get; set; }

        public bool FinalBlow { get; set; }

        public float? SecurityStatus { get; set; }
    }

    public class CrestKillmail
    {
        public long KillID { get; set; }

        public CrestSolarSystem SolarSystem { get; set; }

        public DateTime KillTime { get; set; }

        public IList<CrestAttacker> Attackers { get; set; }

        public CrestVictim Victim { get; set; }
    }

    public class ZkbRedisqPackage
    {
        public CrestKillmail Killmail { get; set; }
    }

    public class ZkbRedisqPage
    {
        public ZkbRedisqPackage Package { get; set; }
    }
}
