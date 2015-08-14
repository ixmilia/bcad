using System.Threading.Tasks;
using BCad.Services;

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

            var fileSystemService = workspace.GetService<IFileSystemService>();
            if (filename == null)
                filename = await fileSystemService.GetFileNameFromUserForOpen();

            if (filename == null)
                return false; // cancel

            Drawing drawing;
            ViewPort activeViewPort;
            await fileSystemService.TryReadDrawing(filename, out drawing, out activeViewPort);
            workspace.Update(drawing: drawing, activeViewPort: activeViewPort, isDirty: false);
            workspace.GetService<IUndoRedoService>().ClearHistory();

            return true;
        }
    }
}
