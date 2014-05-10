using System.Composition;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Undo", "UNDO")]
    public class UndoCommandCommand : ICommand
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
