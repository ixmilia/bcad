using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class EntityTests : AbstractDrawingTests
    {
        [TestMethod]
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
            var c1 = new Circle(Point.Origin, 1.0, Vector.ZAxis, null);
            var c2 = c1.Update(center: new Point(1, 1, 0));
            var union = new[] { c1, c2 }.Union();
            var polyline = (Polyline)union.Single();
            var arcs = polyline.GetPrimitives().Cast<PrimitiveEllipse>().OrderBy(e => e.Center.X);

            Assert.AreEqual(2, arcs.Count());
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
    }
}
