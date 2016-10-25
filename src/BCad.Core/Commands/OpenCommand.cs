using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("File.Open", "OPEN", ModifierKeys.Control, Key.O, "open", "o")]
    public class OpenCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            if (await workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
                return false;

            string filename = null;
            if (arg is string)
                filename = (string)arg;

            if (filename == null)
                filename = await workspace.FileSystemService.GetFileNameFromUserForOpen();

            if (filename == null)
                return false; // cancel

            Drawing drawing;
            ViewPort activeViewPort;
            var result = await workspace.FileSystemService.TryReadDrawing(filename, out drawing, out activeViewPort);
            if (!result)
            {
                return false;
            }

            if (drawing == null)
            {
                return false;
            }

            if (activeViewPort == null)
            {
                activeViewPort = drawing.ShowAllViewPort(
                    Vector.ZAxis,
                    Vector.YAxis,
                    workspace.ViewControl.DisplayWidth,
                    workspace.ViewControl.DisplayHeight);
            }

            workspace.Update(drawing: drawing, activeViewPort: activeViewPort, isDirty: false);
            workspace.UndoRedoService.ClearHistory();

            return true;
        }
    }
}
