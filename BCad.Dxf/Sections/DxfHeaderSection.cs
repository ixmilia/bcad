using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Sections
{
    public enum DxfUnitFormat
    {
        Scientific = 1,
        Decimal = 2,
        Engineering = 3,
        ArchitecturalStacked = 4,
        FractionalStacked = 5,
        Architectural = 6,
        Fractional = 7,
    }

    public enum DxfAngleDirection
    {
        CounterClockwise = 0,
        Clockwise = 1
    }

    public enum DxfAttributeVisibility
    {
        None = 0,
        Normal = 1,
        All = 2
    }

    public enum DxfJustification
    {
        Top = 0,
        Middle = 1,
        Bottom = 2
    }

    public enum DxfDimensionTextJustification
    {
        AboveLineCenter = 0,
        AboveLineNextToFirstExtension = 1,
        AboveLineNextToSecondExtension = 2,
        AboveLineCenteredOnFirstExtension = 3,
        AboveLineCenteredOnSecondExtension = 4
    }

    public enum DxfCoordinateDisplay
    {
        Static = 0,
        ContinuousUpdate = 1,
        DistanceAngleFormat = 2
    }

    public enum DxfUnitZeroSuppression
    {
        SuppressZeroFeetAndZeroInches = 0,
        IncludeZeroFeetAndZeroInches = 1,
        IncludeZeroFeetAndSuppressZeroInches = 2,
        IncludeZeroInchesAndSuppressZeroFeet = 3
    }

    public enum DxfAngleFormat
    {
        DecimalDegrees = 0,
        DegreesMinutesSeconds = 1,
        Gradians = 2,
        Radians = 3,
        SurveyorsUnits = 4
    }

    public enum DxfDimensionFit
    {
        TextAndArrowsOutsideLines = 0,
        MoveArrowsFirst = 1,
        MoveTextFirst = 2,
        MoveEitherForBestFit = 3
    }

    public enum DxfDragMode
    {
        Off = 0,
        On = 1,
        Auto = 2
    }

    public enum DxfDrawingUnits
    {
        English = 0,
        Metric = 1
    }

    public enum DxfPickStyle
    {
        None = 0,
        Group = 1,
        AssociativeHatch = 2,
        GroupAndAssociativeHatch = 3
    }

    public enum DxfShadeEdgeMode
    {
        FacesShadedEdgeNotHighlighted = 0,
        FacesShadedEdgesHighlightedInBlack = 1,
        FacesNotFilledEdgesInEntityColor = 2,
        FacesInEntityColorEdgesInBlack = 3
    }

    public enum DxfPolySketchMode
    {
        SketchLines = 0,
        SketchPolylines = 1
    }

    public partial class DxfHeaderSection : DxfSection
    {
        public DxfHeaderSection()
        {
            SetDefaults();
        }

        public override DxfSectionType Type
        {
            get { return DxfSectionType.Header; }
        }

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs()
        {
            var values = new List<DxfCodePair>();
            AddValueToList(values, this);
            return values;
        }

        // object snap flags

        public bool EndPointSnap
        {
            get { return GetFlag(ObjectSnapFlags, 1); }
            set { SetFlag(value, 1); }
        }

        public bool MidPointSnap
        {
            get { return GetFlag(ObjectSnapFlags, 2); }
            set { SetFlag(value, 2); }
        }

        public bool CenterSnap
        {
            get { return GetFlag(ObjectSnapFlags, 4); }
            set { SetFlag(value, 4); }
        }

        public bool NodeSnap
        {
            get { return GetFlag(ObjectSnapFlags, 8); }
            set { SetFlag(value, 8); }
        }

        public bool QuadrantSnap
        {
            get { return GetFlag(ObjectSnapFlags, 16); }
            set { SetFlag(value, 16); }
        }

        public bool IntersectionSnap
        {
            get { return GetFlag(ObjectSnapFlags, 32); }
            set { SetFlag(value, 32); }
        }

        public bool InsertionSnap
        {
            get { return GetFlag(ObjectSnapFlags, 64); }
            set { SetFlag(value, 64); }
        }

        public bool PerpendicularSnap
        {
            get { return GetFlag(ObjectSnapFlags, 128); }
            set { SetFlag(value, 128); }
        }

        public bool TangentSnap
        {
            get { return GetFlag(ObjectSnapFlags, 256); }
            set { SetFlag(value, 256); }
        }

        public bool NearestSnap
        {
            get { return GetFlag(ObjectSnapFlags, 512); }
            set { SetFlag(value, 512); }
        }

        public bool ApparentIntersectionSnap
        {
            get { return GetFlag(ObjectSnapFlags, 2048); }
            set { SetFlag(value, 2048); }
        }

        public bool ExtensionSnap
        {
            get { return GetFlag(ObjectSnapFlags, 4096); }
            set { SetFlag(value, 4096); }
        }

        public bool ParallelSnap
        {
            get { return GetFlag(ObjectSnapFlags, 8192); }
            set { SetFlag(value, 8192); }
        }

        private void SetFlag(bool value, int mask)
        {
            var flags = ObjectSnapFlags;
            if (value) SetFlag(ref flags, mask);
            else ClearFlag(ref flags, mask);
            ObjectSnapFlags = flags;
        }

        private static void SetFlag(ref int flags, int mask)
        {
            flags |= mask;
        }

        private static void ClearFlag(ref int flags, int mask)
        {
            flags &= ~mask;
        }

        private static bool GetFlag(int flags, int mask)
        {
            return (flags & mask) != 0;
        }

        internal static bool BoolShort(short s)
        {
            return s != 0;
        }

        internal static short BoolShort(bool b)
        {
            return (short)(b ? 1 : 0);
        }

        private const double JulianOffset = 2415018.999733797;

        internal static DateTime DateDouble(double d)
        {
            return DateTime.FromOADate(d - JulianOffset);
        }

        internal static double DateDouble(DateTime d)
        {
            return d.ToOADate() + JulianOffset;
        }

        internal static TimeSpan TimeSpanDouble(double d)
        {
            return TimeSpan.FromDays(d);
        }

        internal static double TimeSpanDouble(TimeSpan t)
        {
            return t.TotalDays;
        }

        internal static short RawValue(DxfColor c)
        {
            return c.RawValue;
        }

        internal static DxfHeaderSection HeaderSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfHeaderSection();
            string keyName = null;
            Func<short, bool> shortToBool = value => value != 0;

            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfCodePair.IsSectionEnd(pair))
                {
                    // done reading settings
                    break;
                }

                if (pair.Code == 9)
                {
                    // what setting to get
                    keyName = pair.StringValue;
                }
                else
                {
                    SetHeaderVariable(keyName, pair, section);
                }
            }

            return section;
        }

        private static void EnsureCode(DxfCodePair pair, int code)
        {
            if (pair.Code != code)
            {
                throw new DxfReadException(string.Format("Expected code {0}, got {1}", code, pair.Code));
            }
        }

        private static void SetPoint(DxfCodePair pair, DxfPoint point)
        {
            switch (pair.Code)
            {
                case 10:
                    point.X = pair.DoubleValue;
                    break;
                case 20:
                    point.Y = pair.DoubleValue;
                    break;
                case 30:
                    point.Z = pair.DoubleValue;
                    break;
                default:
                    break;
            }
        }
    }
}
