using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfSimpleTable
    {
        public string TableName { get; set; }

        public IEnumerable<DxfCodePair> ValuePairs { get; private set; }

        public DxfSimpleTable(string name, IEnumerable<DxfCodePair> pairs)
        {
            TableName = name;
            ValuePairs = pairs;
        }
    }
}
