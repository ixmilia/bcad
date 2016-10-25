using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("Zoom.Extents", "ZOOMEXTENTS", "zoomextents", "ze")]
    internal class ZoomExtentsCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var newVp = workspace.Drawing.ShowAllViewPort(
                workspace.ActiveViewPort.Sight,
                workspace.ActiveViewPort.Up,
                workspace.ViewControl.DisplayWidth,
                workspace.ViewControl.DisplayHeight);
            if (newVp != null)
            {
                workspace.Update(activeViewPort: newVp);
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
