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

            //a = new PrimitiveLine(new Point(0, 3, 0), new Point(1, 1, -1));
            //b = new PrimitiveLine(new Point(5, 8, 2), new Point(3, 7, 1));

            // intersection is (0.5, 1.5, 2)

            Assert.Null(a.IntersectionXY(b));
            Assert.Equal(a.IntersectionXY(b, false), new Point(0, 1, 0));
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
