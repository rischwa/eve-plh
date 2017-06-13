using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EveLocalChatAnalyser.Utilities
{
    public static class EnumerableUtils
    {
        //public static HashSet<T> ToHashSet<T, K>(this IEnumerable<K> enumerable, Func<K, T> selector)
        //{
        //    return new HashSet<T>(enumerable.S);
        //}

        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            return enumerable == null || !enumerable.GetEnumerator().MoveNext();
        }

        public static void AddAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TValue> values,
                                                Func<TValue, TKey> key)
        {
            foreach (var value in values)
            {
                dictionary.Add(key(value), value);
            }
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> enumerable, int maxAmount)
        {
            var list = enumerable as IList<T> ?? enumerable.ToArray();
            for (var i = 0; i < list.Count / maxAmount + 1; ++i)
            {
                yield return list.Skip(i * maxAmount)
                    .Take(maxAmount);
            }
        }
    }
}