using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Redo", ModifierKeys.Control, Key.Y, "redo", "re", "r")]
    public class RedoCommandCommand : ICommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        public Task<bool> Execute(object arg)
        {
            if (UndoRedoService.RedoHistorySize == 0)
                return Task.FromResult<bool>(false);

            UndoRedoService.Redo();
            return Task.FromResult<bool>(true);
        }

        public string DisplayName
        {
            get { return "REDO"; }
        }
    }
}
