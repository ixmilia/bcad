using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class EntityTests : TestBase
    {
        [Fact]
        public void CircleIntersectionTest()
        {
            //          ______                  ______
            //         /      \                /      \
            //        (        )              (        )
            //     __(___ c2    )          __(          )
            //   /    (  \     )   ==>   /             )
            //  (      \__)___/         (          ___/
            // (     c1    )           (           )
            //  (         )             (         )
            //   \_______/               \_______/
            var c1 = new Circle(Point.Origin, 1.0, Vector.ZAxis);
            var c2 = c1.Update(center: new Point(1, 1, 0));
            var union = new[] { c1, c2 }.Union();
            var polyline = (Polyline)union.Single();
            var arcs = polyline.GetPrimitives().Cast<PrimitiveEllipse>().OrderBy(e => e.Center.X).ToList();

            Assert.Equal(2, arcs.Count());
            var leftArc = arcs.First();
            var rightArc = arcs.Last();

            AssertClose(Point.Origin, leftArc.Center);
            AssertClose(1.0, leftArc.MajorAxis.Length);
            AssertClose(90.0, leftArc.StartAngle, error: 1E-10);
            AssertClose(0.0, leftArc.EndAngle, error: 1E-10);

            AssertClose(new Point(1, 1, 0), rightArc.Center);
            AssertClose(1.0, rightArc.MajorAxis.Length);
            AssertClose(270.0, rightArc.StartAngle, error: 1E-10);
            AssertClose(180.0, rightArc.EndAngle, error: 1E-10);
        }

        [Fact]
        public void PolylineArcDirectionTest1()
        {
            // points A and B are specified by vertices; expect point P to be on the primitive arc
            //     __B
            //   /
            //  (
            // P     .
            //  (
            //   \___A
            var poly = new Polyline(new[]
            {
                new Vertex(new Point(0.0, 0.0, 0.0), 180.0, VertexDirection.Clockwise),
                new Vertex(new Point(0.0, 2.0, 0.0), 180.0, VertexDirection.Clockwise)
            });
            var arc = (PrimitiveEllipse)poly.GetPrimitives().Single();
            AssertClose(90.0, arc.StartAngle);
            AssertClose(270.0, arc.EndAngle);
            AssertClose(new Point(-1.0, 1.0, 0.0), arc.MidPoint());
        }

        [Fact]
        public void PolylineArcDirectionTest2()
        {
            // points A and B are specified by vertices; expect point P to be on the primitive arc
            // B__
            //     \
            //      )
            // .    P
            //      )
            // A___/
            var poly = new Polyline(new[]
            {
                new Vertex(new Point(0.0, 0.0, 0.0), 180.0, VertexDirection.CounterClockwise),
                new Vertex(new Point(0.0, 2.0, 0.0), 180.0, VertexDirection.CounterClockwise)
            });
            var arc = (PrimitiveEllipse)poly.GetPrimitives().Single();
            AssertClose(270.0, arc.StartAngle);
            AssertClose(90.0, arc.EndAngle);
            AssertClose(new Point(1.0, 1.0, 0.0), arc.MidPoint());
        }

        [Fact]
        public void BulgeAffectsLeadingVertexTest()
        {
            var vertices = new[]
            {
                new Vertex(new Point(0.0, 0.0, 0.0)),
                new Vertex(new Point(1.0, 0.0, 0.0), 90.0, VertexDirection.CounterClockwise),
                new Vertex(new Point(2.0, 1.0, 0.0), 90.0, VertexDirection.CounterClockwise),
            };
            var poly = new Polyline(vertices);
            var primitives = poly.GetPrimitives().ToList();
            Assert.Equal(2, primitives.Count);
            
            var line = (PrimitiveLine)primitives[0];
            Assert.Equal(new Point(0.0, 0.0, 0.0), line.P1);
            Assert.Equal(new Point(1.0, 0.0, 0.0), line.P2);

            var arc = (PrimitiveEllipse)primitives[1];
            AssertClose(270.0, arc.StartAngle);
            AssertClose(0.0, arc.EndAngle);
            AssertClose(new Vector(1.0, 0.0, 0.0), arc.MajorAxis);
            AssertClose(1.0, arc.MinorAxisRatio);
        }

        [Fact]
        public void SplineBuilderKnotInsertionTest()
        {
            var builder = new SplineBuilder(
                3,
                Enumerable.Repeat(Point.Origin, 8),
                new[] { 0.0, 0.0, 0.0, 0.0, 0.2, 0.4, 0.6, 0.8, 1.0, 1.0, 1.0, 1.0 });
            builder.InsertKnot(0.5);
            Assert.Equal(9, builder.ControlPoints.Count());
            Assert.Equal(13, builder.KnotValues.Count());
            Assert.Equal(new[] { 0.0, 0.0, 0.0, 0.0, 0.2, 0.4, 0.5, 0.6, 0.8, 1.0, 1.0, 1.0, 1.0 }, builder.KnotValues);
        }

        [Fact]
        public void SplineToBezierTest()
        {
            var spline = new Spline(
                3,
                new[]
                {
                    new Point(59.1, 66.8, 0.0),
                    new Point(63.1, 81.7, 0.0),
                    new Point(127.2, 93.7, 0.0),
                    new Point(100.1, 12.9, 0.0),
                    new Point(55.4, 52.8, 0.0),
                    new Point(59.1, 66.8, 0.0)
                },
                new[] { 0.0, 0.0, 0.0, 0.0, 0.36, 0.65, 1.0, 1.0, 1.0, 1.0 });
            var beziers = spline.GetPrimitives().Cast<PrimitiveBezier>().ToList();

            Assert.Equal(3, beziers.Count);

            AssertClose(new Point(59.1, 66.8, 0.0), beziers[0].P1);
            AssertClose(new Point(63.1, 81.7, 0.0), beziers[0].P2);
            AssertClose(new Point(98.6015384615385, 88.3461538461538, 0.0), beziers[0].P3);
            AssertClose(new Point(109.037363313609, 75.2010840236686, 0.0), beziers[0].P4);

            AssertClose(new Point(109.037363313609, 75.2010840236686, 0.0), beziers[1].P1);
            AssertClose(new Point(117.444, 64.612, 0.0), beziers[1].P2);
            AssertClose(new Point(109.585, 41.18, 0.0), beziers[1].P3);
            AssertClose(new Point(96.1092041015625, 36.5579833984375, 0.0), beziers[1].P4);

            AssertClose(new Point(96.1092041015625, 36.5579833984375, 0), beziers[2].P1);
            AssertClose(new Point(79.8453125, 30.9796875, 0), beziers[2].P2);
            AssertClose(new Point(55.4, 52.8, 0), beziers[2].P3);
            AssertClose(new Point(59.1, 66.8, 0), beziers[2].P4);
        }

        [Fact]
        public void SplineFromBeziersTest()
        {
            var quadrant1 = new PrimitiveBezier(
                new Point(1.0, 0.0, 0.0),
                new Point(1.0, PrimitiveEllipse.BezierConstant, 0.0),
                new Point(PrimitiveEllipse.BezierConstant, 1.0, 0.0),
                new Point(0.0, 1.0, 0.0));
            var quadrant2 = new PrimitiveBezier(
                new Point(0.0, 1.0, 0.0),
                new Point(-PrimitiveEllipse.BezierConstant, 1.0, 0.0),
                new Point(-1.0, PrimitiveEllipse.BezierConstant, 0.0),
                new Point(-1.0, 0.0, 0.0));
            var spline = Spline.FromBeziers(new[] { quadrant1, quadrant2 });
            Assert.Equal(8, spline.ControlPoints.Count());
            Assert.Equal(new[] { 0.0, 0.0, 0.0, 0.0, 0.5, 0.5, 0.5, 0.5, 1.0, 1.0, 1.0, 1.0 }, spline.KnotValues);
        }

        [Fact]
        public void ExplodeEntities()
        {
            var poly = new Polyline(new[]
            {
                new Vertex(new Point(0.0, 0.0, 0.0)),
                new Vertex(new Point(1.0, 0.0, 0.0), 90.0, VertexDirection.CounterClockwise),
                new Vertex(new Point(2.0, 1.0, 0.0)),
            });
            var line = new Line(new Point(3.0, 3.0, 0.0), new Point(4.0, 4.0, 0.0));
            var agg = new AggregateEntity(new Point(10.0, 10.0, 0.0), ReadOnlyList<Entity>.Create(new Entity[] { poly, line }));
            Assert.True(agg.TryExplodeEntity(out var exploded));
            var explodedList = exploded.ToList();
            Assert.Equal(3, explodedList.Count);

            var line1 = (Line)explodedList[0];
            Assert.Equal(new Point(10.0, 10.0, 0.0), line1.P1);
            Assert.Equal(new Point(11.0, 10.0, 0.0), line1.P2);

            var arc = (Arc)explodedList[1];
            AssertClose(new Point(11.0, 11.0, 0.0), arc.Center);
            AssertClose(1.0, arc.Radius);
            AssertClose(270.0, arc.StartAngle);
            AssertClose(0.0, arc.EndAngle);

            var line2 = (Line)explodedList[2];
            Assert.Equal(new Point(13.0, 13.0, 0.0), line2.P1);
            Assert.Equal(new Point(14.0, 14.0, 0.0), line2.P2);
        }
    }
}
