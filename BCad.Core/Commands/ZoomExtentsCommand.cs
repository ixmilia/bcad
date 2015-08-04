using System.Composition;
using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCommand("Zoom.Extents", "ZOOMEXTENTS", "zoomextents", "ze")]
    internal class ZoomExtentsCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public Task<bool> Execute(object arg = null)
        {
            var newVp = Workspace.Drawing.ShowAllViewPort(
                Workspace.ActiveViewPort.Sight,
                Workspace.ActiveViewPort.Up,
                Workspace.ViewControl.DisplayWidth,
                Workspace.ViewControl.DisplayHeight);
            if (newVp != null)
            {
                Workspace.Update(activeViewPort: newVp);
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
