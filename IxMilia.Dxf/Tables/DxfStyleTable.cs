using System.Collections.Generic;
using System.Linq;
using IxMilia.Dxf.Sections;

namespace IxMilia.Dxf.Tables
{
    public class DxfStyleTable : DxfTable
    {
        public override DxfTableType TableType
        {
            get { return DxfTableType.Style; }
        }

        public List<DxfStyle> Styles { get; private set; }

        public DxfStyleTable()
        {
            Styles = new List<DxfStyle>();
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs(DxfAcadVersion version)
        {
            if (Styles.Count == 0)
                yield break;
            foreach (var common in CommonCodePairs(version))
            {
                yield return common;
            }

            foreach (var style in Styles.OrderBy(d => d.Name))
            {
                foreach (var pair in style.GetValuePairs())
                    yield return pair;
            }

            yield return new DxfCodePair(0, DxfSection.EndTableText);
        }

        internal static DxfStyleTable StyleTableFromBuffer(DxfCodePairBufferReader buffer)
        {
            var table = new DxfStyleTable();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                {
                    break;
                }

                if (pair.Code == 0 && pair.StringValue == DxfTable.StyleText)
                {
                    var style = DxfStyle.FromBuffer(buffer);
                    table.Styles.Add(style);
                }
            }

            return table;
        }
    }
}
