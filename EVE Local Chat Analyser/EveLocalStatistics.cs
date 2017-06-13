#region

using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Properties;

#endregion

namespace EveLocalChatAnalyser
{
    public class EveLocalStatistics
    {
        private Dictionary<string, int> _allianceCount = new Dictionary<string, int>();

        private Dictionary<string, int> _corporationCount = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _coalitionCount = new Dictionary<string, int>();

        public int MembersInNoAllianceCount { get; private set; }

        public int AllianceMembersCount([NotNull] string alliance)
        {
            int allianceMembersCount;
            return _allianceCount.TryGetValue(alliance, out allianceMembersCount) ? allianceMembersCount : 0;
        }

        public int CorporationMembersCount([NotNull] string corporation)
        {
            int corporationMembersCount;
            return _corporationCount.TryGetValue(corporation, out corporationMembersCount) ? corporationMembersCount : 0;
        }

        public void UpdateLocalStatistics(IList<IEveCharacter> charactersInLocal)
        {
            _corporationCount = GroupToCountDictionary(charactersInLocal, curChar => curChar.Corporation);

            _allianceCount = GroupToCountDictionary(charactersInLocal, curChar => curChar.Alliance);

            _coalitionCount.Clear();
            foreach (var curChar in charactersInLocal)
            {
                foreach (var curCoalition in curChar.Coalitions)
                {
                    if (_coalitionCount.ContainsKey(curCoalition.Name))
                    {
                        ++_coalitionCount[curCoalition.Name];
                    }else
                    {
                        _coalitionCount[curCoalition.Name] = 1;
                    }
                }
            }
            MembersInNoCoalitionCount = charactersInLocal.Count(c => !c.Coalitions.Any());
            MembersInNoAllianceCount = charactersInLocal.Count(character => character.Alliance == null);
        }

        public int MembersInNoCoalitionCount { get; set; }

        public int CoalitionMembersCount([NotNull] string coalition)
        {
            if (!_coalitionCount.ContainsKey(coalition))
            {
                return 0;
            }
            return _coalitionCount[coalition];
        }

        private static Dictionary<T, int> GroupToCountDictionary<T>(IEnumerable<IEveCharacter> currentlyInLocal,
                                                                    Func<IEveCharacter, T> grouper) where T : class
        {
            return (from curChar in currentlyInLocal
                    let groupValue = grouper(curChar)
                    where groupValue != null
                    group curChar by groupValue
                    into grouping select grouping).ToDictionary(group => group.Key, group => group.Count());
        }
    }
}