using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("File.Save", "SAVE", ModifierKeys.Control, Key.S, "save", "s")]
    public class SaveCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawing = workspace.Drawing;
            string fileName = drawing.Settings.FileName;
            if (fileName == null)
            {
                fileName = await workspace.FileSystemService.GetFileNameFromUserForSave();
                if (fileName == null)
                    return false;
            }

            if (!await workspace.FileSystemService.TryWriteDrawing(fileName, drawing, workspace.ActiveViewPort))
                return false;

            SaveAsCommand.UpdateDrawingFileName(workspace, fileName);

            return true;
        }
    }
}
