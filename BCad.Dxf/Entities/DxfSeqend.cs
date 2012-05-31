using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfSeqend : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Seqend; } }

        public override string SubclassMarker { get { return null; } }

        protected override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            return null;
        }
    }
}
