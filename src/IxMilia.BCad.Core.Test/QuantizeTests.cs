using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Utilities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class QuantizeTests : TestBase
    {
        private const double OneSixteenth = 0.0625;

        private static void AssertQuantized(double expected, double value, double quantum)
        {
            var actual = QuantizeSettingsExtensions.QuantizeValue(value, quantum);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0.0, 0.0, 0.5)] // no change
        [InlineData(5.0, 5.0, 0.5)] // no change
        [InlineData(0.0, 0.125, 1.0)] // rounds down
        [InlineData(1.0, 0.5, 1.0)] // rounds up
        [InlineData(1.0, 0.9, 1.0)] // rounds up
        [InlineData(1.0, 1.1, 1.0)] // rounds down
        [InlineData(9.0, 9.1, 1.0)] // rounds down
        [InlineData(4.0, 3.9999999999, OneSixteenth)]
        [InlineData(4.0, 4.0000000001, OneSixteenth)]
        [InlineData(4.5, 4.4999999999, OneSixteenth)]
        [InlineData(4.5, 4.5000000001, OneSixteenth)]
        public void PositiveValuesCanBeQuantized(double expected, double value, double quantum)
        {
            AssertQuantized(expected, value, quantum);
            AssertQuantized(-expected, -value, quantum);
        }

        [Theory]
        [InlineData(-5.0, -5.0, 0.5)] // no change
        [InlineData(0.0, -0.125, 1.0)] // rounds up
        [InlineData(-1.0, -0.5, 1.0)] // rounds down
        [InlineData(-1.0, -0.9, 1.0)] // rounds down
        [InlineData(-1.0, -1.1, 1.0)] // rounds up
        [InlineData(-9.0, -9.1, 1.0)] // rounds up
        [InlineData(-4.0, -3.9999999999, OneSixteenth)]
        [InlineData(-4.0, -4.0000000001, OneSixteenth)]
        [InlineData(-4.5, -4.4999999999, OneSixteenth)]
        [InlineData(-4.5, -4.5000000001, OneSixteenth)]
        public void NegativeValuesCanBeQuantized(double expected, double value, double quantum)
        {
            AssertQuantized(expected, value, quantum);
            AssertQuantized(-expected, -value, quantum);
        }

        [Fact]
        public void QuantizeEntity()
        {
            var settings = new QuantizeSettings(distanceQuantum: OneSixteenth, angleQuantum: 1.0);
            var line = new Line(new Point(1.4999999999, 1.5000000001, 0.0), new Point(2.50000000001, 2.4999999999, 0.0));
            var quantized = (Line)EditUtilities.Quantize(line, settings);
            Assert.Equal(new Point(1.5, 1.5, 0.0), quantized.P1);
            Assert.Equal(new Point(2.5, 2.5, 0.0), quantized.P2);
        }
    }
}
