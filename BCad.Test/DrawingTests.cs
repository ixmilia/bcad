using System.Linq;
using BCad.Objects;
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
            Workspace.AddToCurrentLayer(Objects.Line());
            Assert.Equal(1, Workspace.GetLayer("Other").Objects.Count);
            Assert.Equal(Workspace.GetLayer("Other"), Workspace.CurrentLayer);
        }

        [Fact]
        public void DrawLineCommandTest()
        {
            Execute("Draw.Line");
            Push(new Point(1, 1, 1));
            Push(new Point(2, 3, 4));
            WaitForRequest();
            AwaitingPoint();
            Complete();
            AwaitingCommand();
            VerifyLayerContains("0", new Line(new Point(1, 1, 1), new Point(2, 3, 4), Color.Auto));
        }

        [Fact]
        public void DrawLineCommandCancelTest()
        {
            Execute("Draw.Line");
            Push(new Point(1, 1, 1));
            WaitForRequest();
            AwaitingPoint();
            Cancel();
            AwaitingCommand();
            Assert.False(Workspace.GetObjects().Any());
        }

        [Fact]
        public void DrawLineCommandCloseTest()
        {
            Execute("Draw.Line");
            Push(new Point(1, 1, 1));
            Push(new Point(2, 3, 4));
            Push(new Point(3, 3, 3));
            Push(new Point(6, 7, 8));
            Push("c");
            WaitForCompletion();
            AwaitingCommand();
            VerifyLayerContains("0", new Line(new Point(1, 1, 1), new Point(2, 3, 4), Color.Auto));
            VerifyLayerContains("0", new Line(new Point(2, 3, 4), new Point(3, 3, 3), Color.Auto));
            VerifyLayerContains("0", new Line(new Point(3, 3, 3), new Point(6, 7, 8), Color.Auto));
            VerifyLayerContains("0", new Line(new Point(6, 7, 8), new Point(1, 1, 1), Color.Auto));
        }

        [Fact]
        public void DrawCircleCommandTest()
        {
            Execute("Draw.Circle");
            Push(new Point(0, 0, 0));
            Push(new Point(1, 0, 0));
            WaitForCompletion();
            AwaitingCommand();
            VerifyLayerContains("0", new Circle(Point.Origin, 1.0, Vector.ZAxis, Color.Auto));
        }

        [Fact]
        public void DrawCircleCommandCancelTest()
        {
            Execute("Draw.Circle");
            Push(new Point(0, 0, 0));
            WaitForRequest();
            AwaitingPoint();
            Cancel();
            AwaitingCommand();
            Assert.False(Workspace.GetObjects().Any());
        }
    }
}
