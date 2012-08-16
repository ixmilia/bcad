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
    }
}
