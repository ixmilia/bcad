using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Entities;
using BCad.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class OffsetTests : AbstractDrawingTests
    {
        [TestMethod]
        public void CircleOffsetInsideTest()
        {
            var offset = EditUtilities.Offset(
                Workspace,
                new Circle(Point.Origin, 2.0, Vector.ZAxis, IndexedColor.Auto),
                Point.Origin,
                1.0);
            Assert.IsTrue(offset is Circle);
            var circle = (Circle)offset;
            Assert.AreEqual(Point.Origin, circle.Center);
            Assert.AreEqual(1.0, circle.Radius);
        }

        [TestMethod]
        public void CircleOffsetOutsideTest()
        {
            var circle = (Circle)EditUtilities.Offset(
                Workspace,
                new Circle(Point.Origin, 2.0, Vector.ZAxis, IndexedColor.Auto),
                new Point(3, 0, 0),
                1.0);
            Assert.AreEqual(Point.Origin, circle.Center);
            Assert.AreEqual(3.0, circle.Radius);
        }

        [TestMethod]
        public void VerticalLineOffsetLeftTest()
        {
            var offset = (Line)EditUtilities.Offset(
                Workspace,
                new Line(new Point(1, 0, 0), new Point(1, 1, 0), IndexedColor.Auto),
                Point.Origin,
                1.0);
            Assert.AreEqual(new Point(0, 0, 0), offset.P1);
            Assert.AreEqual(new Point(0, 1, 0), offset.P2);
        }

        [TestMethod]
        public void VerticalLineOffsetRightTest()
        {
            var offset = (Line)EditUtilities.Offset(
                Workspace,
                new Line(new Point(1, 0, 0), new Point(1, 1, 0), IndexedColor.Auto),
                new Point(2, 0, 0),
                1.0);
            Assert.AreEqual(new Point(2, 0, 0), offset.P1);
            Assert.AreEqual(new Point(2, 1, 0), offset.P2);
        }

        [TestMethod]
        public void HorizontalLineOffsetUpTest()
        {
            var offset = (Line)EditUtilities.Offset(
                Workspace,
                new Line(new Point(0, 1, 0), new Point(1, 1, 0), IndexedColor.Auto),
                new Point(0, 2, 0),
                1.0);
            Assert.AreEqual(new Point(0, 2, 0), offset.P1);
            Assert.AreEqual(new Point(1, 2, 0), offset.P2);
        }

        [TestMethod]
        public void HorizontalLineOffsetDownTest()
        {
            var offset = (Line)EditUtilities.Offset(
                Workspace,
                new Line(new Point(0, 1, 0), new Point(1, 1, 0), IndexedColor.Auto),
                Point.Origin,
                1.0);
            Assert.AreEqual(new Point(0, 0, 0), offset.P1);
            Assert.AreEqual(new Point(1, 0, 0), offset.P2);
        }

        [TestMethod]
        public void DiagonalLineOffsetTest()
        {
            var offset = (Line)EditUtilities.Offset(
                Workspace,
                new Line(new Point(0, 1, 0), new Point(1, 2, 0), IndexedColor.Auto),
                Point.Origin,
                1.0);
            AssertClose(new Point(0.707106781186547, 0.292893218813453, 0), offset.P1);
            AssertClose(new Point(1.707106781186547, 1.292893218813453, 0), offset.P2);
        }

        [TestMethod]
        public void OffsetPointDirectlyOnEntity()
        {
            // line
            var offset = EditUtilities.Offset(
                Workspace,
                new Line(new Point(-1, 0, 0), new Point(1, 0, 0), IndexedColor.Auto),
                Point.Origin,
                1.0);
            Assert.IsNull(offset);
            
            // circle
            offset = EditUtilities.Offset(
                Workspace,
                new Circle(Point.Origin, 1.0, Vector.ZAxis, IndexedColor.Auto),
                new Point(1.0, 0, 0),
                1.0);
            Assert.IsNull(offset);
        }

        [TestMethod]
        public void CircleOffsetProjectionBug()
        {
            // enuse we're using the correct projection matrix when verifying
            // whether the offset point is inside the circle or not
            var offset = (Circle)EditUtilities.Offset(
                Workspace,
                new Circle(new Point(100, 0, 0), 50, Vector.ZAxis, IndexedColor.Auto),
                new Point(100, 0, 0),
                10);
            Assert.AreEqual(new Point(100, 0, 0), offset.Center);
            Assert.AreEqual(40, offset.Radius);
        }
    }
}
