using System.Collections.Generic;
using System.Linq;

namespace BCad.Dxf
{
    internal class BufferReader<T>
    {
        private T[] items;
        private int position;

        public BufferReader(IEnumerable<T> items)
        {
            this.items = items.ToArray();
            this.position = 0;
        }

        public T Peek()
        {
            if (this.ItemsRemain)
            {
                return this.items[this.position];
            }
            else
            {
                return default(T);
            }
        }

        public void Advance()
        {
            this.position++;
        }

        public bool ItemsRemain
        {
            get
            {
                return this.position >= 0 && this.position < this.items.Length;
            }
        }
    }
}
