using System.Composition;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.Save", "SAVE", ModifierKeys.Control, Key.S, "save", "s")]
    public class SaveCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IFileSystemService FileSystemService { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var drawing = Workspace.Drawing;
            string fileName = drawing.Settings.FileName;
            if (fileName == null)
            {
                fileName = await FileSystemService.GetFileNameFromUserForSave();
                if (fileName == null)
                    return false;
            }

            if (!await FileSystemService.TryWriteDrawing(fileName, drawing, Workspace.ActiveViewPort, null))
                return false;

            SaveAsCommand.UpdateDrawingFileName(Workspace, fileName);

            return true;
        }
    }
}
