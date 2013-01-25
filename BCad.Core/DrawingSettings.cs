using System;
using System.Diagnostics;
using BCad.Helpers;

namespace BCad
{
    public class DrawingSettings
    {
        private readonly string fileName;
        private readonly UnitFormat unitFormat;
        private readonly int unitPrecision;

        public string FileName { get { return this.fileName; } }
        public UnitFormat UnitFormat { get { return this.unitFormat; } }
        public int UnitPrecision { get { return this.unitPrecision; } }

        public DrawingSettings()
            : this(null, UnitFormat.None, -1)
        {
        }

        public DrawingSettings(string path, UnitFormat unitFormat, int unitPrecision)
        {
            this.fileName = path;
            this.unitFormat = unitFormat;
            this.unitPrecision = unitPrecision;
        }

        public DrawingSettings Update(string fileName = null, UnitFormat? unitFormat = null, int? unitPrecision = null)
        {
            return new DrawingSettings(
                fileName ?? this.fileName,
                unitFormat ?? this.unitFormat,
                unitPrecision ?? this.unitPrecision);
        }

        public static string FormatUnits(double value, UnitFormat unitFormat, int unitPrecision)
        {
            switch (unitFormat)
            {
                case BCad.UnitFormat.None:
                    return value.ToString();
                case BCad.UnitFormat.Architectural:
                    return FormatArchitectural(value, unitPrecision);
                case BCad.UnitFormat.Metric:
                    return FormatMetric(value, unitPrecision);
                default:
                    throw new ArgumentException("value");
            }
        }

        public static bool TryParseUnits(string text, out double value)
        {
            throw new NotImplementedException();
        }

        private static string FormatMetric(double value, int precision)
        {
            // precision is requested decimal places
            Debug.Assert(precision >= 0 && precision < decimalFormats.Length);
            return value.ToString(decimalFormats[precision]);
        }

        private static string FormatArchitectural(double value, int precision)
        {
            var feet = (int)value / 12;
            var inches = (int)value % 12;
            var frac = value - ((double)(int)value);

            int numerator = 0, denominator = 0;
            switch (precision)
            {
                case 2: // 1/2
                case 4: // 1/4
                case 8: // 1/8
                case 16: // 1/16
                case 32: // 1/32
                    int i = 0;
                    for (i = 0; i < precision; i++)
                    {
                        var limit = (double)i / precision;
                        if (frac < limit)
                        {
                            i--;
                            break;
                        }
                    }
                    numerator = i;
                    denominator = precision;
                    if (numerator == denominator)
                    {
                        numerator = denominator = 0;
                        inches++;
                    }
                    break;
                default:
                    // unsupported fractional part
                    break;
            }

            while (inches >= 12)
            {
                inches -= 12;
                feet++;
            }

            ReduceFraction(ref numerator, ref denominator);
            string fractional = string.Empty;
            if (numerator != 0 && denominator != 0)
                fractional = string.Format("-{0}/{1}", numerator, denominator);

            string formatted = string.Format("{0}'{1}{2}\"", feet, inches, fractional);
            return formatted;
        }

        private static void ReduceFraction(ref int numerator, ref int denominator)
        {
            if (numerator == 0 || denominator == 0)
                return;
            while (numerator % 2 == 0 && denominator % 2 == 0)
            {
                numerator /= 2;
                denominator /= 2;
            }
        }

        private static string[] decimalFormats = new string[]
        {
            "#",
            ".#",
            ".##",
            ".###",
            ".####",
            ".#####",
            ".######",
            ".#######",
            ".########",
            ".#########",
            ".##########",
            ".###########",
            ".############",
            ".#############",
            ".##############",
            ".###############",
            ".################",
        };
    }
}
