using System;
using BCad.Entities;
using BCad.Extensions;
using BCad.Utilities;
using Xunit;

namespace BCad.Test
{
    public class DrawingTests : AbstractDrawingTests
    {
        [Fact]
        public void CurrentLayerStillSetAfterDrawingTest()
        {
            Workspace.AddLayer("Other");
            Workspace.SetCurrentLayer("Other");
            Workspace.AddToCurrentLayer(Entities.Line());
            Assert.Equal(1, Workspace.GetLayer("Other").EntityCount);
            Assert.Equal(Workspace.GetLayer("Other"), Workspace.Drawing.CurrentLayer);
        }

        [Fact]
        public void CurrentLayerStillSetAfterDrawingToOtherLayerTest()
        {
            Workspace.AddLayer("Other");
            Workspace.SetCurrentLayer("Other");
            Workspace.Add(Workspace.GetLayer("0"), Entities.Line());
            Assert.Equal(1, Workspace.GetLayer("0").EntityCount);
            Assert.Equal(Workspace.GetLayer("Other"), Workspace.Drawing.CurrentLayer);
        }

        [Fact]
        public void DeleteCurrentLayerTest()
        {
            Workspace.AddLayer("Other");
            Workspace.SetCurrentLayer("Other");
            Workspace.Remove(Workspace.GetLayer("Other"));
            Assert.Equal(Workspace.GetLayer("0"), Workspace.Drawing.CurrentLayer);
        }

        [Fact]
        public void DeleteOnlyLayerTest()
        {
            var zero = Workspace.GetLayer("0");
            Workspace.Remove(zero);
            Assert.Equal(Workspace.GetLayer("0"), Workspace.Drawing.CurrentLayer);
            Assert.NotEqual(zero, Workspace.Drawing.CurrentLayer);
        }

        [Fact]
        public void CircleTtrTest()
        {
            var ellipse = EditUtilities.Ttr(
                Workspace.DrawingPlane,
                new SelectedEntity(new Line(Point.Origin, new Point(3, 0, 0), null), new Point(1, 0, 0)),
                new SelectedEntity(new Line(Point.Origin, new Point(0, 3, 0), null), new Point(0, 1, 0)),
                1.0);
            Assert.Equal(1.0, ellipse.MinorAxisRatio);
            Assert.Equal(1.0, ellipse.MajorAxis.Length);
            Assert.Equal(new Point(1, 1, 0), ellipse.Center);
            Assert.Equal(Workspace.DrawingPlane.Normal, ellipse.Normal);
        }

        [Fact]
        public void CircleTtrWithCirclesTest()
        {
            // from test.dxf
            var el = EditUtilities.Ttr(
                Workspace.DrawingPlane,
                new SelectedEntity(new Circle(new Point(100, 0, 0), 50, Vector.ZAxis, null), new Point(140, 30, 0)),
                new SelectedEntity(new Circle(new Point(100, 100, 0), 50, Vector.ZAxis, null), new Point(140, 70, 0)),
                30.0);
            Assert.Equal(1.0, el.MinorAxisRatio);
            Assert.Equal(30, el.MajorAxis.Length);
            AssertClose(new Point(162.449979983983, 50, 0), el.Center);
            Assert.Equal(Workspace.DrawingPlane.Normal, el.Normal);
        }

        [Fact]
        public void ArcMidpointTests()
        {
            Action<double, Arc> TestMidpoint = (midPointAngle, arc) =>
            {
                var transform = arc.GetUnitCircleProjection();
                var mp = (Vector)transform.Transform((Vector)arc.MidPoint);
                AssertClose(midPointAngle, mp.ToAngle());
            };
            TestMidpoint(45, new Arc(Point.Origin, 1, 315, 135, Vector.ZAxis, null));
            TestMidpoint(135, new Arc(Point.Origin, 1, 45, 225, Vector.ZAxis, null));
            TestMidpoint(315, new Arc(Point.Origin, 1, 225, 45, Vector.ZAxis, null));
            TestMidpoint(225, new Arc(Point.Origin, 1, 135, 315, Vector.ZAxis, null));
        }
    }
}
