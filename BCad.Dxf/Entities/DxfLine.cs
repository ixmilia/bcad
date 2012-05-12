using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfLine : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Line; } }

        public override string SubclassMarker { get { return "AcDbLine"; } }

        public DxfPoint P1 { get; set; }

        public DxfPoint P2 { get; set; }

        public DxfLine()
            : this(new DxfPoint() { X = 0, Y = 0, Z = 0 }, new DxfPoint() { X = 0, Y = 0, Z = 0 })
        {
        }

        public DxfLine(DxfPoint p1, DxfPoint p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public static DxfLine FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            var line = new DxfLine();
            line.PopulateDefaultAndCommonValues(pairs);
            foreach (var pair in pairs)
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
                }
            }
            return line;
        }

        protected override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            yield return new DxfCodePair(10, P1.X);
            yield return new DxfCodePair(20, P1.Y);
            yield return new DxfCodePair(30, P1.Z);
            yield return new DxfCodePair(11, P2.X);
            yield return new DxfCodePair(21, P2.Y);
            yield return new DxfCodePair(31, P2.Z);
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(":{0}{1}", P1, P2);
        }
    }
}
