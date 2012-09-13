using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Collections
{
    public class ReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly IDictionary<TKey, TValue> dictionary;

        public ReadOnlyDictionary()
        {
            this.dictionary = new Dictionary<TKey, TValue>();
        }

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        #region Query methods

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return this.dictionary.Keys;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                return this.dictionary.Values;
            }
        }

        #endregion

        public ReadOnlyDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            return Update(this.dictionary, dict => dict.Add(key, value));
        }

        public ReadOnlyDictionary<TKey, TValue> Remove(TKey key)
        {
            return Update(this.dictionary, dict => dict.Remove(key));
        }

        private static ReadOnlyDictionary<TKey, TValue> Update(IDictionary<TKey, TValue> dict, Action<IDictionary<TKey, TValue>> action)
        {
            var newDict = new Dictionary<TKey, TValue>(dict);
            action(newDict);
            return new ReadOnlyDictionary<TKey, TValue>(newDict);
        }

        public IEnumerator GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }
    }
}
