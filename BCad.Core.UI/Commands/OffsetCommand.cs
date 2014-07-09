using System.Composition;
using System.Threading.Tasks;
using BCad.Extensions;
using BCad.Services;
using BCad.Utilities;

namespace BCad.Commands
{
    [ExportUICommand("Edit.Offset", "OFFSET", "offset", "off", "of")]
    public class OffsetCommand : IUICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IOutputService OutputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        private static double lastOffsetDistance = 0.0;

        public async Task<bool> Execute(object arg)
        {
            var drawingPlane = Workspace.DrawingPlane;
            var distance = await InputService.GetDistance(defaultDistance: lastOffsetDistance);
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

            OutputService.WriteLine("Using offset distance of {0}", dist);
            lastOffsetDistance = dist;
            var selection = await InputService.GetEntity(new UserDirective("Select entity"));
            while (!selection.Cancel && selection.HasValue)
            {
                var ent = selection.Value.Entity;
                if (!EditUtilities.CanOffsetEntity(ent))
                {
                    OutputService.WriteLine("Unable to offset {0}", ent.Kind);
                    selection = await InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                if (!drawingPlane.Contains(ent))
                {
                    OutputService.WriteLine("Entity must be entirely on the drawing plane to offset");
                    selection = await InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                Workspace.SelectedEntities.Clear();
                Workspace.SelectedEntities.Add(ent);
                var point = await InputService.GetPoint(new UserDirective("Side to offset"));
                if (point.Cancel || !point.HasValue)
                {
                    break;
                }

                if (!drawingPlane.Contains(point.Value))
                {
                    OutputService.WriteLine("Point must be on the drawing plane to offset");
                    selection = await InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                // do the actual offset
                var updated = EditUtilities.Offset(Workspace, ent, point.Value, dist);

                if (updated != null)
                {
                    var oldLayer = Workspace.Drawing.ContainingLayer(ent);
                    Workspace.Add(oldLayer, updated);
                }

                Workspace.SelectedEntities.Clear();
                selection = await InputService.GetEntity(new UserDirective("Select entity"));
            }

            return true;
        }
    }
}
