using System.Threading.Tasks;
using BCad.Extensions;
using BCad.Services;
using BCad.Utilities;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Offset", "OFFSET", "offset", "off", "of")]
    public class OffsetCommand : ICadCommand
    {
        private static double lastOffsetDistance = 0.0;

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawingPlane = workspace.DrawingPlane;
            var inputService = workspace.GetService<IInputService>();
            var distance = await inputService.GetDistance(defaultDistance: lastOffsetDistance);
            if (distance.Cancel)
            {
                return false;
            }

            double dist;
            if (distance.HasValue)
            {
                dist = distance.Value;
            }
            else
            {
                dist = lastOffsetDistance;
            }

            var outputService = workspace.GetService<IOutputService>();
            outputService.WriteLine("Using offset distance of {0}", dist);
            lastOffsetDistance = dist;
            var selection = await inputService.GetEntity(new UserDirective("Select entity"));
            while (!selection.Cancel && selection.HasValue)
            {
                var ent = selection.Value.Entity;
                if (!EditUtilities.CanOffsetEntity(ent))
                {
                    outputService.WriteLine("Unable to offset {0}", ent.Kind);
                    selection = await inputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                if (!drawingPlane.Contains(ent))
                {
                    outputService.WriteLine("Entity must be entirely on the drawing plane to offset");
                    selection = await inputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                workspace.SelectedEntities.Clear();
                workspace.SelectedEntities.Add(ent);
                var point = await inputService.GetPoint(new UserDirective("Side to offset"));
                if (point.Cancel || !point.HasValue)
                {
                    break;
                }

                if (!drawingPlane.Contains(point.Value))
                {
                    outputService.WriteLine("Point must be on the drawing plane to offset");
                    selection = await inputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                // do the actual offset
                var updated = EditUtilities.Offset(workspace, ent, point.Value, dist);

                if (updated != null)
                {
                    var oldLayer = workspace.Drawing.ContainingLayer(ent);
                    workspace.Add(oldLayer, updated);
                }

                workspace.SelectedEntities.Clear();
                selection = await inputService.GetEntity(new UserDirective("Select entity"));
            }

            return true;
        }
    }
}
