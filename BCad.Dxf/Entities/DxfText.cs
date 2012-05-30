using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Entities
{
    public class DxfText : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Text; } }

        public override string SubclassMarker { get { return "AcDbText"; } }

        public DxfPoint Location { get; set; }

        public DxfVector Normal { get; set; }

        public double TextHeight { get; set; }

        public double Rotation { get; set; }

        public string Value { get; set; }

        public DxfText()
            : this(DxfPoint.Origin, 1.0, "")
        {
        }

        public DxfText(DxfPoint location, double textHeight, string value)
        {
            this.Location = location;
            this.TextHeight = textHeight;
            this.Value = value;
            this.Normal = DxfVector.ZAxis;
            this.Rotation = 0.0;
        }

        public static DxfText FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            var text = new DxfText();
            text.PopulateDefaultAndCommonValues(pairs);
            foreach (var pair in pairs)
            {
                switch (pair.Code)
                {
                    case 1:
                        text.Value = pair.StringValue;
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
                    case 40:
                        text.TextHeight = pair.DoubleValue;
                        break;
                    case 50:
                        text.Rotation = pair.DoubleValue;
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
                }
            }
            return text;
        }

        protected override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            yield return new DxfCodePair(1, Value);
            yield return new DxfCodePair(10, Location.X);
            yield return new DxfCodePair(20, Location.Y);
            yield return new DxfCodePair(30, Location.Z);
            yield return new DxfCodePair(40, TextHeight);
            if (Rotation != 0.0)
            {
                yield return new DxfCodePair(50, Rotation);
            }
            if (Normal != DxfVector.ZAxis)
            {
                yield return new DxfCodePair(210, Normal.X);
                yield return new DxfCodePair(220, Normal.Y);
                yield return new DxfCodePair(230, Normal.Z);
            }
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
