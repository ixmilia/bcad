using BCad.Objects;
using Xunit;

namespace BCad.Test
{
    public class DrawingTests
    {
        public DrawingTests()
        {
            this.Host = TestHost.CreateHost();
        }

        private void ExecuteAndPush(string commandName, params object[] args)
        {
            Workspace.ExecuteCommand(commandName);
            foreach (var arg in args)
                InputService.PushValue(arg);
            Assert.Equal(InputType.Command, InputService.DesiredInputType);
        }

        private void VerifyContains(string layerName, IObject obj)
        {
            var x = Workspace.Document.Layers[layerName];
        }

        private TestHost Host { get; set; }

        private IWorkspace Workspace { get { return Host.Workspace; } }

        private IInputService InputService { get { return Host.InputService; } }

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
        public void DrawLineCommand()
        {
            ExecuteAndPush("Draw.Line",
                new Point(1, 1, 1),
                new Point(2, 3, 4));
            VerifyContains("0", new Line(new Point(1, 1, 1), new Point(2, 3, 4), Color.Auto));
        }
    }
}
