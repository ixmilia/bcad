using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfViewPort
    {
        public const string ViewPortText = "VPORT";

        public string Name { get; set; }

        public DxfPoint LowerLeft { get; set; }

        public DxfPoint UpperRight { get; set; }

        public DxfPoint ViewCenter { get; set; }

        public DxfPoint SnapBasePoint { get; set; }

        public DxfVector SnapSpacing { get; set; }

        public DxfVector GridSpacing { get; set; }

        public DxfVector ViewDirection { get; set; }

        public DxfPoint TargetViewPoint { get; set; }

        public double ViewHeight { get; set; }

        public double ViewPortAspectRatio { get; set; }

        public double LensLength { get; set; }

        public double FrontClippingPlane { get; set; }

        public double BackClippingPlane { get; set; }

        public double SnapRotationAngle { get; set; }

        public double ViewTwistAngle { get; set; }

        public DxfViewPort()
        {
            Name = "*ACTIVE";
            LowerLeft = new DxfPoint();
            UpperRight = new DxfPoint();
            ViewCenter = new DxfPoint();
            SnapBasePoint = new DxfPoint();
            SnapSpacing = new DxfVector();
            GridSpacing = new DxfVector();
            ViewDirection = new DxfVector();
            TargetViewPoint = new DxfPoint();
            ViewHeight = 0.0;
            ViewPortAspectRatio = 0.0;
            LensLength = 0.0;
            FrontClippingPlane = 0.0;
            BackClippingPlane = 0.0;
            SnapRotationAngle = 0.0;
            ViewTwistAngle = 0.0;
        }

        public IEnumerable<DxfCodePair> ValuePairs
        {
            get
            {
                yield return new DxfCodePair(2, Name);
                yield return new DxfCodePair(10, LowerLeft.X);
                yield return new DxfCodePair(20, LowerLeft.Y);
                yield return new DxfCodePair(11, UpperRight.X);
                yield return new DxfCodePair(21, UpperRight.Y);
                yield return new DxfCodePair(12, ViewCenter.X);
                yield return new DxfCodePair(22, ViewCenter.Y);
                yield return new DxfCodePair(13, SnapBasePoint.X);
                yield return new DxfCodePair(23, SnapBasePoint.Y);
                yield return new DxfCodePair(14, SnapSpacing.X);
                yield return new DxfCodePair(24, SnapSpacing.Y);
                yield return new DxfCodePair(15, GridSpacing.X);
                yield return new DxfCodePair(25, GridSpacing.Y);
                yield return new DxfCodePair(16, ViewDirection.X);
                yield return new DxfCodePair(26, ViewDirection.Y);
                yield return new DxfCodePair(36, ViewDirection.Z);
                yield return new DxfCodePair(17, TargetViewPoint.X);
                yield return new DxfCodePair(27, TargetViewPoint.Y);
                yield return new DxfCodePair(37, TargetViewPoint.Z);
                yield return new DxfCodePair(40, ViewHeight);
                yield return new DxfCodePair(41, ViewPortAspectRatio);
                yield return new DxfCodePair(42, LensLength);
                yield return new DxfCodePair(43, FrontClippingPlane);
                yield return new DxfCodePair(44, BackClippingPlane);
                yield return new DxfCodePair(50, SnapRotationAngle);
                yield return new DxfCodePair(51, ViewTwistAngle);
            }
        }

        public static DxfViewPort FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            var viewPort = new DxfViewPort();
            foreach (var p in pairs)
            {
                switch (p.Code)
                {
                    case 2:
                        viewPort.Name = p.StringValue;
                        break;
                    case 10:
                        viewPort.LowerLeft.X = p.DoubleValue;
                        break;
                    case 20:
                        viewPort.LowerLeft.Y = p.DoubleValue;
                        break;
                    case 11:
                        viewPort.UpperRight.X = p.DoubleValue;
                        break;
                    case 21:
                        viewPort.UpperRight.Y = p.DoubleValue;
                        break;
                    case 12:
                        viewPort.ViewCenter.X = p.DoubleValue;
                        break;
                    case 22:
                        viewPort.ViewCenter.Y = p.DoubleValue;
                        break;
                    case 13:
                        viewPort.SnapBasePoint.X = p.DoubleValue;
                        break;
                    case 23:
                        viewPort.SnapBasePoint.Y = p.DoubleValue;
                        break;
                    case 14:
                        viewPort.SnapSpacing.X = p.DoubleValue;
                        break;
                    case 24:
                        viewPort.SnapSpacing.Y = p.DoubleValue;
                        break;
                    case 15:
                        viewPort.GridSpacing.X = p.DoubleValue;
                        break;
                    case 25:
                        viewPort.GridSpacing.Y = p.DoubleValue;
                        break;
                    case 16:
                        viewPort.ViewDirection.X = p.DoubleValue;
                        break;
                    case 26:
                        viewPort.ViewDirection.Y = p.DoubleValue;
                        break;
                    case 36:
                        viewPort.ViewDirection.Z = p.DoubleValue;
                        break;
                    case 17:
                        viewPort.TargetViewPoint.X = p.DoubleValue;
                        break;
                    case 27:
                        viewPort.TargetViewPoint.Y = p.DoubleValue;
                        break;
                    case 37:
                        viewPort.TargetViewPoint.Z = p.DoubleValue;
                        break;
                    case 40:
                        viewPort.ViewHeight = p.DoubleValue;
                        break;
                    case 41:
                        viewPort.ViewPortAspectRatio = p.DoubleValue;
                        break;
                    case 42:
                        viewPort.LensLength = p.DoubleValue;
                        break;
                    case 43:
                        viewPort.FrontClippingPlane = p.DoubleValue;
                        break;
                    case 44:
                        viewPort.BackClippingPlane = p.DoubleValue;
                        break;
                    case 50:
                        viewPort.SnapRotationAngle = p.DoubleValue;
                        break;
                    case 51:
                        viewPort.ViewTwistAngle = p.DoubleValue;
                        break;
                }
            }

            return viewPort;
        }
    }
}
