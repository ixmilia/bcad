using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("File.New", "NEW", ModifierKeys.Control, Key.N, "new", "n")]
    public class NewCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var unsaved = await workspace.PromptForUnsavedChanges();
            if (unsaved == UnsavedChangesResult.Cancel)
            {
                return false;
            }

            workspace.Update(drawing: new Drawing(), activeViewPort: ViewPort.CreateDefaultViewPort(), isDirty: false);
            workspace.UndoRedoService.ClearHistory();
            return true;
        }
    }
}
