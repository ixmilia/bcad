using System;
using System.Collections.Generic;
using System.Diagnostics;
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
