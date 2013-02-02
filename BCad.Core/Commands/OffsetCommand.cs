using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Offset", "offset", "off", "of")]
    public class OffsetCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IEditService EditService = null;

        [Import]
        private IWorkspace Workspace = null;

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

            InputService.WriteLine("Using offset distance of {0}", dist);
            lastOffsetDistance = dist;
            var selection = await InputService.GetEntity(new UserDirective("Select entity"));
            while (!selection.Cancel && selection.HasValue)
            {
                var ent = selection.Value.Entity;
                if (!EditService.CanOffsetEntity(ent))
                {
                    InputService.WriteLine("Unable to offset {0}", ent.Kind);
                    selection = await InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                if (!drawingPlane.Contains(ent))
                {
                    InputService.WriteLine("Entity must be entirely on the drawing plane to offset");
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
                    InputService.WriteLine("Point must be on the drawing plane to offset");
                    selection = await InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                // do the actual offset
                var updated = EditService.Offset(Workspace, ent, point.Value, dist);

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

        public string DisplayName
        {
            get { return "OFFSET"; }
        }
    }
}
