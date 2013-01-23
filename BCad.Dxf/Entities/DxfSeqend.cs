using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfSeqend : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Seqend; } }

        public override string SubclassMarker { get { return null; } }

        internal override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            return new DxfCodePair[0];
        }

        internal static DxfSeqend SeqendFromBuffer(DxfCodePairBufferReader buffer)
        {
            var seqend = new DxfSeqend();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                if (!seqend.TrySetSharedCode(pair))
                {
                    // nothing to parse
                }
            }

            return seqend;
        }
    }
}
