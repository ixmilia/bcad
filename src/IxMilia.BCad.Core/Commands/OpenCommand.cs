using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Settings;

namespace IxMilia.BCad.Commands
{
    public class OpenCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            if (await workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
                return false;

            string fileName = null;
            if (arg is string)
                fileName = (string)arg;

            if (fileName == null)
                fileName = await workspace.FileSystemService.GetFileNameFromUserForOpen();

            if (fileName == null)
                return false; // cancel

            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var result = await workspace.ReaderWriterService.ReadDrawing(fileName, stream, workspace.FileSystemService.GetContentResolverRelativeToPath(fileName));
                if (!result.Success)
                {
                    return false;
                }

                if (result.Drawing == null)
                {
                    return false;
                }

                var activeViewPort = result.ViewPort
                    ?? result.Drawing.ShowAllViewPort(
                        Vector.ZAxis,
                        Vector.YAxis,
                        workspace.ViewControl.DisplayWidth,
                        workspace.ViewControl.DisplayHeight);

                workspace.SettingsService.SetValue(DefaultSettingsNames.DrawingPrecision, result.Drawing.Settings.UnitPrecision);
                workspace.SettingsService.SetValue(DefaultSettingsNames.DrawingUnits, result.Drawing.Settings.UnitFormat);
                workspace.Update(drawing: result.Drawing, activeViewPort: activeViewPort, isDirty: false);
                workspace.UndoRedoService.ClearHistory();
            }

            return true;
        }
    }
}
