using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.BCad.Collections
{
    public class ObservableHashSet<T> : IEnumerable<T>, IEnumerable
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
            var fireEvent = this.items.Any() || items.Any();
            this.items.Clear();
            foreach (var item in items)
            {
                this.items.Add(item);
            }

            if (fireEvent)
            {
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

        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.items).GetEnumerator();
        }
    }
}
