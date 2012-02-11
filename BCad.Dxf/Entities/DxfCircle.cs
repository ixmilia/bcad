using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfCircle : DxfEntity
    {
        public override DxfEntityType EntityType
        {
            get { return DxfEntityType.Circle; }
        }

        public DxfPoint Center { get; set; }

        public double Radius { get; set; }

        public DxfVector Normal { get; set; }

        public DxfCircle()
            : this(new DxfPoint() { X = 0, Y = 0, Z = 0 }, 0.0)
        {
        }

        public DxfCircle(DxfPoint center, double radius)
        {
            Center = center;
            Radius = radius;
            Normal = new DxfVector() { X = 0.0, Y = 0.0, Z = 1.0 };
        }

        public static DxfCircle FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            var cir = new DxfCircle();
            cir.PopulateDefaultAndCommonValues(pairs);
            foreach (var pair in pairs)
            {
                switch (pair.Code)
                {
                    case 10:
                        cir.Center.X = pair.DoubleValue;
                        break;
                    case 20:
                        cir.Center.Y = pair.DoubleValue;
                        break;
                    case 30:
                        cir.Center.Z = pair.DoubleValue;
                        break;
                    case 40:
                        cir.Radius = pair.DoubleValue;
                        break;
                    case 210:
                        cir.Normal.X = pair.DoubleValue;
                        break;
                    case 220:
                        cir.Normal.Y = pair.DoubleValue;
                        break;
                    case 230:
                        cir.Normal.Z = pair.DoubleValue;
                        break;
                }
            }
            return cir;
        }

        protected override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
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
