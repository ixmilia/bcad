using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BCad.Collections
{
    public class ReadOnlyList<T> : IEnumerable<T>
    {
        private T[] Items;

        public int Count { get { return Items.Length; } }

        public T this[int index]
        {
            get
            {
                return Items[index];
            }
        }

        private ReadOnlyList(T[] items)
        {
            Items = items;
        }

        public ReadOnlyList<T> Add(T item)
        {
            var newItems = new T[Count + 1];
            Array.Copy(Items, newItems, Count);
            newItems[newItems.Length - 1] = item;
            return new ReadOnlyList<T>(newItems);
        }

        public ReadOnlyList<T> Remove(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            var newItems = new T[Count - 1];
            int i = 0;
            for (; i < index; i++)
            {
                newItems[i] = Items[i];
            }
            for (; i < newItems.Length; i++)
            {
                newItems[i] = Items[i + 1];
            }

            return new ReadOnlyList<T>(newItems);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Items).GetEnumerator();
        }

        public static ReadOnlyList<T> Empty()
        {
            var items = new T[0];
            return new ReadOnlyList<T>(items);
        }

        public static ReadOnlyList<T> Create(IEnumerable<T> items)
        {
            var newItems = new T[items.Count()];
            int i = 0;
            foreach (var item in items)
            {
                newItems[i++] = item;
            }

            return new ReadOnlyList<T>(newItems);
        }
    }
}
