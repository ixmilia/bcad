using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Redo", "REDO", ModifierKeys.Control, Key.Y, "redo", "re", "r")]
    public class RedoCommandCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var undoRedoService = workspace.GetService<IUndoRedoService>();
            if (undoRedoService.RedoHistorySize == 0)
            {
                workspace.GetService<IOutputService>().WriteLine("Nothing to redo");
                return Task.FromResult<bool>(false);
            }

            undoRedoService.Redo();
            return Task.FromResult(true);
        }
    }
}
