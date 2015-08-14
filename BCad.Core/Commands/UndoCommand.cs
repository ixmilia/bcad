using System.Composition;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Undo", "UNDO", ModifierKeys.Control, Key.Z, "undo", "u")]
    public class UndoCommandCommand : ICadCommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        [Import]
        public IOutputService OutputService { get; set; }

        public Task<bool> Execute(object arg)
        {
            if (UndoRedoService.UndoHistorySize == 0)
            {
                OutputService.WriteLine("Nothing to undo");
                return Task.FromResult(false);
            }

            UndoRedoService.Undo();
            return Task.FromResult(true);
        }
    }
}
