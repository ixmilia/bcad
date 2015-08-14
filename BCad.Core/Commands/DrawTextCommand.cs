using System.Threading.Tasks;
using BCad.Entities;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Text", "TEXT", "text", "t")]
    public class DrawTextCommand : ICadCommand
    {
        private static double lastHeight = 1.0;

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var inputService = workspace.GetService<IInputService>();

            // get location
            var input = await inputService.GetPoint(new UserDirective("Location"));
            if (input.Cancel || !input.HasValue) return false;
            var location = input.Value;

            // get height
            var heightInput = await inputService.GetDistance("Text height or first point", lastHeight);
            if (heightInput.Cancel || !heightInput.HasValue) return false;
            var height = heightInput.Value;
            lastHeight = height;

            // get text
            var textInput = await inputService.GetText();
            if (textInput.Cancel || !textInput.HasValue) return false;
            var text = textInput.Value;

            // add it
            workspace.AddToCurrentLayer(new Text(text, location, workspace.DrawingPlane.Normal, height, 0.0, null));

            return true;
        }
    }
}
