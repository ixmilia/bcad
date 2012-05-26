using System;
using System.Collections.Generic;

namespace BCad.Collections
{
    public class ObservableHashSet<T>
    {
        private HashSet<T> items = new HashSet<T>();
        private HashSet<int> itemHashes = new HashSet<int>();

        public event EventHandler CollectionChanged;

        public int Count { get { return items.Count; } }

        public bool Add(T item)
        {
            var result = items.Add(item);
            itemHashes.Add(item.GetHashCode());
            OnCollectionChanged();
            return result;
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.items.Add(item);
                this.itemHashes.Add(item.GetHashCode());
            }
            OnCollectionChanged();
        }

        public void Clear()
        {
            items.Clear();
            itemHashes.Clear();
            OnCollectionChanged();
        }

        public void Set(IEnumerable<T> items)
        {
            this.items.Clear();
            this.itemHashes.Clear();
            AddRange(items);
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public bool ContainsHash(int hash)
        {
            return itemHashes.Contains(hash);
        }

        protected void OnCollectionChanged()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new EventArgs());
        }
    }
}
