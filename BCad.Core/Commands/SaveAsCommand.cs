using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("File.SaveAs", "SAVEAS", "saveas", "sa")]
    public class SaveAsCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawing = workspace.Drawing;
            string fileName = null;
            if (arg is string && !string.IsNullOrEmpty((string)arg))
                fileName = (string)arg;

            var fileSystemService = workspace.GetService<IFileSystemService>();
            if (fileName == null)
                fileName = await fileSystemService.GetFileNameFromUserForSave();

            if (fileName == null)
                return false;

            if (!await fileSystemService.TryWriteDrawing(fileName, drawing, workspace.ActiveViewPort))
                return false;

            UpdateDrawingFileName(workspace, fileName);

            return true;
        }

        internal static void UpdateDrawingFileName(IWorkspace workspace, string fileName)
        {
            var drawing = workspace.Drawing;
            if (drawing.Settings.FileName != fileName)
            {
                var newSettings = drawing.Settings.Update(fileName: fileName);
                var newDrawing = drawing.Update(settings: newSettings);
                workspace.Update(drawing: newDrawing, isDirty: false);
            }
            else
            {
                workspace.Update(isDirty: false);
            }
        }
    }
}
