using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    public class RotateCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var entities = await workspace.InputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            if (!entities.Value.Any())
            {
                return true;
            }

            var origin = await workspace.InputService.GetPoint(new UserDirective("Point of rotation"));
            if (origin.Cancel || !origin.HasValue)
            {
                return false;
            }

            var entityPrimitives = entities.Value.SelectMany(e => e.GetPrimitives(workspace.Drawing.Settings)).ToArray();
            var angleValue = await workspace.InputService.GetAngleInDegrees("Angle of rotation", onCursorMove: angleInDegrees => entityPrimitives.Select(p => EditUtilities.Rotate(p, origin.Value, angleInDegrees)));
            if (angleValue.Cancel || !angleValue.HasValue)
            {
                return false;
            }

            var rotationAngle = angleValue.Value;
            var drawing = workspace.Drawing;
            foreach (var ent in entities.Value)
            {
                drawing = drawing.Replace(ent, EditUtilities.Rotate(ent, origin.Value, rotationAngle));
            }

            workspace.Update(drawing: drawing);
            return true;
        }
    }
}
