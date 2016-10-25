using BCad.Entities;
using BCad.Extensions;
using BCad.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class RotateTests : AbstractDrawingTests
    {
        private void DoRotate(Entity entityToRotate, Vector origin, double angleInDegrees, Entity expectedResult)
        {
            var actual = EditUtilities.Rotate(entityToRotate, origin, angleInDegrees);
            Assert.IsTrue(expectedResult.EquivalentTo(actual));
        }

        [TestMethod]
        public void OriginRotateTest()
        {
            DoRotate(new Line(new Point(0, 0, 0), new Point(1, 0, 0), null),
                Point.Origin,
                90,
                new Line(new Point(0, 0, 0), new Point(0, 1, 0), null));
        }

        [TestMethod]
        public void NonOriginRotateTest()
        {
            DoRotate(new Line(new Point(2, 2, 0), new Point(3, 2, 0), null),
                new Point(1, 1, 0),
                90,
                new Line(new Point(0, 2, 0), new Point(0, 3, 0), null));
        }
    }
}
