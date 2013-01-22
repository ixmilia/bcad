using BCad.Dxf.Sections;
using System.Collections.Generic;
using System.Linq;

namespace BCad.Dxf.Tables
{
    public class DxfViewPortTable : DxfTable
    {
        public override DxfTableType TableType
        {
            get { return DxfTableType.ViewPort; }
        }

        public List<DxfViewPort> ViewPorts { get; private set; }

        public DxfViewPortTable()
            : this(new DxfViewPort[0])
        {
        }

        public DxfViewPortTable(IEnumerable<DxfViewPort> viewPorts)
        {
            ViewPorts = new List<DxfViewPort>(viewPorts);
        }

        public override IEnumerable<DxfCodePair> GetTableValuePairs()
        {
            foreach (var v in ViewPorts)
            {
                yield return new DxfCodePair(0, DxfViewPort.ViewPortText);
                foreach (var p in v.ValuePairs)
                {
                    yield return p;
                }
            }
        }

        internal static DxfViewPortTable ViewPortTableFromBuffer(DxfCodePairBufferReader buffer)
        {
            var table = new DxfViewPortTable();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                {
                    break;
                }

                if (pair.Code != 0 || pair.StringValue != DxfViewPort.ViewPortText)
                {
                    throw new DxfReadException("Expected view port start.");
                }

                var vp = DxfViewPort.FromBuffer(buffer);
                table.ViewPorts.Add(vp);
            }

            return table;
        }
    }
}
