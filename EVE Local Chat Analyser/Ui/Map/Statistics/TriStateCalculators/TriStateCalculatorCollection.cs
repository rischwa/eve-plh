using System;
using System.Collections.Generic;
using System.Linq;
using EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.TriStateCalculators
{
    public static class TriStateCalculatorCollection
    {
        private static readonly Dictionary<string, TypedCalculatorViewModel> VALUES = CalculatorCollectionHelper.GetIndexCalculators<ITriStateCalculator>();

        public static IEnumerable<TypedCalculatorViewModel> ViewModels => VALUES.Values;

        public static TypedCalculatorViewModel GetViewModel(string name)
        {
            return VALUES.ContainsKey(name) ? VALUES[name] : VALUES.First().Value;
        }

        public static ITriStateCalculator CreateByName(string name)
        {
            return (ITriStateCalculator)Activator.CreateInstance(VALUES[name].Type);
        }
    }
}