using System;
using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfBlockRecord : DxfSymbolTableFlags
    {
        internal const string AcDbBlockTableRecordText = "AcDbBlockTableRecord";

        public string Name { get; set; }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            var list = new List<DxfCodePair>();
            Action<int, object> add = (code, value) => list.Add(new DxfCodePair(code, value));
            add(100, AcDbBlockTableRecordText);
            add(2, Name);
            add(70, (short)Flags);

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
