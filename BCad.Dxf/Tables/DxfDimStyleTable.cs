using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Sections;

namespace BCad.Dxf.Tables
{
    public class DxfDimStyleTable : DxfTable
    {
        public override DxfTableType TableType
        {
            get { return DxfTableType.DimStyle; }
        }

        public List<DxfDimStyle> DimensionStyles { get; private set; }

        public DxfDimStyleTable()
            : this(new DxfDimStyle[0])
        {
        }

        public DxfDimStyleTable(IEnumerable<DxfDimStyle> dimStyles)
        {
            DimensionStyles = new List<DxfDimStyle>(dimStyles);
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            if (DimensionStyles.Count == 0)
                yield break;
            yield return new DxfCodePair(0, DxfSection.TableText);
            yield return new DxfCodePair(2, DxfTable.DimStyleText);
            foreach (var dimStyle in DimensionStyles.OrderBy(d => d.Name))
            {
                foreach (var pair in dimStyle.GetValuePairs())
                    yield return pair;
            }

            yield return new DxfCodePair(0, DxfSection.EndTableText);
        }

        internal static DxfDimStyleTable DimStyleTableFromBuffer(DxfCodePairBufferReader buffer)
        {
            var table = new DxfDimStyleTable();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                {
                    break;
                }

                if (pair.Code == 0 && pair.StringValue == DxfTable.DimStyleText)
                {
                    var dimStyle = DxfDimStyle.FromBuffer(buffer);
                    table.DimensionStyles.Add(dimStyle);
                }
            }

            return table;
        }
    }
}
