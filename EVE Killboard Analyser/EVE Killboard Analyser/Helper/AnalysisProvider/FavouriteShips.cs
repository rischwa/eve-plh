using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.AnalysisProvider
{
    public class FavouriteShips : IAnalysisProvider
    {
        public string FieldName
        {
            get { return "favourite_ships"; }
        }

        public object GetValueFromCollection(int characterId, IList<Kill> kills)
        {
            var startDate = DateTime.Now.AddMonths(-3);

            var attackingShips = from curKill in kills
                                  where curKill.KillTime > startDate
                                  let attackers = curKill.Attackers
                                  let attacker =
                                      (attackers ?? new Attacker[0]).FirstOrDefault(
                                          attacker => attacker.CharacterID == characterId)
                                  where attacker != null
                                  select attacker.ShipTypeID;

            var victimShips = from curKill in kills
                              where curKill.KillTime > startDate
                              where curKill.Victim.CharacterID == characterId
                              select curKill.Victim.ShipTypeID;
            
            var favouriteIds = (from curShip in attackingShips.Concat(victimShips)
                                group curShip by curShip
                                into g
                                orderby g.Count() descending
                                select g.Key).ToList();
            if (!favouriteIds.Any())
            {
                return new List<string>(0);
            }

            //we take the 5 favourite, but return only 3, because capsule can be in a top stop e.g. for gankers
            //and #System for unknown values and we don't want to consider those
            favouriteIds = favouriteIds.Take(5).ToList();
            using (var context = new DatabaseContext())
            {
                var ships =
                    context.ExecuteSqlQuery<Ship>(
                        string.Format("select typeID, typeName from evedb.dbo.invTypes where typeID in ({0})",
                                      string.Join(",", favouriteIds)));

                return ships.Join(favouriteIds, ship => ship.typeID, i => i, (ship, i) => ship.typeName).Where(s => s != "Capsule" && s!= "#System").Take(3).ToList();
            }
        }

        private class Ship
        {
            public int typeID { get; set; }
            public string typeName { get; set; }
        }
    }
}