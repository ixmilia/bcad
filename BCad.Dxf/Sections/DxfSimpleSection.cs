using System.Collections.Generic;

namespace BCad.Dxf.Sections
{
    public class DxfSimpleSection
    {
        public string SectionName { get; private set; }

        public IEnumerable<DxfCodePair> ValuePairs { get; private set; }

        public DxfSimpleSection(string name, IEnumerable<DxfCodePair> pairs)
        {
            SectionName = name;
            ValuePairs = pairs;
        }
    }
}
