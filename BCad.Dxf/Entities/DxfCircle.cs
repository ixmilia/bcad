using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfCircle : DxfEntity
    {
        public const string CircleSubclassMarker = "AcDbCircle";

        public override DxfEntityType EntityType { get { return DxfEntityType.Circle; } }

        public override string SubclassMarker { get { return CircleSubclassMarker; } }

        public DxfPoint Center { get; set; }

        public double Radius { get; set; }

        public DxfVector Normal { get; set; }

        public DxfCircle()
            : this(new DxfPoint() { X = 0, Y = 0, Z = 0 }, 0.0)
        {
        }

        public DxfCircle(DxfPoint center, double radius)
            : base()
        {
            Center = center;
            Radius = radius;
            Normal = new DxfVector() { X = 0.0, Y = 0.0, Z = 1.0 };
        }

        internal static DxfCircle CircleFromBuffer(DxfCodePairBufferReader buffer)
        {
            var circle = new DxfCircle();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                if (!circle.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 10:
                            circle.Center.X = pair.DoubleValue;
                            break;
                        case 20:
                            circle.Center.Y = pair.DoubleValue;
                            break;
                        case 30:
                            circle.Center.Z = pair.DoubleValue;
                            break;
                        case 40:
                            circle.Radius = pair.DoubleValue;
                            break;
                        case 210:
                            circle.Normal.X = pair.DoubleValue;
                            break;
                        case 220:
                            circle.Normal.Y = pair.DoubleValue;
                            break;
                        case 230:
                            circle.Normal.Z = pair.DoubleValue;
                            break;
                    }
                }
            }

            return circle;
        }

        internal override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            yield return new DxfCodePair(10, Center.X);
            yield return new DxfCodePair(20, Center.Y);
            yield return new DxfCodePair(30, Center.Z);
            yield return new DxfCodePair(40, Radius);
            if (Normal != new DxfVector(0, 0, 1))
            {
                yield return new DxfCodePair(210, Normal.X);
                yield return new DxfCodePair(220, Normal.Y);
                yield return new DxfCodePair(230, Normal.Z);
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(":{0}R{1}", Center, Radius);
        }
    }
}
