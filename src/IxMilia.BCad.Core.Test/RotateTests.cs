using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Utilities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class RotateTests : TestBase
    {
        private void DoRotate(Entity entityToRotate, Vector origin, double angleInDegrees, Entity expectedResult)
        {
            var actual = EditUtilities.Rotate(entityToRotate, origin, angleInDegrees);
            Assert.True(expectedResult.EquivalentTo(actual));
        }

        [Fact]
        public void OriginRotateTest()
        {
            DoRotate(new Line(new Point(0, 0, 0), new Point(1, 0, 0)),
                Point.Origin,
                90,
                new Line(new Point(0, 0, 0), new Point(0, 1, 0)));
        }

        [Fact]
        public void NonOriginRotateTest()
        {
            DoRotate(new Line(new Point(2, 2, 0), new Point(3, 2, 0)),
                new Point(1, 1, 0),
                90,
                new Line(new Point(0, 2, 0), new Point(0, 3, 0)));
        }

        [Theory]
        [InlineData(0.0, 0.0, 0.0)]
        [InlineData(1.0, 1.0, 45.0)]
        [InlineData(0.0, 1.0, 90.0)]
        [InlineData(-1.0, 1.0, 135.0)]
        [InlineData(-1.0, 0.0, 180.0)]
        [InlineData(-1.0, -1.0, 225.0)]
        [InlineData(0.0, -1.0, 270.0)]
        [InlineData(1.0, -1.0, 315.0)]
        public void AngleBetweenVectors(double x, double y, double expectedAngle)
        {
            var v1 = new Vector(1.0, 0.0, 0.0);
            var v2 = new Vector(x, y, 0.0);
            var actualAngle = Vector.AngleBetweenInDegrees(v1, v2);
            AssertClose(expectedAngle, actualAngle);
        }
    }
}
