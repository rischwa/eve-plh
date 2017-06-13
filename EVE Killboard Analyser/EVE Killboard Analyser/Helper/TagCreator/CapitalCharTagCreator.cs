using System.Collections.Generic;
using System.Linq;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper.TagCreator
{
    public class CapitalCharTagCreator : ITagCreator
    {
        private static IList<CapitalShip> _capitalShips;


        public IList<string> TagsFromCollection(int characterId, IList<Kill> kills)
        {
            var shipTypeIds = (from curKill in kills
                              let attacker =
                                  curKill.Attackers.FirstOrDefault(attacker => attacker.CharacterID == characterId)
                              select (attacker != null ? attacker.ShipTypeID : curKill.Victim.ShipTypeID)).Distinct();

            var flownCapitalCategories = (from curShipType in shipTypeIds.Distinct()
                                          join curCapital in CapitalShips on curShipType equals curCapital.typeID
                                          select curCapital.groupName).Distinct();

            return flownCapitalCategories.Select(s => s + " Pilot").ToList();
        }

        private static IEnumerable<CapitalShip> CapitalShips
        {
            get
            {
                if (_capitalShips != null)
                {
                    return _capitalShips;
                }
                
                const string QUERY = @"
                select 
                    typeID, groupName 
                from 
                    evedb.dbo.invGroups as g, evedb.dbo.invTypes as t
                where
                    t.groupID = g.groupID and
                    groupName IN ('Carrier', 'Dreadnought', 'Supercarrier', 'Titan' )";

                using (var context = new DatabaseContext())
                {
                    _capitalShips = context.ExecuteSqlQuery<CapitalShip>(QUERY);
                }

                return _capitalShips;
            }
        }

        private class CapitalShip
        {
            public int typeID { get; set; }
            public string groupName { get; set; }
        }
    }
}