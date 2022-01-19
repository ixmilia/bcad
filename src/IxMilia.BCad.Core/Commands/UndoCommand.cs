using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class UndoCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg)
        {
            if (workspace.UndoRedoService.UndoHistorySize == 0)
            {
                workspace.OutputService.WriteLine("Nothing to undo");
                return Task.FromResult(false);
            }

            workspace.UndoRedoService.Undo();
            return Task.FromResult(true);
        }
    }
}
