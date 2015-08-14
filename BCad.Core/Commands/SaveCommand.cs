using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("File.Save", "SAVE", ModifierKeys.Control, Key.S, "save", "s")]
    public class SaveCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawing = workspace.Drawing;
            var fileSystemService = workspace.GetService<IFileSystemService>();
            string fileName = drawing.Settings.FileName;
            if (fileName == null)
            {
                fileName = await fileSystemService.GetFileNameFromUserForSave();
                if (fileName == null)
                    return false;
            }

            if (!await fileSystemService.TryWriteDrawing(fileName, drawing, workspace.ActiveViewPort))
                return false;

            SaveAsCommand.UpdateDrawingFileName(workspace, fileName);

            return true;
        }
    }
}
