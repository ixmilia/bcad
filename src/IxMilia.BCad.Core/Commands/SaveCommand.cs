using System.IO;
using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class SaveCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var drawing = workspace.Drawing;
            string fileName = drawing.Settings.FileName;
            if (fileName == null)
            {
                fileName = await workspace.GetDrawingFilenameFromUserForSave();
                if (string.IsNullOrEmpty(fileName))
                    return false;
            }

            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                if (!await workspace.ReaderWriterService.TryWriteDrawing(fileName, drawing, workspace.ActiveViewPort, stream, preserveSettings: true))
                    return false;
            }

            SaveAsCommand.UpdateDrawingFileName(workspace, fileName);

            return true;
        }
    }
}
