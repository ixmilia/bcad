using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Entities;

namespace BCad.Dxf.Sections
{
    public class DxfEntitiesSection : DxfSection
    {
        public List<DxfEntity> Entities { get; private set; }

        public DxfEntitiesSection()
        {
            Entities = new List<DxfEntity>();
        }

        public override DxfSectionType Type
        {
            get { return DxfSectionType.Entities; }
        }

        public override IEnumerable<DxfCodePair> ValuePairs
        {
            get
            {
                foreach (var e in Entities)
                {
                    yield return new DxfCodePair(0, e.EntityTypeString);
                    foreach (var p in e.ValuePairs)
                    {
                        yield return p;
                    }
                }
            }
        }

        internal static DxfEntitiesSection EntitiesSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfEntitiesSection();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (DxfCodePair.IsSectionEnd(pair))
                {
                    // done reading entities
                    buffer.Advance(); // swallow (0, ENDSEC)
                    break;
                }

                if (pair.Code != 0)
                {
                    throw new DxfReadException("Expected new entity.");
                }

                var entity = DxfEntity.FromBuffer(buffer);
                section.Entities.Add(entity);
            }

            return section;
        }
    }
}
