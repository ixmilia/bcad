using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.Save", "SAVE", ModifierKeys.Control, Key.S, "save", "s")]
    public class SaveCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IFileSystemService FileSystemService = null;

        public Task<bool> Execute(object arg)
        {
            var drawing = Workspace.Drawing;
            string fileName = drawing.Settings.FileName;
            if (fileName == null)
            {
                fileName = FileSystemService.GetFileNameFromUserForSave();
                if (fileName == null)
                    return Task.FromResult<bool>(false);
            }

            if (!FileSystemService.TryWriteDrawing(fileName, drawing, Workspace.ActiveViewPort))
                return Task.FromResult<bool>(false);

            SaveAsCommand.UpdateDrawingFileName(Workspace, fileName);

            return Task.FromResult<bool>(true);
        }
    }
}
