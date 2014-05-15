using System;
using System.Collections.Generic;
using System.Linq;

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
            if (items.Any())
            {
                foreach (var item in items)
                {
                    this.items.Add(item);
                }

                OnCollectionChanged();
            }
        }

        public void Clear()
        {
            if (items.Count > 0)
            {
                items.Clear();
                OnCollectionChanged();
            }
        }

        public void Set(IEnumerable<T> items)
        {
            var hadItems = this.items.Count > 0;
            this.items.Clear();
            AddRange(items);

            if (hadItems && !items.Any())
            {
                // need to trigger a changed event for the clearing
                OnCollectionChanged();
            }
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        protected void OnCollectionChanged()
        {
            var changed = CollectionChanged;
            if (changed != null)
                changed(this, new EventArgs());
        }
    }
}
