using IxMilia.BCad.Entities;
using IxMilia.BCad.Utilities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class ScaleTests : TestBase
    {
        [Fact]
        public void ScalePointFromSelf()
        {
            var point = new Point(1.0, 1.0, 0.0);
            var scaled = point.ScaleFrom(point, 2.0);
            Assert.Equal(point, scaled);
        }

        [Fact]
        public void ScalePointFromOtherLocation1()
        {
            var point = new Point(1.0, 1.0, 0.0);
            var scaled = point.ScaleFrom(Point.Origin, 2.0);
            Assert.Equal(new Point(2.0, 2.0, 0.0), scaled);
        }

        [Fact]
        public void ScalePointFromOtherLocation2()
        {
            var point = new Point(1.0, 1.0, 0.0);
            var scaled = point.ScaleFrom(Point.Origin, 0.5);
            Assert.Equal(new Point(0.5, 0.5, 0.0), scaled);
        }

        [Fact]
        public void OffsetLineFromEndpoint()
        {
            var scaled = (Line)EditUtilities.Scale(
                new Line(new Point(1.0, 1.0, 0.0), new Point(2.0, 2.0, 0.0)),
                new Point(1.0, 1.0, 0.0),
                2.0
            );
            Assert.Equal(new Point(1.0, 1.0, 0.0), scaled.P1);
            Assert.Equal(new Point(3.0, 3.0, 0.0), scaled.P2);
        }
    }
}
