using System;
using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfAppId : DxfSymbolTableFlags
    {
        internal const string AcDbRegAppTableRecordText = "AcDbRegAppTableRecord";

        public string Name { get; set; }

        public bool IsXDataSavedWithR12
        {
            get { return !DxfHelpers.GetFlag(Flags, 1); }
            set { DxfHelpers.SetFlag(!value, ref Flags, 1); }
        }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            var list = new List<DxfCodePair>();
            Action<int, object> add = (code, value) => list.Add(new DxfCodePair(code, value));
            add(100, AcDbRegAppTableRecordText);
            add(2, Name);
            add(70, (short)Flags);

            return list;
        }

        internal static DxfAppId FromBuffer(DxfCodePairBufferReader buffer)
        {
            var appId = new DxfAppId();
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
                        appId.Name = pair.StringValue;
                        break;
                    case 70:
                        appId.Flags = pair.ShortValue;
                        break;
                }
            }

            return appId;
        }
    }
}
