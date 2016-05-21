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
    }
}
