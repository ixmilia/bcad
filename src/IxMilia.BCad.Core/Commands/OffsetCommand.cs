using System.Threading.Tasks;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    public class OffsetCommand : ICadCommand
    {
        private static double lastOffsetDistance = 0.0;

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawingPlane = workspace.DrawingPlane;
            var distance = await workspace.InputService.GetDistance(defaultDistance: lastOffsetDistance);
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

            workspace.OutputService.WriteLine("Using offset distance of {0}", dist);
            lastOffsetDistance = dist;
            var selection = await workspace.InputService.GetEntity(new UserDirective("Select entity"));
            while (!selection.Cancel && selection.HasValue)
            {
                var ent = selection.Value.Entity;
                if (!EditUtilities.CanOffsetEntity(ent))
                {
                    workspace.OutputService.WriteLine("Unable to offset {0}", ent.Kind);
                    selection = await workspace.InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                if (!drawingPlane.Contains(ent))
                {
                    workspace.OutputService.WriteLine("Entity must be entirely on the drawing plane to offset");
                    selection = await workspace.InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                workspace.SelectedEntities.Clear();
                workspace.SelectedEntities.Add(ent);
                var point = await workspace.InputService.GetPoint(new UserDirective("Side to offset"));
                if (point.Cancel || !point.HasValue)
                {
                    break;
                }

                if (!drawingPlane.Contains(point.Value))
                {
                    workspace.OutputService.WriteLine("Point must be on the drawing plane to offset");
                    selection = await workspace.InputService.GetEntity(new UserDirective("Select entity"));
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
                selection = await workspace.InputService.GetEntity(new UserDirective("Select entity"));
            }

            return true;
        }
    }
}
