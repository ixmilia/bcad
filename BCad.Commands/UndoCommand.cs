using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Edit.Undo", ModifierKeys.Control, Key.Z, "undo", "u")]
    internal class UndoCommandCommand : ICommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        public bool Execute(params object[] param)
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
