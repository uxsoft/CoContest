using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest
{
    public static class Extensions
    {
        public static IEnumerable<T> Add<T>(this IEnumerable<T> col, T newObj)
        {
            foreach (T item in col)
                yield return item;
            yield return newObj;
        }

        public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> col, Random rng, Func<T, double> keySelector)
        {
            Dictionary<T, double> cache = new Dictionary<T, double>();
            return col.OrderBy(t =>
            {
                if (cache.ContainsKey(t))
                {
                    return cache[t];
                }
                else
                {
                    double key = keySelector(t) * rng.NextDouble();
                    cache[t] = key;
                    return key;
                }
            });
        }
    }
}
