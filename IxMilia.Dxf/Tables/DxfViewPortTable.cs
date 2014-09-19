using IxMilia.Dxf.Sections;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dxf.Tables
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

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            if (ViewPorts.Count == 0)
                yield break;
            yield return new DxfCodePair(0, DxfSection.TableText);
            yield return new DxfCodePair(2, DxfTable.ViewPortText);
            foreach (var viewPort in ViewPorts)
            {
                foreach (var pair in viewPort.GetValuePairs())
                    yield return pair;
            }

            yield return new DxfCodePair(0, DxfSection.EndTableText);
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

                if (pair.Code == 0 && pair.StringValue == DxfViewPort.ViewPortText)
                {
                    var vp = DxfViewPort.FromBuffer(buffer);
                    table.ViewPorts.Add(vp);
                }
                else
                {
                    // TODO: viewport options
                }
            }

            return table;
        }
    }
}
