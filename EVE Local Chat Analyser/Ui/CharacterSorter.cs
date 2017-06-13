using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui
{
    internal class CharacterSorter
    {
        public enum SortOrder
        {
            Ascending = 1,
            Descending = -1
        }

        public CharacterSorter()
        {
            Comparator = CompareName;
        }

        public SortOrder Order { get; set; }
        public Func<IEveCharacter, IEveCharacter, SortOrder, int> Comparator { get; set; }

        public IList<EveCharacterViewModel> Sorted(IEnumerable<EveCharacterViewModel> characters)
        {
            var entries = characters.ToList();
            entries.Sort(Sort);

            return entries;
        }

        private int Sort(EveCharacterViewModel char1, EveCharacterViewModel char2)
        {
            var changeStatusComparison = CompareLocalChangeStatus(char1.EveCharacter, char2.EveCharacter);
            return changeStatusComparison != 0
                       ? changeStatusComparison
                       : Comparator(char1.EveCharacter, char2.EveCharacter, Order);
        }

        public void InvertSortOrder()
        {
            Order = Order == SortOrder.Descending ? SortOrder.Ascending : SortOrder.Descending;
        }

        public int CompareName(IEveCharacter char1, IEveCharacter char2, SortOrder order)
        {
            return String.Compare(char1.Name, char2.Name, StringComparison.InvariantCultureIgnoreCase)*(int) order;
        }

        public int CompareCorporation(IEveCharacter char1, IEveCharacter char2, SortOrder order)
        {
            var comparison =
                String.Compare(char1.Corporation, char2.Corporation, StringComparison.InvariantCultureIgnoreCase)*
                (int) order;
            return comparison == 0 ? CompareName(char1, char2, SortOrder.Ascending) : comparison;
        }

        public int CompareAlliance(IEveCharacter char1, IEveCharacter char2, SortOrder order)
        {
            var comparison =
                String.Compare(char1.Alliance, char2.Alliance, StringComparison.InvariantCultureIgnoreCase)*
                (int) order;
            return comparison == 0 ? CompareCorporation(char1, char2, SortOrder.Ascending) : comparison;
        }

        public int CompareCoalitions(IEveCharacter char1, IEveCharacter char2, SortOrder order)
        {
            var char1Coalition = char1.Coalitions.FirstOrDefault();
            var char1CoalitionName = char1Coalition != null ? char1Coalition.Name : "";

            var char2Coalition = char2.Coalitions.FirstOrDefault();
            var char2CoalitionName = char2Coalition != null ? char2Coalition.Name : "";

            var comparison =
               String.Compare(char1CoalitionName, char2CoalitionName, StringComparison.InvariantCultureIgnoreCase) *
               (int)order;
            return comparison == 0 ? CompareAlliance(char1, char2, SortOrder.Ascending) : comparison;
        }

        private static int CompareLocalChangeStatus(IEveCharacter char1, IEveCharacter char2)
        {
            if (char1.LocalChangeStatus == char2.LocalChangeStatus)
            {
                return 0;
            }

            if (char1.LocalChangeStatus == LocalChangeStatus.Exited)
            {
                return 1;
            }

            return char2.LocalChangeStatus == LocalChangeStatus.Exited ? -1 : 0;
        }
    }
}