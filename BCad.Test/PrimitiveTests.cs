using System;
using System.Linq;
using BCad.Extensions;
using BCad.Primitives;
using Xunit;

namespace BCad.Test
{
    public class PrimitiveTests
    {
        private static void Test(IPrimitive first, IPrimitive second, bool withinBounds, params Point[] points)
        {
            var p = first.IntersectionPoints(second, withinBounds).OrderBy(x => x.X).ThenBy(y => y.Y).ThenBy(z => z.Z).ToArray();
            points = points.OrderBy(x => x.X).ThenBy(y => y.Y).ThenBy(z => z.Z).ToArray();
            Assert.Equal(points.Length, p.Length);
            for (int i = 0; i < p.Length; i++)
            {
                Assert.Equal(points[i], p[i]);
            }
        }

        private static PrimitiveEllipse Circle(Point center, double radius)
        {
            return new PrimitiveEllipse(center, radius, Vector.ZAxis, Color.Auto);
        }

        private static PrimitiveEllipse Ellipse(Point center, double radiusX, double radiusY)
        {
            return new PrimitiveEllipse(center, new Vector(radiusX, 0, 0), Vector.ZAxis, radiusY / radiusX, 0, 360, Color.Auto);
        }

        [Fact]
        public void LineIntersectionTest()
        {
            var a = new PrimitiveLine(new Point(-1, 0, 0), new Point(1, 0, 0));
            var b = new PrimitiveLine(new Point(0, -1, 0), new Point(0, 1, 0));
            Assert.Equal(a.IntersectionPoint(b), new Point(0, 0, 0));
        }

        [Fact]
        public void LinePointDistanceTest()
        {
            var l = new PrimitiveLine(new Point(0, 0, 0), new Point(2, 0, 0));
            var p = new Point(1, 1, 0);
            Assert.Equal(new Point(1, 0, 0), l.ClosestPoint(p));
        }

        [Fact]
        public void LinePointDistanceTest2()
        {
            var a = new PrimitiveLine(new Point(0, 0, 0), new Point(10, 0, 0));
            var b = new Point(5, 3, 0);
            var c = a.ClosestPoint(b);
            Assert.Equal(new Point(5, 0, 0), c);
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

        [Fact]
        public void LineCircleIntersectionTest1()
        {
            var el = new PrimitiveEllipse(Point.Origin, 1.0, Vector.ZAxis, Color.Auto);
            var l = new PrimitiveLine(new Point(1, 0, -1), new Point(1, 0, 1));
            var points = l.IntersectionPoints(el);
            Assert.Equal(1, points.Count());
            Assert.Equal(new Point(1, 0, 0), points.Single());
        }

        [Fact]
        public void LineCircleIntersectionTest2()
        {
            var el = new PrimitiveEllipse(Point.Origin, 1.0, Vector.ZAxis, Color.Auto);
            var l = new PrimitiveLine(new Point(-2, 0, 0), new Point(2, 0, 0));
            var points = l.IntersectionPoints(el).ToArray();
            Assert.Equal(2, points.Length);
            Assert.True(points.Contains(new Point(1, 0, 0)));
            Assert.True(points.Contains(new Point(-1, 0, 0)));
        }

        [Fact]
        public void LineCircleIntersectionTest3()
        {
            var el = new PrimitiveEllipse(new Point(1, 1, 0), 2.0, Vector.ZAxis, Color.Auto);
            var l = new PrimitiveLine(new Point(-3, 1, 0), new Point(3, 1, 0));
            var points = l.IntersectionPoints(el).ToArray();
            Assert.Equal(2, points.Length);
            Assert.True(points.Contains(new Point(-1, 1, 0)));
            Assert.True(points.Contains(new Point(3, 1, 0)));
        }

        [Fact]
        public void LineCircleIntersectionTest4()
        {
            var el = new PrimitiveEllipse(new Point(1, 1, 0), 2.0, Vector.ZAxis, Color.Auto);
            var l = new PrimitiveLine(new Point(2, 1, 0), new Point(4, 1, 0));
            var points = l.IntersectionPoints(el).ToArray();
            Assert.Equal(1, points.Length);
            Assert.True(points.Contains(new Point(3, 1, 0)));
        }

        [Fact]
        public void LineCircleIntersectionTestOffPlane()
        {
            var el = new PrimitiveEllipse(Point.Origin, 1, Vector.ZAxis, Color.Auto);
            var l = new PrimitiveLine(new Point(1, 0, 1), new Point(1, 0, -1));
            var points = l.IntersectionPoints(el).ToArray();
            Assert.Equal(1, points.Length);
            Assert.True(points.Contains(new Point(1, 0, 0)));
        }

        [Fact]
        public void LineCircleIntersectionTestOffPlaneOutsideAngle()
        {
            var el = new PrimitiveEllipse(Point.Origin, 1, 90, 270, Vector.ZAxis, Color.Auto);
            var l = new PrimitiveLine(new Point(1, 0, 1), new Point(1, 0, -1));
            var points = l.IntersectionPoints(el).ToArray();
            Assert.Equal(0, points.Length);
        }

        [Fact]
        public void CircleCircleIntersectionTestSamePlaneOnePoint()
        {
            Test(
                new PrimitiveEllipse(Point.Origin, 1, Vector.ZAxis, Color.Auto),
                new PrimitiveEllipse(new Point(2, 0, 0), 1, Vector.ZAxis, Color.Auto),
                true,
                new Point(1, 0, 0));
        }

        [Fact]
        public void CircleCircleIntersectionTestSamePlaneTwoPoints()
        {
            var x = Math.Sqrt(3.0) / 2.0;
            Test(
                new PrimitiveEllipse(Point.Origin, 1, Vector.ZAxis, Color.Auto),
                new PrimitiveEllipse(new Point(1, 0, 0), 1, Vector.ZAxis, Color.Auto),
                true,
                new Point(0.5, x, 0),
                new Point(0.5, -x, 0));
        }

        [Fact]
        public void CircleCircleIntersectionTestSamePlaneNoPoints()
        {
            var x = Math.Sqrt(3.0) / 2.0;
            Test(
                new PrimitiveEllipse(Point.Origin, 1, Vector.ZAxis, Color.Auto),
                new PrimitiveEllipse(new Point(3, 0, 0), 1, Vector.ZAxis, Color.Auto),
                true);
        }

        [Fact]
        public void CircleEllipseIntersectionTestSamePlaneOnePoint()
        {
            Test(
                Circle(Point.Origin, 1),
                Ellipse(new Point(3, 0, 0), 2, 1),
                true,
                new Point(1, 0, 0));
            Test(
                Circle(Point.Origin, 1),
                Ellipse(new Point(0, 2, 0), 2, 1),
                false,
                new Point(0, 1, 0));
        }
    }
}
