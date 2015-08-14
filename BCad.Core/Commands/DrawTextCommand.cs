using System.Composition;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Text", "TEXT", "text", "t")]
    public class DrawTextCommand : ICadCommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        private static double lastHeight = 1.0;

        public async Task<bool> Execute(object arg)
        {
            // get location
            var input = await InputService.GetPoint(new UserDirective("Location"));
            if (input.Cancel || !input.HasValue) return false;
            var location = input.Value;

            // get height
            var heightInput = await InputService.GetDistance("Text height or first point", lastHeight);
            if (heightInput.Cancel || !heightInput.HasValue) return false;
            var height = heightInput.Value;
            lastHeight = height;

            // get text
            var textInput = await InputService.GetText();
            if (textInput.Cancel || !textInput.HasValue) return false;
            var text = textInput.Value;

            // add it
            Workspace.AddToCurrentLayer(new Text(text, location, Workspace.DrawingPlane.Normal, height, 0.0, null));

            return true;
        }
    }
}
