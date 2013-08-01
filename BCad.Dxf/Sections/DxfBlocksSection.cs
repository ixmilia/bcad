using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Blocks;

namespace BCad.Dxf.Sections
{
    public class DxfBlocksSection : DxfSection
    {
        public List<DxfBlock> Blocks { get; private set; }

        public DxfBlocksSection()
        {
            Blocks = new List<DxfBlock>();
        }

        public override DxfSectionType Type
        {
            get { return DxfSectionType.Blocks; }
        }

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs()
        {
            return this.Blocks.SelectMany(e => e.GetValuePairs());
        }

        internal static DxfBlocksSection BlocksSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfBlocksSection();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (DxfCodePair.IsSectionEnd(pair))
                {
                    // done reading blocks
                    buffer.Advance(); // swallow (0, ENDSEC)
                    break;
                }

                if (pair.Code != 0)
                {
                    throw new DxfReadException("Expected new block.");
                }

                buffer.Advance(); // swallow (0, CLASS)
                var block = DxfBlock.FromBuffer(buffer);
                if (block != null)
                {
                    section.Blocks.Add(block);
                }
            }

            return section;
        }
    }
}
