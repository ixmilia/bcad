using BCad.Primitives;
using Xunit;

namespace BCad.Test
{
    public class PrimitiveTests
    {
        [Fact]
        public void LineIntersectionTest()
        {
            var a = new PrimitiveLine(new Point(0, 0, 0), new Point(0, 2, 0));
            var b = new PrimitiveLine(new Point(1, 1, 0), new Point(2, 1, 0));
            Assert.False(a.IntersectsInXY(b));
        }
    }
}
