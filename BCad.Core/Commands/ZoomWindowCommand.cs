using System.Composition;
using System.Threading.Tasks;
using BCad.Extensions;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCommand("Zoom.Window", "ZOOMWINDOW", "zw")]
    internal class ZoomWindowCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public async Task<bool> Execute(object arg = null)
        {
            var selection = await Workspace.ViewControl.GetSelectionRectangle();
            if (selection == null)
                return false;

            var transform = Workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(Workspace.ViewControl.DisplayWidth, Workspace.ViewControl.DisplayHeight);
            var newVp = GetBoundingPrimitives(selection.TopLeftWorld, selection.BottomRightWorld).ShowAllViewPort(
                Workspace.ActiveViewPort.Sight,
                Workspace.ActiveViewPort.Up,
                Workspace.ViewControl.DisplayWidth,
                Workspace.ViewControl.DisplayHeight,
                pixelBuffer: 0);

            if (newVp == null)
                return false;

            Workspace.Update(activeViewPort: newVp);
            return true;
        }

        private static PrimitiveLine[] GetBoundingPrimitives(Point p1, Point p2)
        {
            var a = p1;
            var b = new Point(p1.X, p2.Y, p1.Z);
            var c = new Point(p2.X, p1.Y, p2.Z);
            var d = p2;
            return new[]
            {
                new PrimitiveLine(a, b),
                new PrimitiveLine(a, c),
                new PrimitiveLine(b, d),
                new PrimitiveLine(c, d)
            };
        }
    }
}
