using System;
using System.Collections.Generic;

namespace EveLocalChatAnalyser.Ui.Map.Statistics.ColorCalculators
{
    public static class IndexCalculatorCollection
    {
        private static readonly Dictionary<string, TypedCalculatorViewModel> VALUES = CalculatorCollectionHelper.GetIndexCalculators<IIndexCalculator>();

        public static IEnumerable<TypedCalculatorViewModel> ViewModels => VALUES.Values;

        public static TypedCalculatorViewModel GetViewModel(string name)
        {
            return VALUES[name];
        }

        public static IIndexCalculator CreateByName(string name)
        {
            return (IIndexCalculator)Activator.CreateInstance(VALUES[name].Type);
        }
    }
}
