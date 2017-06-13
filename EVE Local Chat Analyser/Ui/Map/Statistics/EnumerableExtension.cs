using System;
using System.Collections.Generic;
using System.Linq;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    public static class EnumerableExtension
    {
        public static int MaxOrDefault<T>(this IEnumerable<T> entries, Func<T, int> maxSelector)
        {
            var enumerable = entries as T[] ?? entries.ToArray();
            return enumerable.Any() ? enumerable.Max(maxSelector) : 0;
        }
    }
}