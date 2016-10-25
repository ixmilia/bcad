using System.Linq;
using System.Threading.Tasks;
using BCad.Utilities;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Rotate", "ROTATE", "rotate", "rot", "ro")]
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

            var angleText = await workspace.InputService.GetText(prompt: "Angle of rotation");
            if (angleText.Cancel || !angleText.HasValue)
            {
                return false;
            }

            double angle;
            if (double.TryParse(angleText.Value, out angle))
            {
                var drawing = workspace.Drawing;
                foreach (var ent in entities.Value)
                {
                    drawing = drawing.Replace(ent, EditUtilities.Rotate(ent, origin.Value, angle));
                }

                workspace.Update(drawing: drawing);
                return true;
            }

            return false;
        }
    }
}
