using System.Composition;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Redo", "REDO", ModifierKeys.Control, Key.Y, "redo", "re", "r")]
    public class RedoCommandCommand : ICommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        [Import]
        public IOutputService OutputService { get; set; }

        public Task<bool> Execute(object arg)
        {
            if (UndoRedoService.RedoHistorySize == 0)
            {
                OutputService.WriteLine("Nothing to redo");
                return Task.FromResult<bool>(false);
            }

            UndoRedoService.Redo();
            return Task.FromResult<bool>(true);
        }
    }
}
