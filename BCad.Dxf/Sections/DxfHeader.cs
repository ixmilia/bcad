using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static DateTime FromOADate(double value)
        {
            return new DateTime(DoubleDateToTicks(value), DateTimeKind.Unspecified);
        }

        private static double ToOADate(DateTime d)
        {
            return TicksToOADate(d.Ticks);
        }

        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;
        private const long MinTicks = 0;
        private const long MaxTicks = DaysTo10000 * TicksPerDay - 1;
        private const long MaxMillis = (long)DaysTo10000 * MillisPerDay;
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;
        private const int DaysPerYear = 365;
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;  // 3652059
        private const long DoubleDateOffset = DaysTo1899 * TicksPerDay;

        private static long DoubleDateToTicks(double value)
        {
            // Conversion to long will not cause an overflow here, as at this point the "value" is in between OADateMinAsDouble and OADateMaxAsDouble
            long millis = (long)(value * MillisPerDay + (value >= 0 ? 0.5 : -0.5));
            // The interesting thing here is when you have a value like 12.5 it all positive 12 days and 12 hours from 01/01/1899
            // However if you a value of -12.25 it is minus 12 days but still positive 6 hours, almost as though you meant -11.75 all negative
            // This line below fixes up the millis in the negative case
            if (millis < 0)
            {
                millis -= (millis % MillisPerDay) * 2;
            }

            millis += DoubleDateOffset / TicksPerMillisecond;

            return millis * TicksPerMillisecond;
        }

        private static double TicksToOADate(long value)
        {
            if (value == 0)
                return 0.0;  // Returns OleAut's zero'ed date value.
            if (value < TicksPerDay) // This is a fix for VB. They want the default day to be 1/1/0001 rathar then 12/30/1899.
                value += DoubleDateOffset; // We could have moved this fix down but we would like to keep the bounds check.
            // Currently, our max date == OA's max date (12/31/9999), so we don't 
            // need an overflow check in that direction.
            long millis = (value - DoubleDateOffset) / TicksPerMillisecond;
            if (millis < 0)
            {
                long frac = millis % MillisPerDay;
                if (frac != 0) millis -= (MillisPerDay + frac) * 2;
            }
            return (double)millis / MillisPerDay;
        }
    }
}
