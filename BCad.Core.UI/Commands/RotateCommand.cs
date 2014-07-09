using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using BCad.Services;
using BCad.Utilities;

namespace BCad.Commands
{
    [ExportUICommand("Edit.Rotate", "ROTATE", "rotate", "rot", "ro")]
    public class RotateCommand : IUICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var entities = await InputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            if (!entities.Value.Any())
            {
                return true;
            }

            var origin = await InputService.GetPoint(new UserDirective("Point of rotation"));
            if (origin.Cancel || !origin.HasValue)
            {
                return false;
            }

            var angleText = await InputService.GetText(prompt: "Angle of rotation");
            if (angleText.Cancel || !angleText.HasValue)
            {
                return false;
            }

            double angle;
            if (double.TryParse(angleText.Value, out angle))
            {
                var drawing = Workspace.Drawing;
                foreach (var ent in entities.Value)
                {
                    drawing = drawing.Replace(ent, EditUtilities.Rotate(ent, origin.Value, angle));
                }

                Workspace.Update(drawing: drawing);
                return true;
            }

            return false;
        }
    }
}
