using System;
using System.Diagnostics;

namespace BCad.Dxf
{
    public partial class DxfHeader
    {
        internal DxfHeader()
        {
            SetDefaults();
        }

        public bool IsViewportScaledToFit
        {
            get { return ViewportViewScaleFactor == 0.0; }
            set { ViewportViewScaleFactor = value ? 0.0 : 1.0; }
        }

        public object this[string variableName]
        {
            get { return GetValue(variableName); }
            set { SetValue(variableName, value); }
        }

        private static bool BoolShort(short s)
        {
            return s != 0;
        }

        private static short BoolShort(bool b)
        {
            return (short)(b ? 1 : 0);
        }

        private static string GuidString(Guid g)
        {
            return g.ToString();
        }

        private static Guid GuidString(string s)
        {
            return new Guid(s);
        }

        private const double JulianOffset = 2415018.999733797;

        private static DateTime DateDouble(double d)
        {
            var offset = d - JulianOffset;
            if (offset < 0.0)
                offset = 0.0;
            return FromOADate(offset);
        }

        private static double DateDouble(DateTime d)
        {
            return ToOADate(d) + JulianOffset;
        }

        private static TimeSpan TimeSpanDouble(double d)
        {
            return TimeSpan.FromDays(d);
        }

        private static double TimeSpanDouble(TimeSpan t)
        {
            return t.TotalDays;
        }

        private static void EnsureCode(DxfCodePair pair, int code)
        {
            if (pair.Code != code)
            {
                Debug.Assert(false, string.Format("Expected code {0}, got {1}", code, pair.Code));
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

        private static DateTime FromOADate(double value)
        {
            return new DateTime(DoubleDateToTicks(value), DateTimeKind.Unspecified);
        }

        private static double ToOADate(DateTime d)
        {
            return TicksToOADate(d.Ticks);
        }

        private const long TicksPerMillisecond = 10000;
        private const long TicksPerDay = TicksPerMillisecond * 1000 * 60 * 60 * 24;
        private const int MillisecondsPerDay = 1000 * 60 * 60 * 24;
        private const long DoubleDateOffset = (146097 * 4 + 36524 * 3 - 367) * TicksPerDay;

        private static long DoubleDateToTicks(double value)
        {
            var milliseconds = (long)(value * MillisecondsPerDay + (value >= 0 ? 0.5 : -0.5));
            if (milliseconds < 0)
            {
                milliseconds -= (milliseconds % MillisecondsPerDay) * 2;
            }

            milliseconds += DoubleDateOffset / TicksPerMillisecond;

            return milliseconds * TicksPerMillisecond;
        }

        private static double TicksToOADate(long value)
        {
            if (value == 0)
            {
                return 0.0;
            }

            if (value < TicksPerDay)
            {
                value += DoubleDateOffset;
            }

            var milliseconds = (value - DoubleDateOffset) / TicksPerMillisecond;
            if (milliseconds < 0)
            {
                var frac = milliseconds % MillisecondsPerDay;
                if (frac != 0)
                {
                    milliseconds -= (MillisecondsPerDay + frac) * 2;
                }
            }

            return (double)milliseconds / MillisecondsPerDay;
        }
    }
}
