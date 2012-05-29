using BCad.Extensions;
using BCad.Primitives;
using Xunit;

namespace BCad.Test
{
    public class PrimitiveTests
    {
        [Fact]
        public void LineIntersectionTest()
        {
            var a = new PrimitiveLine(new Point(-1, 0, 0), new Point(1, 0, 0));
            var b = new PrimitiveLine(new Point(0, -1, 0), new Point(0, 1, 0));
            Assert.Equal(a.IntersectionPoint(b), new Point(0, 0, 0));
        }

        [Fact]
        public void ThreePointCircleTest()
        {
            var a = new Point(0, 0, 0);
            var b = new Point(0, 2, 0);
            var c = new Point(1, 1, 0);
            var circ = PrimitiveEllipse.ThreePointCircle(a, b, c);
            Assert.NotNull(circ);
            Assert.Equal(new Point(0, 1, 0), circ.Center);
            Assert.Equal(Vector.XAxis, circ.MajorAxis);
            Assert.Equal(Vector.ZAxis, circ.Normal);
        }
    }
}
