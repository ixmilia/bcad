using System;
using System.Collections.Generic;
using IxMilia.Dxf.Tables;

namespace IxMilia.Dxf
{
    public class DxfBlockRecord : DxfSymbolTableFlags
    {
        internal const string AcDbBlockTableRecordText = "AcDbBlockTableRecord";

        public string Name { get; set; }

        protected override string TableType { get { return DxfTable.BlockRecordText; } }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            var list = new List<DxfCodePair>();
            Action<int, object> add = (code, value) => list.Add(new DxfCodePair(code, value));

            foreach (var pair in CommonCodePairs())
                add(pair.Code, pair.Value);

            add(100, AcDbBlockTableRecordText);
            add(2, Name);

            return list;
        }

        internal static DxfBlockRecord FromBuffer(DxfCodePairBufferReader buffer)
        {
            var blockRecord = new DxfBlockRecord();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                switch (pair.Code)
                {
                    case 2:
                        blockRecord.Name = pair.StringValue;
                        break;
                    case 70:
                        blockRecord.Flags = pair.ShortValue;
                        break;
                }
            }

            return blockRecord;
        }
    }
}
