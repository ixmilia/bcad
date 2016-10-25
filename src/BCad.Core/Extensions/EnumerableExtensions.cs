using System.Collections.Generic;
using System.Linq;

namespace BCad.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items)
        {
            return items.Where(x => x != null);
        }
    }
}
