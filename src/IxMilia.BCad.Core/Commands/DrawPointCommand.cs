using System.Threading.Tasks;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.Commands
{
    public class DrawPointCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var location = await workspace.InputService.GetPoint(new UserDirective("Location"));
            if (location.Cancel) return false;
            if (!location.HasValue) return true;
            workspace.AddToCurrentLayer(new Location(location.Value, lineTypeSpecification: workspace.Drawing.Settings.CurrentLineTypeSpecification));
            return true;
        }
    }
}
