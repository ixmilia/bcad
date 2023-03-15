using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class UnitFormatTests
    {
        [Theory]
        [InlineData(3.0, "3", 0, DrawingUnits.English, UnitFormat.Decimal)] // nearest whole number
        [InlineData(3.4, "3", 0, DrawingUnits.English, UnitFormat.Decimal)]
        [InlineData(3.5, "4", 0, DrawingUnits.English, UnitFormat.Decimal)]
        [InlineData(3.0, "3", 3, DrawingUnits.English, UnitFormat.Decimal)] // 3 decimal places
        [InlineData(3.14159, "3.142", 3, DrawingUnits.English, UnitFormat.Decimal)]
        [InlineData(3.14159, "3.1416", 4, DrawingUnits.English, UnitFormat.Decimal)] // 4 decimal places
        [InlineData(-3.0, "-3", 4, DrawingUnits.English, UnitFormat.Decimal)] // negative value
        [InlineData(1.578E-12, "0", 4, DrawingUnits.English, UnitFormat.Decimal)] // really close to zero
        [InlineData(0.0, "0'0\"", 0, DrawingUnits.English, UnitFormat.Architectural)] // nearest inch
        [InlineData(15.2, "1'3\"", 0, DrawingUnits.English, UnitFormat.Architectural)]
        [InlineData(0.0, "0'0\"", 3, DrawingUnits.English, UnitFormat.Architectural)] // nearest eighth inch
        [InlineData(15.2, "1'3-1/4\"", 3, DrawingUnits.English, UnitFormat.Architectural)]
        [InlineData(24.0, "2'0\"", 4, DrawingUnits.English, UnitFormat.Architectural)] // even feet
        [InlineData(0.125, "0'0-1/8\"", 4, DrawingUnits.English, UnitFormat.Architectural)] // only fractional inches
        [InlineData(36.625, "3'0-5/8\"", 4, DrawingUnits.English, UnitFormat.Architectural)] // feet and fractional inches, no whole inches
        [InlineData(15.99999999, "1'4\"", 4, DrawingUnits.English, UnitFormat.Architectural)] // near the upper limit
        [InlineData(-18.5, "-1'6-1/2\"", 4, DrawingUnits.English, UnitFormat.Architectural)] // negative value
        [InlineData(0.0, "0\"", 0, DrawingUnits.English, UnitFormat.Fractional)] // nearest inch
        [InlineData(15.2, "15\"", 0, DrawingUnits.English, UnitFormat.Fractional)]
        [InlineData(0.0, "0\"", 3, DrawingUnits.English, UnitFormat.Fractional)] // nearest eighth inch
        [InlineData(15.2, "15-1/4\"", 3, DrawingUnits.English, UnitFormat.Fractional)]
        [InlineData(24.0, "24\"", 4, DrawingUnits.English, UnitFormat.Fractional)] // even feet
        [InlineData(0.125, "0-1/8\"", 4, DrawingUnits.English, UnitFormat.Fractional)] // only fractional inches
        [InlineData(0.625, "0-5/8\"", 4, DrawingUnits.English, UnitFormat.Fractional)]
        [InlineData(36.625, "36-5/8\"", 4, DrawingUnits.English, UnitFormat.Fractional)] // feet and fractional inches, no whole inches
        [InlineData(15.99999999, "16\"", 4, DrawingUnits.English, UnitFormat.Fractional)] // near the upper limit
        [InlineData(-18.5, "-18-1/2\"", 4, DrawingUnits.English, UnitFormat.Fractional)] // negative value
        public void FormatTests(double value, string expected, int precision, DrawingUnits drawingUnits, UnitFormat unitFormat)
        {
            var actual = DrawingSettings.FormatUnits(value, drawingUnits, unitFormat, precision);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MetricParseTest()
        {
            TestParse("0", 0.0);
            TestParse("3.25", 3.25);
            TestParse("-4.8", -4.8);
        }

        [Fact]
        public void ArchitecturalParseTest()
        {
            TestParse("18", 18.0); // just inches, no specifier
            TestParse("18\"", 18.0); // just inches, with specifier

            TestParse("2'", 24.0); // just feet

            TestParse("2'3\"", 27.0); // feet and inches, with specifier
            TestParse("2'3", 27.0); // feet and inches, no specifier

            TestParse("1'3-5/8\"", 15.625); // feet and whole and fractional inches
            TestParse("1'5/8\"", 12.625); // feet and fractional inches

            TestParse("1'3.5\"", 15.5); // feet with decimal inches with specifier
            TestParse("1'3.5", 15.5); // feet with decimal inches without specifier

            TestParse("-1'6-1/2\"", -18.5); // negative feet with fractional inches
            TestParse("-1'6.5\"", -18.5); // negative feet with decimal inches
        }

        [Fact]
        public void ParseFailureTest()
        {
            ParseFail(""); // empty string
            ParseFail("foo"); // not a number
            ParseFail("1'3foo"); // garbage tail
        }

        private void TestParse(string text, double expected)
        {
            double actual;
            Assert.True(DrawingSettings.TryParseUnits(text, out actual));
            Assert.Equal(expected, actual);
        }

        private void ParseFail(string text)
        {
            double temp;
            Assert.False(DrawingSettings.TryParseUnits(text, out temp));
        }
    }
}
