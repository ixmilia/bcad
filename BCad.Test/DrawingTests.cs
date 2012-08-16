using System.Linq;
using BCad.Entities;
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
            Assert.Equal(1, Workspace.GetLayer("Other").Entities.Count);
            Assert.Equal(Workspace.GetLayer("Other"), Workspace.CurrentLayer);
        }

        [Fact]
        public void CircleTtrTest()
        {
            var ellipse = EditService.Ttr(
                Workspace.DrawingPlane,
                new SelectedEntity(new Line(Point.Origin, new Point(3, 0, 0), Color.Auto), new Point(1, 0, 0)),
                new SelectedEntity(new Line(Point.Origin, new Point(0, 3, 0), Color.Auto), new Point(0, 1, 0)),
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
            var el = EditService.Ttr(
                Workspace.DrawingPlane,
                new SelectedEntity(new Circle(new Point(100, 0, 0), 50, Vector.ZAxis, Color.Auto), new Point(140, 30, 0)),
                new SelectedEntity(new Circle(new Point(100, 100, 0), 50, Vector.ZAxis, Color.Auto), new Point(140, 70, 0)),
                30.0);
            Assert.Equal(1.0, el.MinorAxisRatio);
            Assert.Equal(30, el.MajorAxis.Length);
            AssertClose(new Point(162.449979983983, 50, 0), el.Center);
            Assert.Equal(Workspace.DrawingPlane.Normal, el.Normal);
        }
    }
}
