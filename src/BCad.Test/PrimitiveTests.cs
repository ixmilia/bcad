using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class PrimitiveTests
    {

        #region Helpers

        private static void TestIntersection(IPrimitive first, IPrimitive second, bool withinBounds, params Point[] points)
        {
            var p = first.IntersectionPoints(second, withinBounds).OrderBy(x => x.X).ThenBy(y => y.Y).ThenBy(z => z.Z).ToArray();
            points = points.OrderBy(x => x.X).ThenBy(y => y.Y).ThenBy(z => z.Z).ToArray();
            Assert.AreEqual(points.Length, p.Length);
            for (int i = 0; i < p.Length; i++)
            {
                AssertClose(points[i], p[i]);
            }
        }

        private static void AssertClose(double expected, double actual)
        {
            Assert.IsTrue(Math.Abs(expected - actual) < MathHelper.Epsilon, string.Format("Expected: {0}\nActual: {1}", expected, actual));
        }

        private static void AssertClose(Point expected, Point actual)
        {
            AssertClose(expected.X, actual.X);
            AssertClose(expected.Y, actual.Y);
            AssertClose(expected.Z, actual.Z);
        }

        private static void AssertClose(Vector expected, Vector actual)
        {
            AssertClose(expected.X, actual.X);
            AssertClose(expected.Y, actual.Y);
            AssertClose(expected.Z, actual.Z);
        }

        private static void TestThreePointArcNormal(Point a, Point b, Point c, Vector idealNormal, Point expectedCenter, double expectedRadius, double expectedStartAngle, double expectedEndAngle)
        {
            var arc = PrimitiveEllipse.ThreePointArc(a, b, c, idealNormal);
            AssertClose(idealNormal, arc.Normal);
            AssertClose(expectedCenter, arc.Center);
            AssertClose(1.0, arc.MinorAxisRatio);
            AssertClose(expectedRadius, arc.MajorAxis.Length);
            AssertClose(expectedStartAngle, arc.StartAngle);
            AssertClose(expectedEndAngle, arc.EndAngle);
        }

        private static PrimitiveLine Line(Point p1, Point p2)
        {
            return new PrimitiveLine(p1, p2, null);
        }

        private static PrimitiveEllipse Circle(Point center, double radius)
        {
            return new PrimitiveEllipse(center, radius, Vector.ZAxis, null);
        }

        private static PrimitiveEllipse Arc(Point center, double radius, double startAngle, double endAngle)
        {
            return new PrimitiveEllipse(center, radius, startAngle, endAngle, Vector.ZAxis, null);
        }

        private static PrimitiveEllipse Ellipse(Point center, double radiusX, double radiusY)
        {
            return new PrimitiveEllipse(center, new Vector(radiusX, 0, 0), Vector.ZAxis, radiusY / radiusX, 0, 360, null);
        }

        private static void TestPointContainment(IPrimitive primitive, IEnumerable<Point> contained = null, IEnumerable<Point> excluded = null)
        {
            if (contained != null)
                Assert.IsTrue(contained.All(p => primitive.IsPointOnPrimitive(p)));
            if (excluded != null)
                Assert.IsTrue(excluded.All(p => !primitive.IsPointOnPrimitive(p)));
        }

        #endregion

        [TestMethod]
        public void LineIntersectionTest()
        {
            TestIntersection(
                Line(new Point(-1, 0, 0), new Point(1, 0, 0)),
                Line(new Point(0, -1, 0), new Point(0, 1, 0)),
                true,
                new Point(0, 0, 0));
        }

        [TestMethod]
        public void LinePointDistanceTest()
        {
            var l = Line(new Point(0, 0, 0), new Point(2, 0, 0));
            var p = new Point(1, 1, 0);
            Assert.AreEqual(new Point(1, 0, 0), l.ClosestPoint(p));
        }

        [TestMethod]
        public void LinePointDistanceTest2()
        {
            var a = Line(new Point(0, 0, 0), new Point(10, 0, 0));
            var b = new Point(5, 3, 0);
            var c = a.ClosestPoint(b);
            Assert.AreEqual(new Point(5, 0, 0), c);
        }

        [TestMethod]
        public void ThreePointCircleTest()
        {
            var a = new Point(0, 0, 0);
            var b = new Point(0, 2, 0);
            var c = new Point(1, 1, 0);
            var circ = PrimitiveEllipse.ThreePointCircle(a, b, c);
            Assert.IsNotNull(circ);
            Assert.AreEqual(new Point(0, 1, 0), circ.Center);
            Assert.AreEqual(Vector.XAxis, circ.MajorAxis);
            Assert.AreEqual(Vector.ZAxis, circ.Normal);
        }

        [TestMethod]
        public void ThreePointArcNormalizedNormalTest()
        {
            var rad = Math.Sqrt(2.0) / 2.0;
            // up then left
            //
            // 3         2
            //
            //      c
            //
            //           1
            TestThreePointArcNormal(
                new Point(1, 0, 0),
                new Point(1, 1, 0),
                new Point(0, 1, 0),
                Vector.ZAxis,
                new Point(0.5, 0.5, 0),
                rad,
                315.0,
                135.0);
            // up then right
            //
            // 2         3
            //
            //      c
            //
            // 1
            TestThreePointArcNormal(
                new Point(0, 0, 0),
                new Point(0, 1, 0),
                new Point(1, 1, 0),
                Vector.ZAxis,
                new Point(0.5, 0.5, 0),
                rad,
                45.0,
                225.0);
            // down then left
            //
            //           1
            //
            //      c
            //
            // 3         2
            TestThreePointArcNormal(
                new Point(1, 1, 0),
                new Point(1, 0, 0),
                new Point(0, 0, 0),
                Vector.ZAxis,
                new Point(0.5, 0.5, 0),
                rad,
                225.0,
                45.0);
            // down then right
            //
            // 1
            //
            //      c
            //
            // 2         3
            TestThreePointArcNormal(
                new Point(0, 1, 0),
                new Point(0, 0, 0),
                new Point(1, 0, 0),
                Vector.ZAxis,
                new Point(0.5, 0.5, 0),
                rad,
                135.0,
                315.0);
        }

        [TestMethod]
        public void ThreePointArcWithLargeAngleTest()
        {
            var sqrt22 = Math.Sqrt(2.0) / 2.0;

            // counter clockwise
            //
            //      1
            //
            //      c    3
            //
            // 2
            TestThreePointArcNormal(
                new Point(0, 1, 0),
                new Point(-sqrt22, -sqrt22, 0),
                new Point(1, 0, 0),
                idealNormal: Vector.ZAxis,
                expectedCenter: Point.Origin,
                expectedRadius: 1.0,
                expectedStartAngle: 90.0,
                expectedEndAngle: 0.0);
            // clockwise
            //
            //      3
            //
            //      c    1
            //
            // 2
            TestThreePointArcNormal(
                new Point(1, 0, 0),
                new Point(-sqrt22, -sqrt22, 0),
                new Point(0, 1, 0),
                idealNormal: Vector.ZAxis,
                expectedCenter: Point.Origin,
                expectedRadius: 1.0,
                expectedStartAngle: 90.0,
                expectedEndAngle: 0.0);
        }

        [TestMethod]
        public void LineCircleIntersectionTest1()
        {
            TestIntersection(
                Circle(Point.Origin, 2),
                Line(new Point(2, 0, -2), new Point(2, 0, 2)),
                true,
                new Point(2, 0, 0));
        }

        [TestMethod]
        public void LineCircleIntersectionTest2()
        {
            TestIntersection(
                Circle(new Point(1, 0, 0), 1.0),
                Line(new Point(-4, 0, 0), new Point(4, 0, 0)),
                true,
                new Point(2, 0, 0),
                new Point(0, 0, 0));
        }

        [TestMethod]
        public void LineCircleIntersectionTest3()
        {
            TestIntersection(
                Circle(new Point(1, 1, 0), 2),
                Line(new Point(-3, 1, 0), new Point(3, 1, 0)),
                true,
                new Point(-1, 1, 0),
                new Point(3, 1, 0));
        }

        [TestMethod]
        public void LineCircleIntersectionTest4()
        {
            TestIntersection(
                Circle(new Point(1, 1, 0), 2),
                Line(new Point(2, 1, 0), new Point(4, 1, 0)),
                true,
                new Point(3, 1, 0));
        }

        [TestMethod]
        public void LineCircleIntersectionTestOffPlane()
        {
            TestIntersection(
                Circle(Point.Origin, 1),
                Line(new Point(1, 0, 1), new Point(1, 0, -1)),
                true,
                new Point(1, 0, 0));
        }

        [TestMethod]
        public void LineCircleIntersectionTestOffPlaneOutsideAngle()
        {
            TestIntersection(
                Arc(Point.Origin, 1, 90, 270),
                Line(new Point(1, 0, 1), new Point(1, 0, -1)),
                true);
        }

        [TestMethod]
        public void CircleCircleIntersectionTestSamePlaneOnePoint()
        {
            TestIntersection(
                Circle(new Point(1, 1, 0), 2),
                Circle(new Point(4, 1, 0), 1),
                true,
                new Point(3, 1, 0));
            TestIntersection(
                Circle(new Point(100, 100, 0), 10),
                Circle(new Point(120, 100, 0), 10),
                true,
                new Point(110, 100, 0));
        }

        [TestMethod]
        public void CircleCircleIntersectionTestSamePlaneTwoPoints()
        {
            var x = Math.Sqrt(3.0) / 2.0;
            TestIntersection(
                Circle(Point.Origin, 1),
                Circle(new Point(1, 0, 0), 1),
                true,
                new Point(0.5, x, 0),
                new Point(0.5, -x, 0));
            TestIntersection(
                Circle(new Point(100, 0, 0), 80),
                Circle(new Point(100, 100, 0), 80),
                true,
                new Point(37.550020016016, 50, 0),
                new Point(162.449979983983, 50, 0));
        }

        [TestMethod]
        public void CircleCircleIntersectionTestSamePlaneNoPoints()
        {
            var x = Math.Sqrt(3.0) / 2.0;
            TestIntersection(
                Circle(Point.Origin, 1),
                Circle(new Point(3, 0, 0), 1),
                true);
        }

        [TestMethod]
        public void CircleEllipseIntersectionTestSamePlaneOnePoint()
        {
            // x-axis alignment horizontal
            TestIntersection(
                Circle(new Point(1, 0, 0), 1),
                Ellipse(new Point(4, 0, 0), 2, 1),
                true,
                new Point(2, 0, 0));
            // x-axis alignment vertical
            TestIntersection(
                Circle(new Point(1, 0, 0), 1),
                Ellipse(new Point(3, 0, 0), 1, 2),
                true,
                new Point(2, 0, 0));
            // y-axis alignment horizontal
            TestIntersection(
                Circle(Point.Origin, 1),
                Ellipse(new Point(0, 2, 0), 2, 1),
                true,
                new Point(0, 1, 0));
            // y-axis alignment vertical
            TestIntersection(
                Circle(Point.Origin, 1),
                Ellipse(new Point(0, 3, 0), 1, 2),
                true,
                new Point(0, 1, 0));
            // rotates to x-axis alignment
            TestIntersection(
                Circle(Point.Origin, 1),
                new PrimitiveEllipse(new Point(-Math.Sqrt(2), Math.Sqrt(2), 0), new Vector(Math.Sqrt(2), Math.Sqrt(2), 0), Vector.ZAxis, 0.5, 0, 360, null),
                true,
                new Point(-0.707106781187, 0.707106781187, 0));
        }

        [TestMethod]
        public void CircleEllipseIntersectionTestSamePlaneTwoPoints()
        {
            // y-axis alignment
            TestIntersection(
                Circle(new Point(1, 0, 0), 1),
                Ellipse(new Point(3, 0, 0), 2, 1),
                true,
                new Point(1.666666666667, -0.7453559925, 0),
                new Point(1.666666666667, 0.7453559925, 0));
            // no axis alignment
            TestIntersection(
                Circle(Point.Origin, 1),
                Ellipse(new Point(2, 1, 0), 2, 1),
                true,
                new Point(0, 1, 0),
                new Point(1, 0.133974596216, 0));
        }

        [TestMethod]
        public void CircleEllipseIntersectionTestDifferentPlanes()
        {
            // 1 intersection point, x-axis plane intersection
            TestIntersection(
                Circle(Point.Origin, 1),
                new PrimitiveEllipse(new Point(0, 1, 1), 1, Vector.YAxis, null),
                true,
                new Point(0, 1, 0));
            // 1 intersection point, y-axis plane intersection
            TestIntersection(
                Circle(Point.Origin, 1),
                new PrimitiveEllipse(new Point(1, 0, 1), 1, Vector.XAxis, null),
                true,
                new Point(1, 0, 0));
            // 1 intersection point, z-axis plane intersection
            TestIntersection(
                new PrimitiveEllipse(Point.Origin, 1, Vector.XAxis, null),
                new PrimitiveEllipse(new Point(1, 1, 0), 1, Vector.YAxis, null),
                true,
                new Point(0, 1, 0));
            // 2 intersection points
            TestIntersection(
                Circle(new Point(1, 0, 0), 1),
                new PrimitiveEllipse(new Point(1, 0, 0), new Vector(0, 0, 2), Vector.XAxis, 0.5, 0, 360, null),
                true,
                new Point(1, -1, 0),
                new Point(1, 1, 0));
        }

        [TestMethod]
        public void PointOnLineTest()
        {
            TestPointContainment(Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)),
                contained: new[]
                {
                    new Point(0.0, 0.0, 0.0),
                    new Point(0.5, 0.0, 0.0),
                    new Point(1.0, 0.0, 0.0)
                });
            TestPointContainment(Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 1.0)),
                contained: new[]
                {
                    new Point(0.0, 0.0, 0.0),
                    new Point(0.5, 0.5, 0.5),
                    new Point(1.0, 1.0, 1.0)
                });
        }

        [TestMethod]
        public void PointOnCircleTest()
        {
            var x = Math.Sin(45.0 * MathHelper.DegreesToRadians);
            TestPointContainment(Circle(new Point(0.0, 0.0, 0.0), 1.0),
                contained: new[]
                {
                    new Point(1.0, 0.0, 0.0),
                    new Point(0.0, 1.0, 0.0),
                    new Point(-1.0, 0.0, 0.0),
                    new Point(0.0, -1.0, 0.0),
                    new Point(x, x, 0.0)
                },
                excluded: new[]
                {
                    new Point(0.5, 0.0, 0.0),
                    new Point(1.5, 0.0, 0.0),
                });
            TestPointContainment(Circle(new Point(1.0, 1.0, 0.0), 1.0),
                contained: new[]
                {
                    new Point(2.0, 1.0, 0.0),
                    new Point(1.0, 2.0, 0.0),
                    new Point(0.0, 1.0, 0.0),
                    new Point(1.0, 0.0, 0.0),
                    new Point(x + 1.0, x + 1.0, 0.0)
                });
            TestPointContainment(Arc(new Point(0.0, 0.0, 0.0), 1.0, 90.0, 180.0),
                contained: new[]
                {
                    new Point(0.0, 1.0, 0.0), // 90 degrees
                    new Point(-1.0, 0.0, 0.0) // 180 degrees
                },
                excluded: new[]
                {
                    new Point(0.0, -1.0, 0.0), // 270 degrees
                    new Point(0.0, 0.0, 0.0) // 0/360 degrees
                });
        }

        [TestMethod]
        public void PointInTextTest()
        {
            // text width = 9.23076923076923
            TestPointContainment(new PrimitiveText(" ", new Point(0.0, 0.0, 0.0), 12.0, Vector.ZAxis, 0.0, null),
                contained: new[]
                {
                    new Point(0.0, 0.0, 0.0),
                    new Point(9.0, 12.0, 0.0)
                },
                excluded: new[]
                {
                    new Point(0.0, 12.1, 0.0)
                });
            TestPointContainment(new PrimitiveText(" ", new Point(5.0, 5.0, 5.0), 12.0, Vector.ZAxis, 0.0, null),
                contained: new[]
                {
                    new Point(5.0, 5.0, 5.0),
                    new Point(14.0, 17.0, 5.0)
                },
                excluded: new[]
                {
                    new Point(5.0, 17.1, 5.0)
                });
        }

        [TestMethod]
        public void EllipseAngleContainmentTest()
        {
            var el = new PrimitiveEllipse(Point.Origin, 1.0, 90.0, 360.0, Vector.ZAxis, null);
            Assert.IsTrue(el.IsAngleContained(90.0));
            Assert.IsTrue(el.IsAngleContained(180.0));
            Assert.IsTrue(el.IsAngleContained(270.0));
            Assert.IsTrue(el.IsAngleContained(360.0));
            Assert.IsFalse(el.IsAngleContained(45.0));
        }

        [TestMethod]
        public void EllipseGetPointTest()
        {
            var el = new PrimitiveEllipse(Point.Origin, 1.0, 0.0, 180.0, Vector.ZAxis, null);
            Assert.IsTrue(el.StartPoint().CloseTo(new Point(1.0, 0.0, 0.0)));
            Assert.IsTrue(el.EndPoint().CloseTo(new Point(-1.0, 0.0, 0.0)));
            Assert.IsTrue(el.GetPoint(90.0).CloseTo(new Point(0.0, 1.0, 0.0)));

            el = new PrimitiveEllipse(new Point(1.0, 1.0, 0.0), 1.0, 0.0, 180.0, Vector.ZAxis, null);
            Assert.IsTrue(el.StartPoint().CloseTo(new Point(2.0, 1.0, 0.0)));
            Assert.IsTrue(el.EndPoint().CloseTo(new Point(0.0, 1.0, 0.0)));
            Assert.IsTrue(el.GetPoint(90.0).CloseTo(new Point(1.0, 2.0, 0.0)));
        }

        [TestMethod]
        public void LinesToLineStripTest()
        {
            var lines = new List<PrimitiveLine>()
            {
                new PrimitiveLine(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)), // bottom of square
                new PrimitiveLine(new Point(1.0, 1.0, 0.0), new Point(1.0, 0.0, 0.0)), // right edge of square, points reversed
                new PrimitiveLine(new Point(1.0, 1.0, 0.0), new Point(0.0, 1.0, 0.0)), // top edge of square
                new PrimitiveLine(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0)), // left edge of square, points reversed
            };
            var points = lines.GetLineStripsFromPrimitives();
        }

        [TestMethod]
        public void ArcsFromPointsAndRadiusTest1()
        {
            // given the points (0, 1) and (0, -1) and an included angle of 90 degrees, the possible centers for the arcs
            // here are (1, 0) and (-1, 0) and a radius of sqrt(2)
            var p1 = new Point(0, 1, 0);
            var p2 = new Point(0, -1, 0);
            var includedAngle = 90.0;

            var sqrt2 = Math.Sqrt(2.0);

            var arc1 = PrimitiveEllipse.ArcFromPointsAndIncludedAngle(p1, p2, includedAngle, VertexDirection.Clockwise);
            AssertClose(new Point(-1, 0, 0), arc1.Center);
            AssertClose(sqrt2, arc1.MajorAxis.Length);
            AssertClose(315.0, arc1.StartAngle);
            AssertClose(45.0, arc1.EndAngle);

            var arc2 = PrimitiveEllipse.ArcFromPointsAndIncludedAngle(p1, p2, includedAngle, VertexDirection.CounterClockwise);
            AssertClose(new Point(1, 0, 0), arc2.Center);
            AssertClose(sqrt2, arc2.MajorAxis.Length);
            AssertClose(135.0, arc2.StartAngle);
            AssertClose(225.0, arc2.EndAngle);
        }
    }
}
