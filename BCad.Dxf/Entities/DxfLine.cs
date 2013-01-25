using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfLine : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Line; } }

        public override string SubclassMarker { get { return "AcDbLine"; } }

        public DxfPoint P1 { get; set; }

        public DxfPoint P2 { get; set; }

        public double Thickness { get; set; }

        public DxfVector ExtrusionDirection { get; set; }

        public DxfLine()
            : this(new DxfPoint() { X = 0, Y = 0, Z = 0 }, new DxfPoint() { X = 0, Y = 0, Z = 0 })
        {
        }

        public DxfLine(DxfPoint p1, DxfPoint p2)
            : base()
        {
            P1 = p1;
            P2 = p2;
            ExtrusionDirection = DxfVector.ZAxis;
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            foreach (var pair in base.GetCommonValuePairs())
                yield return pair;
            yield return new DxfCodePair(10, P1.X);
            yield return new DxfCodePair(20, P1.Y);
            yield return new DxfCodePair(30, P1.Z);
            yield return new DxfCodePair(11, P2.X);
            yield return new DxfCodePair(21, P2.Y);
            yield return new DxfCodePair(31, P2.Z);
            if (Thickness != 0.0)
            {
                yield return new DxfCodePair(39, Thickness);
            }
            if (ExtrusionDirection != DxfVector.ZAxis)
            {
                yield return new DxfCodePair(210, ExtrusionDirection.X);
                yield return new DxfCodePair(220, ExtrusionDirection.Y);
                yield return new DxfCodePair(230, ExtrusionDirection.Z);
            }
        }

        internal static DxfLine LineFromBuffer(DxfCodePairBufferReader buffer)
        {
            var line = new DxfLine();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    // done reading line
                    break;
                }

                buffer.Advance();
                if (!line.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 10:
                            line.P1.X = pair.DoubleValue;
                            break;
                        case 20:
                            line.P1.Y = pair.DoubleValue;
                            break;
                        case 30:
                            line.P1.Z = pair.DoubleValue;
                            break;
                        case 11:
                            line.P2.X = pair.DoubleValue;
                            break;
                        case 21:
                            line.P2.Y = pair.DoubleValue;
                            break;
                        case 31:
                            line.P2.Z = pair.DoubleValue;
                            break;
                        case 39:
                            line.Thickness = pair.DoubleValue;
                            break;
                        case 210:
                            line.ExtrusionDirection.X = pair.DoubleValue;
                            break;
                        case 220:
                            line.ExtrusionDirection.Y = pair.DoubleValue;
                            break;
                        case 230:
                            line.ExtrusionDirection.Z = pair.DoubleValue;
                            break;
                    }
                }
            }

            return line;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(":{0}{1}", P1, P2);
        }
    }
}
