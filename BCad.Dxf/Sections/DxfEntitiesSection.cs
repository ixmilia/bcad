using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Entities;

namespace BCad.Dxf.Sections
{
    internal class DxfEntitiesSection : DxfSection
    {
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

        public List<DxfEntity> Entities { get; private set; }

        public DxfEntitiesSection()
        {
            Entities = new List<DxfEntity>();
        }

        public DxfEntitiesSection(IEnumerable<DxfEntity> entities)
        {
            Entities = new List<DxfEntity>(entities);
        }

        public DxfEntitiesSection(IEnumerable<DxfCodePair> pairs)
        {
            var simpleRecords = DxfSection.SplitAtZero(pairs);
            Entities = simpleRecords.Select(r => DxfEntity.FromCodeValuePairs(r)).ToList();
        }
    }
}
