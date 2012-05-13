using System;
using System.Collections.Generic;

namespace BCad.Collections
{
    public class ObservableHashSet<T>
    {
        private HashSet<T> items = new HashSet<T>();

        public event EventHandler CollectionChanged;

        public int Count { get { return items.Count; } }

        public bool Add(T item)
        {
            var result = items.Add(item);
            OnCollectionChanged();
            return result;
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                this.items.Add(item);
            OnCollectionChanged();
        }

        public void Clear()
        {
            items.Clear();
            OnCollectionChanged();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        protected void OnCollectionChanged()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new EventArgs());
        }
    }
}
