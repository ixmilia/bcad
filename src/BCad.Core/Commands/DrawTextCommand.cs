using System.Threading.Tasks;
using BCad.Entities;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Text", "TEXT", "text", "t")]
    public class DrawTextCommand : ICadCommand
    {
        private static double lastHeight = 1.0;

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            // get location
            var input = await workspace.InputService.GetPoint(new UserDirective("Location"));
            if (input.Cancel || !input.HasValue) return false;
            var location = input.Value;

            // get height
            var heightInput = await workspace.InputService.GetDistance("Text height or first point", lastHeight);
            if (heightInput.Cancel || !heightInput.HasValue) return false;
            var height = heightInput.Value;
            lastHeight = height;

            // get text
            var textInput = await workspace.InputService.GetText();
            if (textInput.Cancel || !textInput.HasValue) return false;
            var text = textInput.Value;

            // add it
            workspace.AddToCurrentLayer(new Text(text, location, workspace.DrawingPlane.Normal, height, 0.0, null));

            return true;
        }
    }
}
