using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Extensions;
using Xunit;

namespace BCad.Test
{
    public class PointTests : AbstractDrawingTests
    {
        private const double Tolerance = 0.000001;

        private void WithinTolerance(double expected, double value)
        {
            Assert.True(Math.Abs(expected - value) < Tolerance);
        }

        private void TestParse(string text, Point expected, Optional<Point> cursor = default(Optional<Point>), Optional<Point> last = default(Optional<Point>))
        {
            var realCursor = cursor.HasValue ? cursor.Value : Point.Origin;
            var realLast = last.HasValue ? last.Value : Point.Origin;
            Point point;
            bool result = InputService.TryParsePoint(text, realCursor, realLast, out point);
            Assert.True(expected.CloseTo(point));
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

        [Fact]
        public void PointParseTests()
        {
            // length on current vector
            TestParse("0", new Point(2.0, 2.0, 2.0), cursor: new Point(2.0, 2.0, 2.0), last: new Point(2.0, 2.0, 2.0)); // zero length
            TestParse("3", new Point(5.0, 2.0, 2.0), cursor: new Point(3.0, 2.0, 2.0), last: new Point(2.0, 2.0, 2.0)); // non-zero length

            // relative
            TestParse("@3,2", new Point(4.0, 3.0, 1.0), last: new Point(1.0, 1.0, 1.0));
            TestParse("@3,2,-5", new Point(4.0, 3.0, -4.0), last: new Point(1.0, 1.0, 1.0));

            // distance and angle
            TestParse("15<180", new Point(-14.0, 1.0, 1.0), last: new Point(1.0, 1.0, 1.0));

            // absolute point
            TestParse("1,2", new Point(1.0, 2.0, 0.0));
            TestParse("1,2,3", new Point(1.0, 2.0, 3.0));
        }
    }
}
