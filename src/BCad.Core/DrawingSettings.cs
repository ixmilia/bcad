// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

        private static int[] AllowedArchitecturalPrecisions = new[] { 0, 2, 4, 8, 16, 32 };

        public DrawingSettings()
            : this(null, UnitFormat.Architectural, 16)
        {
        }

        public DrawingSettings(string path, UnitFormat unitFormat, int unitPrecision)
        {
            this.fileName = path;
            this.unitFormat = unitFormat;
            this.unitPrecision = unitPrecision < 0 ? 0 : unitPrecision;

            switch (unitFormat)
            {
                case UnitFormat.Architectural:
                    // only allowable values are 0, 2, 4, 8, 16, 32
                    this.unitPrecision = AllowedArchitecturalPrecisions.Where(x => x <= this.unitPrecision).Max();
                    break;
                case UnitFormat.Metric:
                    // only allowable values are [0, 16]
                    this.unitPrecision = Math.Max(0, this.unitPrecision);
                    this.unitPrecision = Math.Min(16, this.unitPrecision);
                    break;
            }
        }

        public DrawingSettings Update(string fileName = null, Optional<UnitFormat> unitFormat = default(Optional<UnitFormat>), Optional<int> unitPrecision = default(Optional<int>))
        {
            var newFileName = fileName ?? this.fileName;
            var newUnitFormat = unitFormat.HasValue ? unitFormat.Value : this.unitFormat;
            var newUnitPrecision = unitPrecision.HasValue ? unitPrecision.Value : this.unitPrecision;

            if (newFileName == this.fileName &&
                newUnitFormat == this.unitFormat &&
                newUnitPrecision == this.unitPrecision)
            {
                return this;
            }

            return new DrawingSettings(newFileName, newUnitFormat, newUnitPrecision);
        }

        public static string FormatUnits(double value, UnitFormat unitFormat, int unitPrecision)
        {
            var prefix = Math.Sign(value) < 0 ? "-" : "";
            value = Math.Abs(value);
            switch (unitFormat)
            {
                case UnitFormat.Architectural:
                    return string.Concat(prefix, FormatArchitectural(value, unitPrecision));
                case UnitFormat.Metric:
                    return string.Concat(prefix, FormatMetric(value, unitPrecision));
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

            var isNegative = false;
            if (text[0] == '-')
            {
                text = text.Substring(1);
                isNegative = true;
            }

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
                if (isNegative)
                {
                    value *= -1.0;
                }

                return true;
            }

            match = MixedArchitecturalPattern.Match(text);
            if (match.Success)
            {
                Debug.Assert(match.Groups.Count == 4);
                var feet = ParseIntAsDouble(match.Groups[1].Value);
                var inches = double.Parse(match.Groups[2].Value);
                value = (feet * 12.0) + inches;
                if (isNegative)
                {
                    value *= -1.0;
                }

                return true;
            }

            return false;
        }

        private static Regex FullArchitecturalPattern = new Regex(@"^\s*((\d+)')?(\d+)?((-?(\d+)/(\d+))?"")?\s*$");
        //                                                              12       3     45  6     7
        //                                                               feet '  inches  -  num /denom  "

        private static Regex MixedArchitecturalPattern = new Regex(@"^\s*(\d+)'(\d+(\.\d+)?)""?\s*$");
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
            return value == 0.0
                ? "0"
                : value.ToString(decimalFormats[precision]);
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
