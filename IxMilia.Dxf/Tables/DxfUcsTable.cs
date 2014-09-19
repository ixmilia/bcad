using System.Collections.Generic;
using System.Linq;
using IxMilia.Dxf.Sections;

namespace IxMilia.Dxf.Tables
{
    public class DxfUcsTable : DxfTable
    {
        public override DxfTableType TableType
        {
            get { return DxfTableType.Ucs; }
        }

        public List<DxfUcs> UserCoordinateSystems { get; private set; }

        public DxfUcsTable()
        {
            UserCoordinateSystems = new List<DxfUcs>();
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            if (UserCoordinateSystems.Count == 0)
                yield break;
            yield return new DxfCodePair(0, DxfSection.TableText);
            yield return new DxfCodePair(2, DxfTable.UcsText);
            foreach (var ucs in UserCoordinateSystems.OrderBy(d => d.Name))
            {
                foreach (var pair in ucs.GetValuePairs())
                    yield return pair;
            }

            yield return new DxfCodePair(0, DxfSection.EndTableText);
        }

        internal static DxfUcsTable UcsTableFromBuffer(DxfCodePairBufferReader buffer)
        {
            var table = new DxfUcsTable();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                {
                    break;
                }

                if (pair.Code == 0 && pair.StringValue == DxfTable.UcsText)
                {
                    var ucs = DxfUcs.FromBuffer(buffer);
                    table.UserCoordinateSystems.Add(ucs);
                }
            }

            return table;
        }
    }
}
