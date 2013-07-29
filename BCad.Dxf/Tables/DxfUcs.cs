using System;
using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfUcs : DxfSymbolTableFlags
    {
        internal const string AcDbUCSTableRecordText = "AcDbUCSTableRecord";

        public string Name { get; set; }
        public DxfPoint Origin { get; set; }
        public DxfVector XAxisDirection { get; set; }
        public DxfVector YAxisDirection { get; set; }

        public DxfUcs()
        {
            Origin = DxfPoint.Origin;
            XAxisDirection = DxfVector.XAxis;
            YAxisDirection = DxfVector.YAxis;
        }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            var list = new List<DxfCodePair>();
            Action<int, object> add = (code, value) => list.Add(new DxfCodePair(code, value));
            add(100, AcDbUCSTableRecordText);
            add(2, Name);
            add(70, (short)Flags);
            add(10, Origin.X);
            add(20, Origin.Y);
            add(30, Origin.Z);
            add(11, XAxisDirection.X);
            add(21, XAxisDirection.Y);
            add(31, XAxisDirection.Z);
            add(12, YAxisDirection.X);
            add(22, YAxisDirection.Y);
            add(32, YAxisDirection.Z);

            return list;
        }

        internal static DxfUcs FromBuffer(DxfCodePairBufferReader buffer)
        {
            var ucs = new DxfUcs();
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
                        ucs.Name = pair.StringValue;
                        break;
                    case 10:
                        ucs.Origin.X = pair.DoubleValue;
                        break;
                    case 11:
                        ucs.XAxisDirection.X = pair.DoubleValue;
                        break;
                    case 12:
                        ucs.YAxisDirection.X = pair.DoubleValue;
                        break;
                    case 20:
                        ucs.Origin.Y = pair.DoubleValue;
                        break;
                    case 21:
                        ucs.XAxisDirection.Y = pair.DoubleValue;
                        break;
                    case 22:
                        ucs.YAxisDirection.Y = pair.DoubleValue;
                        break;
                    case 30:
                        ucs.Origin.Z = pair.DoubleValue;
                        break;
                    case 31:
                        ucs.XAxisDirection.Z = pair.DoubleValue;
                        break;
                    case 32:
                        ucs.YAxisDirection.Z = pair.DoubleValue;
                        break;
                    case 70:
                        ucs.Flags = pair.ShortValue;
                        break;
                }
            }

            return ucs;
        }
    }
}
