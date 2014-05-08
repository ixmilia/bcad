using System.Composition;
using System.Threading.Tasks;
using BCad.Services;
using System.Collections.Generic;

namespace BCad.Commands
{
    [ExportCommand("File.Open", "OPEN", ModifierKeys.Control, Key.O, "open", "o")]
    public class OpenCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        [Import]
        public IFileSystemService FileSystemService { get; set; }

        public async Task<bool> Execute(object arg)
        {
            if (await Workspace.PromptForUnsavedChanges() == UnsavedChangesResult.Cancel)
                return false;

            string filename = null;
            if (arg is string)
                filename = (string)arg;

            if (filename == null)
                filename = await FileSystemService.GetFileNameFromUserForOpen();

            if (filename == null)
                return false; // cancel

            Drawing drawing;
            ViewPort activeViewPort;
            Dictionary<string, object> propertyBag;
            await FileSystemService.TryReadDrawing(filename, out drawing, out activeViewPort, out propertyBag);
            Workspace.Update(drawing: drawing, activeViewPort: activeViewPort, isDirty: false);

            bool isColorMapSet = false;
            if (propertyBag != null)
            {
                foreach (var key in propertyBag.Keys)
                {
                    switch (key)
                    {
                        case "ColorMap":
                            var colorMap = propertyBag[key] as ColorMap;
                            Workspace.SettingsManager.ColorMap = colorMap ?? ColorMap.Default;
                            isColorMapSet = true;
                            break;
                        default:
                            // unsupported property
                            break;
                    }
                }
            }

            if (!isColorMapSet)
            {
                Workspace.SettingsManager.ColorMap = ColorMap.Default;
            }

            UndoRedoService.ClearHistory();

            return true;
        }
    }
}
