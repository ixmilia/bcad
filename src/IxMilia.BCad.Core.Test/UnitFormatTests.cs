using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class UnitFormatTests
    {
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
