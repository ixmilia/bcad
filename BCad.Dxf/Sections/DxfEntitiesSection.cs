using System.Collections.Generic;
using System.Diagnostics;
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

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs(DxfAcadVersion version)
        {
           return this.Entities.SelectMany(e => e.GetValuePairs());
        }

        internal static DxfEntitiesSection EntitiesSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfEntitiesSection();
            bool isReadingPolyline = false;
            DxfPolyline currentPolyline = null;
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
                    if (isReadingPolyline)
                    {
                        switch (entity.EntityType)
                        {
                            case DxfEntityType.Vertex:
                                currentPolyline.Vertices.Add((DxfVertex)entity);
                                break;
                            case DxfEntityType.Seqend:
                                currentPolyline.Seqend = (DxfSeqend)entity;
                                isReadingPolyline = false;
                                break;
                            default:
                                Debug.Assert(false, "Unexpected entity found while reading polyline");
                                isReadingPolyline = false;
                                section.Entities.Add(entity);
                                break;
                        }
                    }
                    else
                    {
                        if (entity.EntityType == DxfEntityType.Polyline)
                        {
                            isReadingPolyline = true;
                            currentPolyline = (DxfPolyline)entity;
                        }

                        section.Entities.Add(entity);
                    }
                }
            }

            return section;
        }
    }
}
