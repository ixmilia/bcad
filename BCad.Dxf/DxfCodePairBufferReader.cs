using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Dxf
{
    internal class DxfCodePairBufferReader
    {
        private DxfCodePair[] items;
        private int position;

        public DxfCodePairBufferReader(IEnumerable<DxfCodePair> pairs)
        {
            this.items = pairs.ToArray();
            this.position = 0;
        }

        public bool ItemsRemain
        {
            get
            {
                return this.position < this.items.Length;
            }
        }

        public DxfCodePair Peek()
        {
            if (!this.ItemsRemain)
            {
                throw new DxfReadException("No more code pairs.");
            }

            return this.items[this.position];
        }

        public void Advance()
        {
            this.position++;
            while (ItemsRemain && Peek().Code == 999)
            {
                this.position++; // swallow all comments
            }
        }
    }
}
