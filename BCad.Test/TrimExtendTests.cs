using System.Linq;
using BCad.Entities;
using Xunit;

namespace BCad.Test
{
    public class TrimExtendTests : AbstractDrawingTests
    {
        private void PrepareSimpleLineTrimBoundaryLines()
        {
            Workspace.AddToCurrentLayer(new Line(new Point(-1.0, -1.0, 0.0), new Point(-1.0, 1.0, 0.0), Color.Auto));
            Workspace.AddToCurrentLayer(new Line(new Point(1.0, -1.0, 0.0), new Point(1.0, 1.0, 0.0), Color.Auto));
        }

        [Fact]
        public void SimpleLineTrimLeftTest()
        {
            PrepareSimpleLineTrimBoundaryLines();
            var line = new Line(new Point(0, 0, 0), new Point(2, 0, 0), Color.Auto);
            Workspace.AddToCurrentLayer(line);
        }
    }
}
