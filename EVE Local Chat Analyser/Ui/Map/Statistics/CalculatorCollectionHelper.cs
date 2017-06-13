using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    public static class CalculatorCollectionHelper
    {

        private static readonly ILog LOG = LogManager.GetLogger("CalculatorCollectionHelper");
        public static Dictionary<string, TypedCalculatorViewModel> GetIndexCalculators<T>() where  T:class
        {
            try
            {
                var iPaletteType = typeof(T);
                return Assembly.GetAssembly(iPaletteType)
                    .GetTypes()
                    .Where(x => iPaletteType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .ToDictionary(x => x.Name, x => new TypedCalculatorViewModel(x));
            }
            catch (Exception e)
            {
                LOG.Error("could not load index calculators: " + e.Message, e);
                return new Dictionary<string, TypedCalculatorViewModel>();
            }
        }
    }
}