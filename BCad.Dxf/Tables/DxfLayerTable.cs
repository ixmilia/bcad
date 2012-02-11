using System.Collections.Generic;

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
            foreach (var l in Layers)
            {
                yield return new DxfCodePair(0, DxfLayer.LayerText);
                foreach (var p in l.ValuePairs)
                {
                    yield return p;
                }
            }
        }
    }
}
