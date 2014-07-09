using System;
using System.Collections.Generic;

namespace BCad.Dxf
{
    public class DxfView : DxfSymbolTableFlags
    {
        internal const string AcDbViewTableRecordText = "AcDbViewTableRecord";

        public string Name { get; set; }

        public bool IsPaperspaceView
        {
            get { return DxfHelpers.GetFlag(Flags, 1); }
            set { DxfHelpers.SetFlag(value, ref Flags, 1); }
        }

        public double ViewHeight { get; set; }
        public DxfPoint CenterPoint { get; set; }
        public double ViewWidth { get; set; }
        public DxfVector ViewDirection { get; set; }
        public DxfPoint TargetPoint { get; set; }
        public double LensLength { get; set; }
        public double FrontClippingOffsetFromTarget { get; set; }
        public double BackClippingOffsetFromTarget { get; set; }
        public double TwistAngle { get; set; }
        public DxfViewMode ViewMode { get; set; }

        public DxfView()
        {
            CenterPoint = DxfPoint.Origin;
            ViewDirection = DxfVector.ZAxis;
            TargetPoint = DxfPoint.Origin;
        }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            var list = new List<DxfCodePair>();
            Action<int, object> add = (code, value) => list.Add(new DxfCodePair(code, value));
            add(100, AcDbViewTableRecordText);
            add(2, Name);
            add(70, (short)Flags);
            add(40, ViewHeight);
            add(10, CenterPoint.X);
            add(20, CenterPoint.Y);
            add(41, ViewWidth);
            add(11, ViewDirection.X);
            add(21, ViewDirection.Y);
            add(31, ViewDirection.Z);
            add(12, TargetPoint.X);
            add(22, TargetPoint.Y);
            add(32, TargetPoint.Z);
            add(42, LensLength);
            add(43, FrontClippingOffsetFromTarget);
            add(44, BackClippingOffsetFromTarget);
            add(50, TwistAngle);
            add(71, (short)ViewMode.Value);

            return list;
        }

        internal static DxfView FromBuffer(DxfCodePairBufferReader buffer)
        {
            var view = new DxfView();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                switch (pair.Code)
                {
                    case 2:
                        view.Name = pair.StringValue;
                        break;
                    case 10:
                        view.CenterPoint.X = pair.DoubleValue;
                        break;
                    case 11:
                        view.ViewDirection.X = pair.DoubleValue;
                        break;
                    case 12:
                        view.TargetPoint.X = pair.DoubleValue;
                        break;
                    case 20:
                        view.CenterPoint.Y = pair.DoubleValue;
                        break;
                    case 21:
                        view.ViewDirection.Y = pair.DoubleValue;
                        break;
                    case 22:
                        view.TargetPoint.Y = pair.DoubleValue;
                        break;
                    case 31:
                        view.ViewDirection.Z = pair.DoubleValue;
                        break;
                    case 32:
                        view.TargetPoint.Z = pair.DoubleValue;
                        break;
                    case 40:
                        view.ViewHeight = pair.DoubleValue;
                        break;
                    case 41:
                        view.ViewWidth = pair.DoubleValue;
                        break;
                    case 42:
                        view.LensLength = pair.DoubleValue;
                        break;
                    case 43:
                        view.FrontClippingOffsetFromTarget = pair.DoubleValue;
                        break;
                    case 44:
                        view.BackClippingOffsetFromTarget = pair.DoubleValue;
                        break;
                    case 50:
                        view.TwistAngle = pair.DoubleValue;
                        break;
                    case 70:
                        view.Flags = pair.ShortValue;
                        break;
                    case 71:
                        view.ViewMode = new DxfViewMode(pair.ShortValue);
                        break;
                }
            }

            return view;
        }
    }
}
