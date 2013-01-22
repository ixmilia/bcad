using BCad.Dxf.Sections;
using System.Collections.Generic;
using System.Linq;

namespace BCad.Dxf.Tables
{
    public class DxfLayerTable : DxfTable
    {
        public override DxfTableType TableType
        {
            get { return DxfTableType.Layer; }
        }

        public List<DxfLayer> Layers { get; private set; }

        public DxfLayerTable()
            : this(new DxfLayer[0])
        {
        }

        public DxfLayerTable(IEnumerable<DxfLayer> layers)
        {
            Layers = new List<DxfLayer>(layers);
        }

        public override IEnumerable<DxfCodePair> GetTableValuePairs()
        {
            foreach (var l in Layers.OrderBy(l => l.Name))
            {
                yield return new DxfCodePair(0, DxfLayer.LayerText);
                foreach (var p in l.ValuePairs)
                {
                    yield return p;
                }
            }
        }

        internal static DxfLayerTable LayerTableFromBuffer(DxfCodePairBufferReader buffer)
        {
            var table = new DxfLayerTable();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                {
                    break;
                }

                if (pair.Code != 0 || pair.StringValue != DxfLayerTable.LayerText)
                {
                    throw new DxfReadException("Expected layer start.");
                }

                var layer = DxfLayer.FromBuffer(buffer);
                table.Layers.Add(layer);
            }

            return table;
        }
    }
}
