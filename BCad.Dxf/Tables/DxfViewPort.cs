using System;
using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfViewPort
    {
        public const string ViewPortText = "VPORT";

        public const string ActiveViewPortName = "*ACTIVE";

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
            Name = ActiveViewPortName;
            LowerLeft = new DxfPoint();
            UpperRight = new DxfPoint();
            ViewCenter = new DxfPoint();
            SnapBasePoint = new DxfPoint();
            SnapSpacing = new DxfVector();
            GridSpacing = new DxfVector();
            ViewDirection = DxfVector.ZAxis;
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
                var pairs = new List<DxfCodePair>();
                Action<int, object, object> addIfNotDefault = (code, actualValue, defaultValue) =>
                    {
                        if (!actualValue.Equals(defaultValue))
                            pairs.Add(new DxfCodePair(code, actualValue));
                    };

                pairs.Add(new DxfCodePair(2, Name ?? ActiveViewPortName));
                addIfNotDefault(10, LowerLeft.X, 0.0);
                addIfNotDefault(20, LowerLeft.Y, 0.0);
                addIfNotDefault(11, UpperRight.X, 0.0);
                addIfNotDefault(21, UpperRight.Y, 0.0);
                addIfNotDefault(12, ViewCenter.X, 0.0);
                addIfNotDefault(22, ViewCenter.Y, 0.0);
                addIfNotDefault(13, SnapBasePoint.X, 0.0);
                addIfNotDefault(23, SnapBasePoint.Y, 0.0);
                addIfNotDefault(14, SnapSpacing.X, 0.0);
                addIfNotDefault(24, SnapSpacing.Y, 0.0);
                addIfNotDefault(15, GridSpacing.X, 0.0);
                addIfNotDefault(25, GridSpacing.Y, 0.0);
                addIfNotDefault(16, ViewDirection.X, 0.0);
                addIfNotDefault(26, ViewDirection.Y, 0.0);
                addIfNotDefault(36, ViewDirection.Z, 1.0);
                addIfNotDefault(17, TargetViewPoint.X, 0.0);
                addIfNotDefault(27, TargetViewPoint.Y, 0.0);
                addIfNotDefault(37, TargetViewPoint.Z, 0.0);
                addIfNotDefault(40, ViewHeight, 0.0);
                addIfNotDefault(41, ViewPortAspectRatio, 0.0);
                addIfNotDefault(42, LensLength, 0.0);
                addIfNotDefault(43, FrontClippingPlane, 0.0);
                addIfNotDefault(44, BackClippingPlane, 0.0);
                addIfNotDefault(50, SnapRotationAngle, 0.0);
                addIfNotDefault(51, ViewTwistAngle, 0.0);

                return pairs;
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
