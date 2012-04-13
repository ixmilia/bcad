using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BCad.Test
{
    public class PointTests
    {
        private const double Tolerance = 0.000001;

        private void WithinTolerance(double expected, double value)
        {
            Assert.True(Math.Abs(expected - value) < Tolerance);
        }

        [Fact]
        public void EqualityTests()
        {
            Assert.Equal(new Point(0, 0, 0), new Point(0, 0, 0));
            Assert.Equal(new Point(1, 1, 1), new Point(1, 1, 1));
            Assert.NotEqual(new Point(0, 0, 0), new Point(1, 0, 0));
        }

        [Fact]
        public void PointParsingTests()
        {
            Assert.Equal(new Point(0, 0, 0), Point.Parse("0,0,0"));
        }

        [Fact]
        public void ToAngleTests()
        {
            WithinTolerance(45.0, new Vector(1, 1, 0).ToAngle());
            WithinTolerance(90.0, new Vector(0, 1, 0).ToAngle());
            WithinTolerance(135.0, new Vector(-1, 1, 0).ToAngle());
            WithinTolerance(180.0, new Vector(-1, 0, 0).ToAngle());
            WithinTolerance(225.0, new Vector(-1, -1, 0).ToAngle());
            WithinTolerance(270.0, new Vector(0, -1, 0).ToAngle());
            WithinTolerance(315.0, new Vector(1, -1, 0).ToAngle());
        }
    }
}
