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

            switch (precision)
            {
                case 0: // no fractional inches
                    break;
                case 2: // 1/2
                    break;
                case 4: // 1/4
                    break;
                case 8: // 1/8
                    break;
                case 16: // 1/16
                    break;
                case 32: // 1/32
                    break;
            }
            return null;
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
