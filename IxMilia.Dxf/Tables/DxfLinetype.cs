using System.Collections.Generic;
using IxMilia.Dxf.Tables;

namespace IxMilia.Dxf
{
    public class DxfLinetype : DxfSymbolTableFlags
    {
        internal const string AcDbLinetypeTableRecordString = "AcDbLinetypeTableRecord";

        public string Name { get; set; }

        public string DescriptiveText { get; set; }

        public char AlignmentCode { get; set; }

        public short LinetypeElements { get; set; }

        public double TotalPatternLength { get; set; }

        // TODO: combine these List<T> into a list of more stuff
        public List<double> ElementLengths { get; private set; }

        public List<ComplexElementAttributes> ComplexAttributes { get; set; }

        public List<int> ShapeNumbers { get; private set; }

        public List<string> StylePointers { get; private set; }

        public List<double> ScaleValues { get; private set; }

        public List<double> RotationValues { get; private set; }

        public List<double> XOffsetValues { get; private set; }

        public List<double> YOffsetValues { get; private set; }

        public List<string> TextStrings { get; private set; }

        public class ComplexElementAttributes
        {
            internal int Flags = 0;

            public ComplexElementAttributes()
            {
            }

            internal ComplexElementAttributes(int flags)
            {
                Flags = flags;
            }

            public bool AbsoluteRotation
            {
                get { return DxfHelpers.GetFlag(Flags, 1); }
                set { DxfHelpers.SetFlag(value, ref Flags, 1); }
            }

            public bool EmbeddedElementIsString
            {
                get { return DxfHelpers.GetFlag(Flags, 2); }
                set { DxfHelpers.SetFlag(value, ref Flags, 2); }
            }

            public bool EmbeddedElementIsShape
            {
                get { return DxfHelpers.GetFlag(Flags, 4); }
                set { DxfHelpers.SetFlag(value, ref Flags, 4); }
            }
        }

        public DxfLinetype()
        {
            AlignmentCode = 'A';
            ElementLengths = new List<double>();
            ComplexAttributes = new List<ComplexElementAttributes>();
            ShapeNumbers = new List<int>();
            StylePointers = new List<string>();
            ScaleValues = new List<double>();
            RotationValues = new List<double>();
            XOffsetValues = new List<double>();
            YOffsetValues = new List<double>();
            TextStrings = new List<string>();
        }

        protected override string TableType { get { return DxfTable.LTypeText; } }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            foreach (var pair in CommonCodePairs())
                yield return pair;

            yield return new DxfCodePair(100, AcDbLinetypeTableRecordString);
            yield return new DxfCodePair(2, Name);
            yield return new DxfCodePair(70, (short)Flags);
            yield return new DxfCodePair(3, DescriptiveText);
            yield return new DxfCodePair(72, (short)AlignmentCode);
            yield return new DxfCodePair(73, LinetypeElements);
            yield return new DxfCodePair(40, TotalPatternLength);
            foreach (var length in ElementLengths)
                yield return new DxfCodePair(49, length);
            foreach (var attr in ComplexAttributes)
                yield return new DxfCodePair(74, (short)attr.Flags);
            foreach (var shape in ShapeNumbers)
                yield return new DxfCodePair(75, (short)shape);
            foreach (var pointer in StylePointers)
                yield return new DxfCodePair(340, pointer);
            foreach (var scale in ScaleValues)
                yield return new DxfCodePair(46, scale);
            foreach (var rotation in RotationValues)
                yield return new DxfCodePair(50, rotation);
            foreach (var xoffset in XOffsetValues)
                yield return new DxfCodePair(44, xoffset);
            foreach (var yoffset in YOffsetValues)
                yield return new DxfCodePair(45, yoffset);
            foreach (var text in TextStrings)
                yield return new DxfCodePair(9, text);
        }

        internal static DxfLinetype FromBuffer(DxfCodePairBufferReader buffer)
        {
            var linetype = new DxfLinetype();
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
                        linetype.Name = pair.StringValue;
                        break;
                    case 3:
                        linetype.DescriptiveText = pair.StringValue;
                        break;
                    case 9:
                        linetype.TextStrings.Add(pair.StringValue);
                        break;
                    case 40:
                        linetype.TotalPatternLength = pair.DoubleValue;
                        break;
                    case 44:
                        linetype.XOffsetValues.Add(pair.DoubleValue);
                        break;
                    case 45:
                        linetype.YOffsetValues.Add(pair.DoubleValue);
                        break;
                    case 46:
                        linetype.ScaleValues.Add(pair.DoubleValue);
                        break;
                    case 49:
                        linetype.ElementLengths.Add(pair.DoubleValue);
                        break;
                    case 50:
                        linetype.RotationValues.Add(pair.DoubleValue);
                        break;
                    case 70:
                        linetype.Flags = pair.ShortValue;
                        break;
                    case 72:
                        linetype.AlignmentCode = (char)pair.ShortValue;
                        break;
                    case 73:
                        linetype.LinetypeElements = pair.ShortValue;
                        break;
                    case 74:
                        linetype.ComplexAttributes.Add(new ComplexElementAttributes(pair.ShortValue));
                        break;
                    case 75:
                        linetype.ShapeNumbers.Add(pair.ShortValue);
                        break;
                    case 340:
                        linetype.StylePointers.Add(pair.StringValue);
                        break;
                }
            }

            return linetype;
        }
    }
}
