using System.Collections.Generic;
using System;

namespace BCad.Dxf.Entities
{
    public class DxfEllipse: DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Ellipse; } }

        public override string SubclassMarker { get { return "AcDbEllipse"; } }

        public DxfPoint Center { get; set; }

        public DxfVector MajorAxis { get; set; }

        public DxfVector Normal { get; set; }

        public double MinorAxisRatio { get; set; }

        public double StartParameter { get; set; }

        public double EndParameter { get; set; }

        public DxfEllipse()
            : this(new DxfPoint() { X = 0, Y = 0, Z = 0 }, new DxfVector() { X = 0, Y = 0, Z = 0 }, 1.0)
        {
        }

        public DxfEllipse(DxfPoint center, DxfVector majorAxis, double minorAxisRatio)
            : base()
        {
            Center = center;
            MajorAxis = majorAxis;
            MinorAxisRatio = minorAxisRatio;
            Normal = new DxfVector() { X = 0.0, Y = 0.0, Z = 1.0 };
            StartParameter = 0.0;
            EndParameter = 360.0;
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            foreach (var pair in base.GetCommonValuePairs())
                yield return pair;
            yield return new DxfCodePair(10, Center.X);
            yield return new DxfCodePair(20, Center.Y);
            yield return new DxfCodePair(30, Center.Z);
            yield return new DxfCodePair(11, MajorAxis.X);
            yield return new DxfCodePair(21, MajorAxis.Y);
            yield return new DxfCodePair(31, MajorAxis.Z);
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

        internal static DxfEllipse EllipseFromBuffer(DxfCodePairBufferReader buffer)
        {
            var ellipse = new DxfEllipse();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                if (!ellipse.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 10:
                            ellipse.Center.X = pair.DoubleValue;
                            break;
                        case 20:
                            ellipse.Center.Y = pair.DoubleValue;
                            break;
                        case 30:
                            ellipse.Center.Z = pair.DoubleValue;
                            break;
                        case 11:
                            ellipse.MajorAxis.X = pair.DoubleValue;
                            break;
                        case 21:
                            ellipse.MajorAxis.Y = pair.DoubleValue;
                            break;
                        case 31:
                            ellipse.MajorAxis.Z = pair.DoubleValue;
                            break;
                        case 210:
                            ellipse.Normal.X = pair.DoubleValue;
                            break;
                        case 220:
                            ellipse.Normal.Y = pair.DoubleValue;
                            break;
                        case 230:
                            ellipse.Normal.Z = pair.DoubleValue;
                            break;
                        case 40:
                            ellipse.MinorAxisRatio = pair.DoubleValue;
                            break;
                        case 41:
                            ellipse.StartParameter = pair.DoubleValue;
                            break;
                        case 42:
                            ellipse.EndParameter = pair.DoubleValue;
                            break;
                    }
                }
            }

            return ellipse;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(":{0}R1={1},R2={0}", Center, MajorAxis, MinorAxisRatio);
        }
    }
}
