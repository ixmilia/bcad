using BCad.Objects;
using Xunit;

namespace BCad.Test
{
    public class DrawingTests
    {
        [Fact]
        public void DrawingToCurrentLayerTest()
        {
            var workspace = TestHost.CreateWorkspace("Other");
            workspace.SetCurrentLayer("Other");
            workspace.AddToCurrentLayer(Objects.Line());
            Assert.Equal(1, workspace.GetLayer("Other").Objects.Count);
        }

        [Fact(Skip = "Current layer switches after drawing")]
        public void CurrentLayerStillSetAfterDrawingTest()
        {
            var workspace = TestHost.CreateWorkspace("Other");
            workspace.SetCurrentLayer("Other");
            workspace.AddToCurrentLayer(Objects.Line());
            Assert.Equal(1, workspace.GetLayer("Other").Objects.Count);
            Assert.Equal(workspace.GetLayer("Other"), workspace.CurrentLayer);
        }
    }
}
