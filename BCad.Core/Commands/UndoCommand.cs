using System.ComponentModel.Composition;
using System.Windows.Input;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Undo", ModifierKeys.Control, Key.Z, "undo", "u")]
    public class UndoCommandCommand : ICommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        public bool Execute(object arg)
        {
            if (UndoRedoService.UndoHistorySize == 0)
                return false;

            UndoRedoService.Undo();
            return true;
        }

        public string DisplayName
        {
            get { return "UNDO"; }
        }
    }
}
