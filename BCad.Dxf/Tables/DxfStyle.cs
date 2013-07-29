using System;
using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfStyle : DxfSymbolTableFlags
    {
        internal const string AcDbTextStyleTableRecordText = "AcDbTextStyleTableRecord";

        private int TextGenerationFlags = 0;

        public string Name { get; set; }

        public bool IsShape
        {
            get { return DxfHelpers.GetFlag(Flags, 1); }
            set { DxfHelpers.SetFlag(value, ref Flags, 1); }
        }

        public bool IsVerticalText
        {
            get { return DxfHelpers.GetFlag(Flags, 4); }
            set { DxfHelpers.SetFlag(value, ref Flags, 4); }
        }

        public double FixedTextHeight { get; set; }
        public double WidthFactor { get; set; }
        public double ObliqueAngle { get; set; }

        public bool IsTextBackwards
        {
            get { return DxfHelpers.GetFlag(TextGenerationFlags, 2); }
            set { DxfHelpers.SetFlag(value, ref TextGenerationFlags, 2); }
        }

        public bool IsTextUpsideDown
        {
            get { return DxfHelpers.GetFlag(TextGenerationFlags, 4); }
            set { DxfHelpers.SetFlag(value, ref TextGenerationFlags, 4); }
        }

        public double LastHeight { get; set; }
        public string PrimaryFontFileName { get; set; }
        public string BigFontFileName { get; set; }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            var list = new List<DxfCodePair>();
            Action<int, object> add = (code, value) => list.Add(new DxfCodePair(code, value));
            add(100, AcDbTextStyleTableRecordText);
            add(2, Name);
            add(70, (short)Flags);
            add(40, FixedTextHeight);
            add(41, WidthFactor);
            add(50, ObliqueAngle);
            add(71, (short)TextGenerationFlags);
            add(42, LastHeight);
            add(3, PrimaryFontFileName);
            add(4, BigFontFileName);

            return list;
        }

        internal static DxfStyle FromBuffer(DxfCodePairBufferReader buffer)
        {
            var style = new DxfStyle();
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
                        style.Name = pair.StringValue;
                        break;
                    case 3:
                        style.PrimaryFontFileName = pair.StringValue;
                        break;
                    case 4:
                        style.BigFontFileName = pair.StringValue;
                        break;
                    case 40:
                        style.FixedTextHeight = pair.DoubleValue;
                        break;
                    case 41:
                        style.WidthFactor = pair.DoubleValue;
                        break;
                    case 42:
                        style.LastHeight = pair.DoubleValue;
                        break;
                    case 50:
                        style.ObliqueAngle = pair.DoubleValue;
                        break;
                    case 70:
                        style.Flags = pair.ShortValue;
                        break;
                    case 71:
                        style.TextGenerationFlags = pair.ShortValue;
                        break;
                }
            }

            return style;
        }
    }
}
