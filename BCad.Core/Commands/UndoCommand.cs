using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Undo", ModifierKeys.Control, Key.Z, "undo", "u")]
    public class UndoCommandCommand : ICommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        public Task<bool> Execute(object arg)
        {
            if (UndoRedoService.UndoHistorySize == 0)
                return Task.FromResult<bool>(false);

            UndoRedoService.Undo();
            return Task.FromResult<bool>(true);
        }

        public string DisplayName
        {
            get { return "UNDO"; }
        }
    }
}
