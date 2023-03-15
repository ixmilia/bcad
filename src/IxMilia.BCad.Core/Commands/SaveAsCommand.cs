using System.IO;
using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class SaveAsCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            string fileName = null;
            if (arg is string && !string.IsNullOrEmpty((string)arg))
                fileName = (string)arg;

            if (fileName == null)
                fileName = await workspace.GetDrawingFilenameFromUserForSave();

            if (string.IsNullOrEmpty(fileName))
                return false;

            var result = await SaveFile(workspace, fileName, false);
            return result;
        }

        internal static async Task<bool> SaveFile(IWorkspace workspace, string fileName, bool preserveSettings)
        {
            if (File.Exists(fileName))
            {
                // copy old file, just in case
                var backupFileName = $"{fileName}.bcad.bak";
                File.Copy(fileName, backupFileName, true);
            }

            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                if (!await workspace.ReaderWriterService.TryWriteDrawing(fileName, workspace.Drawing, workspace.ActiveViewPort, stream, preserveSettings))
                    return false;
            }

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
