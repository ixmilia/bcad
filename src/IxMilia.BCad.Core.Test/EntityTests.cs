using System.Linq;
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
    }
}
