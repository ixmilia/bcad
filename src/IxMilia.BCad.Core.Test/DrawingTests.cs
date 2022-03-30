using System;
using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.SnapPoints;
using IxMilia.BCad.Utilities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class DrawingTests : TestBase
    {
        private Line Line()
        {
            return new Line(Point.Origin, Point.Origin);
        }

        [Fact]
        public void CurrentLayerStillSetAfterDrawingTest()
        {
            Workspace.AddLayer("Other");
            Workspace.SetCurrentLayer("Other");
            Workspace.AddToCurrentLayer(Line());
            Assert.Equal(1, Workspace.GetLayer("Other").EntityCount);
            Assert.Equal(Workspace.GetLayer("Other"), Workspace.Drawing.CurrentLayer);
        }

        [Fact]
        public void CurrentLayerStillSetAfterDrawingToOtherLayerTest()
        {
            Workspace.AddLayer("Other");
            Workspace.SetCurrentLayer("Other");
            Workspace.Add(Workspace.GetLayer("0"), Line());
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
                new SelectedEntity(new Line(Point.Origin, new Point(3, 0, 0)), new Point(1, 0, 0)),
                new SelectedEntity(new Line(Point.Origin, new Point(0, 3, 0)), new Point(0, 1, 0)),
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
                new SelectedEntity(new Circle(new Point(100, 0, 0), 50, Vector.ZAxis), new Point(140, 30, 0)),
                new SelectedEntity(new Circle(new Point(100, 100, 0), 50, Vector.ZAxis), new Point(140, 70, 0)),
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
            TestMidpoint(45, new Arc(Point.Origin, 1, 315, 135, Vector.ZAxis));
            TestMidpoint(135, new Arc(Point.Origin, 1, 45, 225, Vector.ZAxis));
            TestMidpoint(315, new Arc(Point.Origin, 1, 225, 45, Vector.ZAxis));
            TestMidpoint(225, new Arc(Point.Origin, 1, 135, 315, Vector.ZAxis));
        }

        [Fact]
        public void IntersectionSnapPointsAreCalculatedTest()
        {
            var drawing = new Drawing()
                .AddToCurrentLayer(new Line(new Point(-1.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)))
                .AddToCurrentLayer(new Line(new Point(0.0, -1.0, 0.0), new Point(0.0, 1.0, 0.0)));
            var snapPoints = drawing.GetSnapPoints(Matrix4.Identity, 1.0, 1.0);
            var intersection = snapPoints.GetContainedItems(new Rect(-1.0, -1.0, 1.0, 1.0)).Single(t => t.Kind == SnapPointKind.Intersection);
            Assert.Equal(Point.Origin, intersection.WorldPoint);
        }

        [Fact]
        public void CreatePolylinesTest()
        {
            var entities = new[]
            {
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)),
                new Line(new Point(1.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0))
            };
            var drawing = new Drawing()
                .Add(new Layer("some-layer"))
                .Add(new Layer("some-other-layer"))
                .Update(currentLayerName: "some-layer");
            foreach (var entity in entities)
            {
                drawing = drawing.AddToCurrentLayer(entity);
            }

            var finalDrawing = drawing.CombineEntitiesIntoPolyline(entities, "some-other-layer");
            Assert.Empty(finalDrawing.Layers.GetValue("some-layer").GetEntities());

            var polyline = (Polyline)finalDrawing.Layers.GetValue("some-other-layer").GetEntities().Single();
            var vertices = polyline.Vertices.ToList();
            Assert.Equal(3, vertices.Count);
            Assert.Equal(new Point(0.0, 0.0, 0.0), vertices[0].Location);
            Assert.Equal(new Point(1.0, 0.0, 0.0), vertices[1].Location);
            Assert.Equal(new Point(1.0, 1.0, 0.0), vertices[2].Location);
        }

        [Fact]
        public void ScaleEntities()
        {
            var drawing = new Drawing();
            drawing = drawing.AddToCurrentLayer(new Line(new Point(1.0, 1.0, 0.0), new Point(2.0, 2.0, 0.0)));
            drawing = drawing.AddToCurrentLayer(new Line(new Point(2.0, 2.0, 0.0), new Point(3.0, 3.0, 0.0)));
            drawing = drawing.AddToCurrentLayer(new Circle(new Point(3.0, 3.0, 0.0), 4.0, Vector.ZAxis));
            var lines = drawing.GetEntities().OfType<Line>().ToList();

            var updatedDrawing = drawing.ScaleEntities(lines, new Point(1.0, 1.0, 0.0), 2.0);

            Assert.True(updatedDrawing.GetEntities().OfType<Circle>().Single().EquivalentTo(new Circle(new Point(3.0, 3.0, 0.0), 4.0, Vector.ZAxis)));
            var scaledLines = updatedDrawing.GetEntities().OfType<Line>().ToList();
            Assert.Equal(2, scaledLines.Count);
            Assert.True(scaledLines[0].EquivalentTo(new Line(new Point(1.0, 1.0, 0.0), new Point(3.0, 3.0, 0.0))));
            Assert.True(scaledLines[1].EquivalentTo(new Line(new Point(3.0, 3.0, 0.0), new Point(5.0, 5.0, 0.0))));
        }
    }
}
