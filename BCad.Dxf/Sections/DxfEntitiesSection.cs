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
            var simpleRecords = CombineEntities(DxfSection.SplitAtZero(pairs));
            Entities = simpleRecords.Select(r => DxfEntity.FromCodeValuePairs(r)).Where(e => e != null).ToList();
        }

        private static IEnumerable<IEnumerable<DxfCodePair>> CombineEntities(IEnumerable<IEnumerable<DxfCodePair>> simpleEntities)
        {
            var entities = new List<IEnumerable<DxfCodePair>>();
            List<DxfCodePair> current = null;
            bool readingCompound = false;
            foreach (var simpleEntity in simpleEntities)
            {
                var first = simpleEntity.First();
                if (readingCompound)
                {
                    switch (first.StringValue)
                    {
                        case DxfEntity.SeqendType:
                            entities.Add(current);
                            current = null;
                            readingCompound = false;
                            break;
                        default:
                            current.AddRange(simpleEntity);
                            break;
                    }
                }
                else
                {
                    // check for start of compound
                    switch (first.StringValue)
                    {
                        case DxfEntity.PolylineType:
                            current = new List<DxfCodePair>(simpleEntity);
                            readingCompound = true;
                            break;
                        default:
                            entities.Add(simpleEntity);
                            break;
                    }
                }
            }

            if (readingCompound)
            {
                throw new DxfReadException("Compound entity not terminated");
            }

            return entities;
        }
    }
}
