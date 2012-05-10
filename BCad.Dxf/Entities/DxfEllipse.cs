using System.Collections.Generic;
using System;

namespace BCad.Dxf.Entities
{
    public class DxfEllipse: DxfEntity
    {
        public override DxfEntityType EntityType
        {
            get { return DxfEntityType.Ellipse; }
        }

        public DxfPoint Center { get; set; }

        public DxfPoint MajorAxisEndPoint { get; set; }

        public DxfVector Normal { get; set; }

        public double MinorAxisRatio { get; set; }

        public double StartParameter { get; set; }

        public double EndParameter { get; set; }

        public DxfEllipse()
            : this(new DxfPoint() { X = 0, Y = 0, Z = 0 }, new DxfPoint() { X = 0, Y = 0, Z = 0 }, 1.0)
        {
        }

        public DxfEllipse(DxfPoint center, DxfPoint majorAxisEndPoint, double minorAxisRatio)
        {
            Center = center;
            MajorAxisEndPoint = majorAxisEndPoint;
            MinorAxisRatio = minorAxisRatio;
            Normal = new DxfVector() { X = 0.0, Y = 0.0, Z = 1.0 };
            StartParameter = 0.0;
            EndParameter = Math.PI * 2.0;
        }

        public static DxfEllipse FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            var el = new DxfEllipse();
            el.PopulateDefaultAndCommonValues(pairs);
            foreach (var pair in pairs)
            {
                switch (pair.Code)
                {
                    case 10:
                        el.Center.X = pair.DoubleValue;
                        break;
                    case 20:
                        el.Center.Y = pair.DoubleValue;
                        break;
                    case 30:
                        el.Center.Z = pair.DoubleValue;
                        break;
                    case 11:
                        el.MajorAxisEndPoint.X = pair.DoubleValue;
                        break;
                    case 21:
                        el.MajorAxisEndPoint.Y = pair.DoubleValue;
                        break;
                    case 31:
                        el.MajorAxisEndPoint.Z = pair.DoubleValue;
                        break;
                    case 210:
                        el.Normal.X = pair.DoubleValue;
                        break;
                    case 220:
                        el.Normal.Y = pair.DoubleValue;
                        break;
                    case 230:
                        el.Normal.Z = pair.DoubleValue;
                        break;
                    case 40:
                        el.MinorAxisRatio = pair.DoubleValue;
                        break;
                    case 41:
                        el.StartParameter = pair.DoubleValue;
                        break;
                    case 42:
                        el.EndParameter = pair.DoubleValue;
                        break;
                }
            }
            return el;
        }

        protected override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            yield return new DxfCodePair(10, Center.X);
            yield return new DxfCodePair(20, Center.Y);
            yield return new DxfCodePair(30, Center.Z);
            yield return new DxfCodePair(11, MajorAxisEndPoint.X);
            yield return new DxfCodePair(21, MajorAxisEndPoint.Y);
            yield return new DxfCodePair(31, MajorAxisEndPoint.Z);
            if (Normal != new DxfVector(0, 0, 1))
            {
                yield return new DxfCodePair(210, Normal.X);
                yield return new DxfCodePair(220, Normal.Y);
                yield return new DxfCodePair(230, Normal.Z);
            }

            yield return new DxfCodePair(40, MinorAxisRatio);
            yield return new DxfCodePair(41, StartParameter);
            yield return new DxfCodePair(42, EndParameter);
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(":{0}R1={1},R2={0}", Center, MajorAxisEndPoint, MinorAxisRatio);
        }
    }
}
