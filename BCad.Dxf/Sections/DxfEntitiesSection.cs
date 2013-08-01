using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Entities;

namespace BCad.Dxf.Sections
{
    internal class DxfEntitiesSection : DxfSection
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

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs()
        {
           return this.Entities.SelectMany(e => e.GetValuePairs());
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
                if (entity != null)
                {
                    section.Entities.Add(entity);
                }
            }

            return section;
        }
    }
}
