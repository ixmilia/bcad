using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.Open", "OPEN", ModifierKeys.Control, Key.O, "open", "o")]
    public class OpenCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IUndoRedoService UndoRedoService = null;

        [Import]
        private IFileSystemService FileSystemService = null;

        public async Task<bool> Execute(object arg)
        {
            if (await Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
                return false;

            string filename = null;
            if (arg is string)
                filename = (string)arg;

            if (filename == null)
                filename = FileSystemService.GetFileNameFromUserForOpen();

            if (filename == null)
                return false; // cancel

            Drawing drawing;
            ViewPort activeViewPort;
            FileSystemService.TryReadDrawing(filename, out drawing, out activeViewPort);
            Workspace.Update(drawing: drawing, activeViewPort: activeViewPort, isDirty: false);
            UndoRedoService.ClearHistory();

            return true;
        }
    }
}
