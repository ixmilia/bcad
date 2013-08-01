using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Dxf
{
    public partial class DxfHeader
    {
        // object snap flags
        public bool EndPointSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 1); }
            set { SetFlag(value, 1); }
        }

        public bool MidPointSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 2); }
            set { SetFlag(value, 2); }
        }

        public bool CenterSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 4); }
            set { SetFlag(value, 4); }
        }

        public bool NodeSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 8); }
            set { SetFlag(value, 8); }
        }

        public bool QuadrantSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 16); }
            set { SetFlag(value, 16); }
        }

        public bool IntersectionSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 32); }
            set { SetFlag(value, 32); }
        }

        public bool InsertionSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 64); }
            set { SetFlag(value, 64); }
        }

        public bool PerpendicularSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 128); }
            set { SetFlag(value, 128); }
        }

        public bool TangentSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 256); }
            set { SetFlag(value, 256); }
        }

        public bool NearestSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 512); }
            set { SetFlag(value, 512); }
        }

        public bool ApparentIntersectionSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 2048); }
            set { SetFlag(value, 2048); }
        }

        public bool ExtensionSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 4096); }
            set { SetFlag(value, 4096); }
        }

        public bool ParallelSnap
        {
            get { return DxfHelpers.GetFlag(ObjectSnapFlags, 8192); }
            set { SetFlag(value, 8192); }
        }

        private void SetFlag(bool value, int mask)
        {
            var flags = ObjectSnapFlags;
            DxfHelpers.SetFlag(value, ref flags, mask);
            ObjectSnapFlags = flags;
        }

        private static bool BoolShort(short s)
        {
            return s != 0;
        }

        private static short BoolShort(bool b)
        {
            return (short)(b ? 1 : 0);
        }

        private const double JulianOffset = 2415018.999733797;

        private static DateTime DateDouble(double d)
        {
            var offset = d - JulianOffset;
            if (offset < 0.0)
                offset = 0.0;
            return DateTime.FromOADate(offset);
        }

        private static double DateDouble(DateTime d)
        {
            return d.ToOADate() + JulianOffset;
        }

        private static TimeSpan TimeSpanDouble(double d)
        {
            return TimeSpan.FromDays(d);
        }

        private static double TimeSpanDouble(TimeSpan t)
        {
            return t.TotalDays;
        }

        private static short RawValue(DxfColor c)
        {
            return c.RawValue;
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
