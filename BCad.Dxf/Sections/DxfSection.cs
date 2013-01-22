using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BCad.Dxf.Sections
{
    public abstract class DxfSection
    {
        internal const string HeaderSectionText = "HEADER";
        internal const string ClassesSectionText = "CLASSES";
        internal const string TablesSectionText = "TABLES";
        internal const string BlocksSectionText = "BLOCKS";
        internal const string EntitiesSectionText = "ENTITIES";
        internal const string ObjectsSectionText = "OBJECTS";

        internal const string SectionText = "SECTION";
        internal const string EndSectionText = "ENDSEC";

        internal const string TableText = "TABLE";
        internal const string EndTableText = "ENDTAB";

        public abstract DxfSectionType Type { get; }

        public abstract IEnumerable<DxfCodePair> ValuePairs { get; }

        protected DxfSection()
        {
        }

        public override string ToString()
        {
            return Type.ToSectionName();
        }

        internal static DxfSection FromBuffer(DxfCodePairBufferReader buffer)
        {
            Debug.Assert(buffer.ItemsRemain);
            var sectionType = buffer.Peek();
            buffer.Advance();
            if (sectionType.Code != 2)
            {
                throw new DxfReadException("Expected code 2, got " + sectionType.Code);
            }

            DxfSection section;
            switch (sectionType.StringValue)
            {
                case EntitiesSectionText:
                    section = DxfEntitiesSection.EntitiesSectionFromBuffer(buffer);
                    break;
                case HeaderSectionText:
                    section = DxfHeaderSection.HeaderSectionFromBuffer(buffer);
                    break;
                default:
                    throw new DxfReadException("Unexpected section type: " + sectionType.StringValue);
            }

            return section;
        }
    }
}
