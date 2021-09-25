using System.IO;
using System.Threading.Tasks;

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
                var result = await workspace.ReaderWriterService.TryReadDrawing(fileName, stream, out var drawing, out var activeViewPort);
                if (!result)
                {
                    return false;
                }

                if (drawing == null)
                {
                    return false;
                }

                if (activeViewPort == null)
                {
                    activeViewPort = drawing.ShowAllViewPort(
                        Vector.ZAxis,
                        Vector.YAxis,
                        workspace.ViewControl.DisplayWidth,
                        workspace.ViewControl.DisplayHeight);
                }

                workspace.Update(drawing: drawing, activeViewPort: activeViewPort, isDirty: false);
                workspace.UndoRedoService.ClearHistory();
            }

            return true;
        }
    }
}
