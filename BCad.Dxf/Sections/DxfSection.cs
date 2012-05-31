using System;
using System.Collections.Generic;
using System.Linq;

namespace BCad.Dxf.Sections
{
    public abstract class DxfSection
    {
        public const string HeaderSectionText = "HEADER";
        public const string ClassesSectionText = "CLASSES";
        public const string TablesSectionText = "TABLES";
        public const string BlocksSectionText = "BLOCKS";
        public const string EntitiesSectionText = "ENTITIES";
        public const string ObjectsSectionText = "OBJECTS";

        public const string SectionText = "SECTION";
        public const string EndSectionText = "ENDSEC";

        public const string TableText = "TABLE";
        public const string EndTableText = "ENDTAB";

        public abstract DxfSectionType Type { get; }

        public abstract IEnumerable<DxfCodePair> ValuePairs { get; }

        protected DxfSection()
        {
        }

        public override string ToString()
        {
            return Type.ToSectionName();
        }

        internal static IEnumerable<IEnumerable<DxfCodePair>> SplitAtZero(IEnumerable<DxfCodePair> pairs)
        {
            var list = new List<List<DxfCodePair>>();
            foreach (var p in pairs)
            {
                if (p.Code == 0)
                    list.Add(new List<DxfCodePair>());
                if (list.Count == 0)
                    list.Add(new List<DxfCodePair>());
                list.Last().Add(p);
            }
            return list;
        }
    }
}
