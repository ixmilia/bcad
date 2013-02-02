using System.ComponentModel.Composition;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Draw.Text", "text", "t")]
    public class DrawTextCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

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
            Workspace.AddToCurrentLayer(new Text(text, location, Workspace.DrawingPlane.Normal, height, 0.0, Color.Default));

            return true;
        }

        public string DisplayName
        {
            get { return "TEXT"; }
        }
    }
}
