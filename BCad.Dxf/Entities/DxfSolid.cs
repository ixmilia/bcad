using System.Collections.Generic;

namespace BCad.Dxf.Entities
{
    public class DxfSolid : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Solid; } }

        public override string SubclassMarker { get { return "AcDbTrace"; } }

        public DxfPoint FirstCorner { get; set; }
        public DxfPoint SecondCorner { get; set; }
        public DxfPoint ThirdCorner { get; set; }
        public DxfPoint FourthCorner { get; set; }
        public double Thickness { get; set; }
        public DxfVector ExtrusionDirection { get; set; }

        public DxfSolid()
        {
            FirstCorner = DxfPoint.Origin;
            SecondCorner = DxfPoint.Origin;
            ThirdCorner = DxfPoint.Origin;
            FourthCorner = null;
            ExtrusionDirection = DxfVector.ZAxis;
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            foreach (var pair in base.GetCommonValuePairs())
                yield return pair;
            yield return new DxfCodePair(10, FirstCorner.X);
            yield return new DxfCodePair(20, FirstCorner.Y);
            yield return new DxfCodePair(30, FirstCorner.Z);
            yield return new DxfCodePair(11, SecondCorner.X);
            yield return new DxfCodePair(21, SecondCorner.Y);
            yield return new DxfCodePair(31, SecondCorner.Z);
            yield return new DxfCodePair(12, ThirdCorner.X);
            yield return new DxfCodePair(22, ThirdCorner.Y);
            yield return new DxfCodePair(32, ThirdCorner.Z);
            var fourth = FourthCorner;
            if (fourth == null)
                fourth = ThirdCorner;
            yield return new DxfCodePair(13, fourth.X);
            yield return new DxfCodePair(23, fourth.Y);
            yield return new DxfCodePair(33, fourth.Z);
            if (Thickness != 0.0)
                yield return new DxfCodePair(39, Thickness);
            if (ExtrusionDirection != DxfVector.ZAxis)
            {
                yield return new DxfCodePair(210, ExtrusionDirection.X);
                yield return new DxfCodePair(220, ExtrusionDirection.Y);
                yield return new DxfCodePair(230, ExtrusionDirection.Z);
            }
        }

        internal static DxfSolid SolidFromBuffer(DxfCodePairBufferReader buffer)
        {
            var solid = new DxfSolid();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    // done reading solid
                    break;
                }

                buffer.Advance();
                if (!solid.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 10:
                            solid.FirstCorner.X = pair.DoubleValue;
                            break;
                        case 20:
                            solid.FirstCorner.Y = pair.DoubleValue;
                            break;
                        case 30:
                            solid.FirstCorner.Z = pair.DoubleValue;
                            break;
                        case 11:
                            solid.SecondCorner.X = pair.DoubleValue;
                            break;
                        case 21:
                            solid.SecondCorner.Y = pair.DoubleValue;
                            break;
                        case 31:
                            solid.SecondCorner.Z = pair.DoubleValue;
                            break;
                        case 12:
                            solid.ThirdCorner.X = pair.DoubleValue;
                            break;
                        case 22:
                            solid.ThirdCorner.Y = pair.DoubleValue;
                            break;
                        case 32:
                            solid.ThirdCorner.Z = pair.DoubleValue;
                            break;
                        case 13:
                            solid.FourthCorner = solid.FourthCorner ?? DxfPoint.Origin;
                            solid.FourthCorner.X = pair.DoubleValue;
                            break;
                        case 23:
                            solid.FourthCorner = solid.FourthCorner ?? DxfPoint.Origin;
                            solid.FourthCorner.Y = pair.DoubleValue;
                            break;
                        case 33:
                            solid.FourthCorner = solid.FourthCorner ?? DxfPoint.Origin;
                            solid.FourthCorner.Z = pair.DoubleValue;
                            break;
                        case 39:
                            solid.Thickness = pair.DoubleValue;
                            break;
                        case 210:
                            solid.ExtrusionDirection.X = pair.DoubleValue;
                            break;
                        case 220:
                            solid.ExtrusionDirection.Y = pair.DoubleValue;
                            break;
                        case 230:
                            solid.ExtrusionDirection.Z = pair.DoubleValue;
                            break;
                    }
                }
            }

            return solid;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(":{0}{1}{2}{3}", FirstCorner, SecondCorner, ThirdCorner, FourthCorner);
        }
    }
}
