using System.Composition;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("File.SaveAs", "SAVEAS", "saveas", "sa")]
    public class SaveAsCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IFileSystemService FileSystemService { get; set; }

        public Task<bool> Execute(object arg)
        {
            var drawing = Workspace.Drawing;
            string fileName = null;
            if (arg is string && !string.IsNullOrEmpty((string)arg))
                fileName = (string)arg;

            if (fileName == null)
                fileName = FileSystemService.GetFileNameFromUserForSave();

            if (fileName == null)
                return Task.FromResult<bool>(false);

            if (!FileSystemService.TryWriteDrawing(fileName, drawing, Workspace.ActiveViewPort))
                return Task.FromResult<bool>(false);

            UpdateDrawingFileName(Workspace, fileName);

            return Task.FromResult<bool>(true);
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
