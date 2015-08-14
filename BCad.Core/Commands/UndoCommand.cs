using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Undo", "UNDO", ModifierKeys.Control, Key.Z, "undo", "u")]
    public class UndoCommandCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var undoRedoService = workspace.GetService<IUndoRedoService>();
            if (undoRedoService.UndoHistorySize == 0)
            {
                workspace.GetService<IOutputService>().WriteLine("Nothing to undo");
                return Task.FromResult(false);
            }

            undoRedoService.Undo();
            return Task.FromResult(true);
        }
    }
}
