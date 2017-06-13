using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.AnalysisProvider
{
    public enum LastSeenType
    {
        Unknown = 0,
        Kill,
        Loss
    }

    public class LastSeen
    {
        public const int CAPSULE_ID = 670; //TODO extrahieren und genolution beruecksichtigen
        private static readonly LastSeen UNKNOWN_LAST_SHIP = new LastSeen
            {
                Occurrence = DateTime.MinValue,
                ShipName = "unknown",
                Weapon = "unknown",
                Type = LastSeenType.Unknown
            };

        public static LastSeen GetValueFromCollection(DatabaseContext context, int characterId, IEnumerable<Kill> kills)
        {
            return GetLastSeenFromLastKill(context, characterId,
                                           kills.OrderByDescending(k => k.KillTime).FirstOrDefault(k=>(k.Victim.CharacterID == characterId && k.Victim.ShipTypeID != CAPSULE_ID) || k.Attackers.Any(a=>a.CharacterID == characterId)) );
        }

 

        private static LastSeen GetLastSeenFromLastKill(DatabaseContext context, int characterId, Kill lastKill)
        {
            if (lastKill == null)
            {
                return UNKNOWN_LAST_SHIP;
            }

            var wasVictim = lastKill.Victim.CharacterID == characterId;
            var attacker = wasVictim ? null : lastKill.Attackers.First(a => a.CharacterID == characterId);
            var shipTypeId = wasVictim
                                 ? lastKill.Victim.ShipTypeID
                                 : attacker.ShipTypeID;

            var shipName = context.ExecuteSqlQuery<String>(
                "select typeName from evedb.dbo.invTypes where typeID = @typeID",
                new SqlParameter("typeID", shipTypeId)).FirstOrDefault();

            //TODO prepared statemtents?
            var weaponName = wasVictim
                                 ? ""
                                 : context.ExecuteSqlQuery<String>(
                                     "select typeName from evedb.dbo.invTypes where typeID = @typeID",
                                     new SqlParameter("typeID", attacker.WeaponTypeID)).FirstOrDefault();

            return new LastSeen
                {
                    Occurrence = lastKill.KillTime,
                    ShipName = shipName,
                    Weapon = weaponName,
                    Type = wasVictim ? LastSeenType.Loss : LastSeenType.Kill
                };
        }

        public LastSeenType Type { get; private set; }

        public String ShipName { get; private set; }

        public DateTime Occurrence { get; private set; }

        public String Weapon { get; private set; }
    }
}