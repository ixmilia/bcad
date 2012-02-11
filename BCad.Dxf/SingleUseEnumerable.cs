using System.Collections.Generic;

namespace BCad.Dxf
{
    internal class SingleUseEnumerable<T>
        where T : class
    {
        private Queue<T> queue;

        public SingleUseEnumerable(IEnumerable<T> items)
        {
            queue = new Queue<T>(items);
        }

        public T GetItem()
        {
            return queue.Count == 0 ? null : queue.Dequeue();
        }

        public IEnumerable<T> GetItems()
        {
            for (var item = GetItem(); item != null; item = GetItem())
                yield return item;
        }

        public int Count
        {
            get { return queue.Count; }
        }
    }
}
