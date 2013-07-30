using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Sections;

namespace BCad.Dxf.Tables
{
    public class DxfBlockRecordTable : DxfTable
    {
        public override DxfTableType TableType
        {
            get { return DxfTableType.BlockRecord; }
        }

        public List<DxfBlockRecord> BlockRecords { get; private set; }

        public DxfBlockRecordTable()
        {
            BlockRecords = new List<DxfBlockRecord>();
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            if (BlockRecords.Count == 0)
                yield break;
            yield return new DxfCodePair(0, DxfSection.TableText);
            yield return new DxfCodePair(2, DxfTable.BlockRecordText);
            foreach (var blockRecord in BlockRecords.OrderBy(d => d.Name))
            {
                foreach (var pair in blockRecord.GetValuePairs())
                    yield return pair;
            }

            yield return new DxfCodePair(0, DxfSection.EndTableText);
        }

        internal static DxfBlockRecordTable BlockRecordTableFromBuffer(DxfCodePairBufferReader buffer)
        {
            var table = new DxfBlockRecordTable();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                {
                    break;
                }

                if (pair.Code == 0 && pair.StringValue == DxfTable.BlockRecordText)
                {
                    var blockRecord = DxfBlockRecord.FromBuffer(buffer);
                    table.BlockRecords.Add(blockRecord);
                }
            }

            return table;
        }
    }
}
