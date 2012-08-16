using System.ComponentModel.Composition;
using System.Diagnostics;
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

        public bool Execute(object arg)
        {
            var distance = InputService.GetDistance(lastOffsetDistance);
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
            var selection = InputService.GetEntity(new UserDirective("Select entity"));
            while (!selection.Cancel && selection.HasValue)
            {
                var ent = selection.Value.Entity;
                if (!EditService.CanOffsetEntity(ent))
                {
                    InputService.WriteLine("Unable to offset {0}", ent.Kind);
                    selection = InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                if (!Workspace.DrawingPlane.Contains(ent))
                {
                    InputService.WriteLine("Entity must be entirely on the drawing plane to offset");
                    selection = InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                Workspace.SelectedEntities.Clear();
                Workspace.SelectedEntities.Add(ent);
                var point = InputService.GetPoint(new UserDirective("Side to offset"));
                if (point.Cancel || !point.HasValue)
                {
                    break;
                }

                if (!Workspace.DrawingPlane.Contains(point.Value))
                {
                    InputService.WriteLine("Point must be on the drawing plane to offset");
                    selection = InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                // do the actual offset
                var updated = EditService.Offset(Workspace, ent, point.Value, dist);

                if (updated != null)
                {
                    Workspace.AddToCurrentLayer(updated);
                }

                Workspace.SelectedEntities.Clear();
                selection = InputService.GetEntity(new UserDirective("Select entity"));
            }

            return true;
        }

        public string DisplayName
        {
            get { return "OFFSET"; }
        }
    }
}
