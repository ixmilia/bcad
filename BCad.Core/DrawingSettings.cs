using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
            value = default(double);
            if (string.IsNullOrWhiteSpace(text))
                return false;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return true;

            var match = FullArchitecturalPattern.Match(text);
            if (match.Success)
            {
                Debug.Assert(match.Groups.Count == 8); // group 0 is the whole string
                var feet = ParseIntAsDouble(match.Groups[2].Value);
                var inches = ParseIntAsDouble(match.Groups[3].Value);
                var num = ParseIntAsDouble(match.Groups[6].Value);
                var denom = ParseIntAsDouble(match.Groups[7].Value);
                if (denom == 0.0) denom = 1.0; // hack to prevent zero division
                value = (feet * 12.0) + inches + (num / denom);
                return true;
            }

            match = MixedArchitecturalPattern.Match(text);
            if (match.Success)
            {
                Debug.Assert(match.Groups.Count == 4);
                var feet = ParseIntAsDouble(match.Groups[1].Value);
                var inches = double.Parse(match.Groups[2].Value);
                value = (feet * 12.0) + inches;
                return true;
            }

            return false;
        }

        private static Regex FullArchitecturalPattern = new Regex(@"^\s*((\d+)')?(\d+)?((-?(\d+)/(\d+))?"")?\s*$", RegexOptions.Compiled);
        //                                                              12       3     45  6     7
        //                                                               feet '  inches  -  num /denom  "

        private static Regex MixedArchitecturalPattern = new Regex(@"^\s*(\d+)'(\d+(\.\d+)?)""?\s*$", RegexOptions.Compiled);
        //                                                               1     2   3
        //                                                               feet 'inches.partial"

        private static double ParseIntAsDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0.0;
            return (double)int.Parse(text);
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
