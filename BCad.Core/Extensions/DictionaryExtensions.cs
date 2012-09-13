using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Collections;

namespace BCad.Extensions
{
    public static class DictionaryExtensions
    {
        public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }
    }
}
