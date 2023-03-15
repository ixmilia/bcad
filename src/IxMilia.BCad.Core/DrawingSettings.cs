using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace IxMilia.BCad
{
    public class DrawingSettings
    {
        public string FileName { get; private set; }
        public DrawingUnits DrawingUnits { get; private set; }
        public UnitFormat UnitFormat { get; private set; }
        public int UnitPrecision { get; private set; }
        public int AnglePrecision { get; private set; }
        public string CurrentLayerName { get; private set; }
        public double FilletRadius { get; private set; }
        public LineTypeSpecification CurrentLineTypeSpecification { get; private set; }

        public DrawingSettings()
            : this(null, DrawingUnits.English, UnitFormat.Architectural, 4, 0, "0", 0.0, null)
        {
        }

        public DrawingSettings(string path, DrawingUnits drawingUnits, UnitFormat unitFormat, int unitPrecision, int anglePrecision, string currentLayerName, double filletRadius, LineTypeSpecification currentLineTypeSpecification)
        {
            FileName = path;
            DrawingUnits = drawingUnits;
            UnitFormat = unitFormat;
            UnitPrecision = unitPrecision < 0 ? 0 : unitPrecision;
            AnglePrecision = anglePrecision < 0 ? 0 : anglePrecision;
            CurrentLayerName = currentLayerName;
            FilletRadius = filletRadius;
            CurrentLineTypeSpecification = currentLineTypeSpecification;

            switch (unitFormat)
            {
                case UnitFormat.Architectural:
                    // only allowable values are [0-8]
                    UnitPrecision = Math.Min(Math.Max(0, UnitPrecision), 8);
                    break;
                case UnitFormat.Decimal:
                    // only allowable values are [0, 16]
                    UnitPrecision = Math.Max(0, UnitPrecision);
                    UnitPrecision = Math.Min(16, UnitPrecision);
                    break;
            }

            if (FilletRadius < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(filletRadius));
            }
        }

        public DrawingSettings Update(
            string fileName = null,
            Optional<DrawingUnits> drawingUnits = default,
            Optional<UnitFormat> unitFormat = default,
            Optional<int> unitPrecision = default,
            Optional<int> anglePrecision = default,
            Optional<string> currentLayerName = default,
            Optional<double> filletRadius = default,
            Optional<LineTypeSpecification> currentLineTypeSpecification = default)
        {
            var newFileName = fileName ?? FileName;
            var newDrawingUnits = drawingUnits.GetValue(DrawingUnits);
            var newUnitFormat = unitFormat.GetValue(UnitFormat);
            var newUnitPrecision = unitPrecision.GetValue(UnitPrecision);
            var newAnglePrecision = anglePrecision.GetValue(AnglePrecision);
            var newCurrentLayerName = currentLayerName.GetValue(CurrentLayerName);
            var newFilletRadius = filletRadius.GetValue(FilletRadius);
            var newCurrentLineTypeSpecification = currentLineTypeSpecification.GetValue(CurrentLineTypeSpecification);

            if (newFileName == FileName &&
                newDrawingUnits == DrawingUnits &&
                newUnitFormat == UnitFormat &&
                newUnitPrecision == UnitPrecision &&
                newAnglePrecision == AnglePrecision &&
                newCurrentLayerName == CurrentLayerName &&
                newFilletRadius == FilletRadius &&
                newCurrentLineTypeSpecification == CurrentLineTypeSpecification)
            {
                return this;
            }

            return new DrawingSettings(newFileName, newDrawingUnits, newUnitFormat, newUnitPrecision, newAnglePrecision, newCurrentLayerName, newFilletRadius, newCurrentLineTypeSpecification);
        }

        public static string FormatAngle(double value, int anglePrecision) => FormatScalar(value, anglePrecision);

        public static string FormatUnits(double value, DrawingUnits drawingUnits, UnitFormat unitFormat, int unitPrecision)
        {
            var prefix = Math.Sign(value) < 0 ? "-" : "";
            value = Math.Abs(value);
            switch (unitFormat)
            {
                case UnitFormat.Architectural:
                    return string.Concat(prefix, FormatArchitectural(value, unitPrecision));
                case UnitFormat.Fractional:
                    return string.Concat(prefix, FormatFractional(value, unitPrecision));
                case UnitFormat.Decimal:
                    return string.Concat(prefix, FormatScalar(value, unitPrecision));
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

        private static string FormatScalar(double value, int precision)
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
            var inches = value - (feet * 12.0);
            var fractional = FormatFractional(inches, precision);
            return $"{feet}'{fractional}";
        }

        private static string FormatFractional(double value, int precision)
        {
            if (value < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive");
            }

            var roundingAmount = Math.Pow(2.0, -(precision + 1));
            var normalizedValue = value + roundingAmount;
            var whole = (int)normalizedValue;
            var fractionalPart = normalizedValue - whole;

            var denominator = (int)Math.Pow(2.0, precision);
            var numerator = (int)(fractionalPart * denominator);

            if (numerator == denominator)
            {
                numerator = denominator = 0;
            }

            ReduceFraction(ref numerator, ref denominator);
            var fractional = numerator != 0
                ? $"-{numerator}/{denominator}"
                : string.Empty;

            var formatted = $"{whole}{fractional}\"";
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

        private static int MaxFractionalPrecision = 8;

        private static double[] PrecisionLimits = Enumerable.Range(1, MaxFractionalPrecision).Select(p => Math.Pow(2.0, -p)).ToArray();

        private static string[] decimalFormats = new string[]
        {
            "#",
            "0.#",
            "0.##",
            "0.###",
            "0.####",
            "0.#####",
            "0.######",
            "0.#######",
            "0.########",
            "0.#########",
            "0.##########",
            "0.###########",
            "0.############",
            "0.#############",
            "0.##############",
            "0.###############",
            "0.################",
        };
    }
}
