using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Entities
{
    public enum HorizontalTextJustification
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Aligned = 3,
        Middle = 4,
        Fit = 5
    }

    public enum VerticalTextJustification
    {
        Baseline = 0,
        Bottom = 1,
        Middle = 2,
        Top = 3
    }

    public class DxfText : DxfEntity
    {
        public const string TextSubclassMarker = "AcDbText";

        public override DxfEntityType EntityType { get { return DxfEntityType.Text; } }

        public override string SubclassMarker { get { return TextSubclassMarker; } }

        public DxfPoint Location { get; set; }

        public DxfVector Normal { get; set; }

        public double TextHeight { get; set; }

        public double Rotation { get; set; }

        public string Value { get; set; }

        public double Thickness { get; set; }

        public double RelativeXScaleFactor { get; set; }

        public double ObliqueAngle { get; set; }

        public string TextStyleName { get; set; }

        public DxfPoint SecondAlignmentPoint { get; set; }

        public bool IsTextBackward
        {
            get { return GetBit(textGenerationFlags, 2); }
            set { textGenerationFlags = SetBit(textGenerationFlags, 2, value); }
        }

        public bool IsTextUpsideDown
        {
            get { return GetBit(textGenerationFlags, 3); }
            set { textGenerationFlags = SetBit(textGenerationFlags, 3, value); }
        }

        public HorizontalTextJustification HorizontalTextJustification { get; set; }

        public VerticalTextJustification VerticalTextJustification { get; set; }

        private int textGenerationFlags = 0;

        public DxfText()
            : this(DxfPoint.Origin, 1.0, null)
        {
        }

        public DxfText(DxfPoint location, double textHeight, string value)
        {
            this.Location = location;
            this.TextHeight = textHeight;
            this.Value = value;
            this.Normal = DxfVector.ZAxis;
            this.Rotation = 0.0;
            this.RelativeXScaleFactor = 1.0;
            this.HorizontalTextJustification = HorizontalTextJustification.Left;
            this.VerticalTextJustification = Entities.VerticalTextJustification.Baseline;
            this.SecondAlignmentPoint = DxfPoint.Origin;
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            foreach (var pair in base.GetCommonValuePairs())
                yield return pair;
            yield return new DxfCodePair(1, Value);
            if (TextStyleName != null && TextStyleName != "STANDARD")
                yield return new DxfCodePair(7, TextStyleName);
            yield return new DxfCodePair(10, Location.X);
            yield return new DxfCodePair(20, Location.Y);
            yield return new DxfCodePair(30, Location.Z);
            if (Thickness != 0.0)
                yield return new DxfCodePair(39, Thickness);
            yield return new DxfCodePair(40, TextHeight);
            if (RelativeXScaleFactor != 1.0)
                yield return new DxfCodePair(41, RelativeXScaleFactor);
            if (Rotation != 0.0)
                yield return new DxfCodePair(50, Rotation);
            if (ObliqueAngle != 0.0)
                yield return new DxfCodePair(51, ObliqueAngle);
            if (textGenerationFlags != 0)
                yield return new DxfCodePair(71, (short)textGenerationFlags);
            if (HorizontalTextJustification != HorizontalTextJustification.Left)
                yield return new DxfCodePair(72, (short)HorizontalTextJustification);
            if (textGenerationFlags != 0 || HorizontalTextJustification != HorizontalTextJustification.Left)
            {
                yield return new DxfCodePair(11, SecondAlignmentPoint.X);
                yield return new DxfCodePair(21, SecondAlignmentPoint.Y);
                yield return new DxfCodePair(31, SecondAlignmentPoint.Z);
            }
            if (VerticalTextJustification != Entities.VerticalTextJustification.Baseline)
                yield return new DxfCodePair(73, (short)VerticalTextJustification);
            if (Normal != DxfVector.ZAxis)
            {
                yield return new DxfCodePair(210, Normal.X);
                yield return new DxfCodePair(220, Normal.Y);
                yield return new DxfCodePair(230, Normal.Z);
            }
        }

        internal static DxfText TextFromBuffer(DxfCodePairBufferReader buffer)
        {
            var text = new DxfText();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    // done
                    break;
                }

                buffer.Advance();
                if (!text.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 1:
                            text.Value = pair.StringValue;
                            break;
                        case 7:
                            text.TextStyleName = pair.StringValue;
                            break;
                        case 10:
                            text.Location.X = pair.DoubleValue;
                            break;
                        case 20:
                            text.Location.Y = pair.DoubleValue;
                            break;
                        case 30:
                            text.Location.Z = pair.DoubleValue;
                            break;
                        case 39:
                            text.Thickness = pair.DoubleValue;
                            break;
                        case 40:
                            text.TextHeight = pair.DoubleValue;
                            break;
                        case 41:
                            text.RelativeXScaleFactor = pair.DoubleValue;
                            break;
                        case 50:
                            text.Rotation = pair.DoubleValue;
                            break;
                        case 51:
                            text.ObliqueAngle = pair.DoubleValue;
                            break;
                        case 71:
                            text.textGenerationFlags = pair.ShortValue;
                            break;
                        case 72:
                            text.HorizontalTextJustification = (HorizontalTextJustification)pair.ShortValue;
                            break;
                        case 73:
                            text.VerticalTextJustification = (VerticalTextJustification)pair.ShortValue;
                            break;
                        case 11:
                            text.SecondAlignmentPoint.X = pair.DoubleValue;
                            break;
                        case 21:
                            text.SecondAlignmentPoint.Y = pair.DoubleValue;
                            break;
                        case 31:
                            text.SecondAlignmentPoint.Z = pair.DoubleValue;
                            break;
                        case 100:
                            Debug.Assert(TextSubclassMarker == pair.StringValue);
                            break;
                        case 210:
                            text.Normal.X = pair.DoubleValue;
                            break;
                        case 220:
                            text.Normal.Y = pair.DoubleValue;
                            break;
                        case 230:
                            text.Normal.Z = pair.DoubleValue;
                            break;
                        default:
                            // unknown or unsupported code
                            break;
                    }
                }
            }

            return text;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
