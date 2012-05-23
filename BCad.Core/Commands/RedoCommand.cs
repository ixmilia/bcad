using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Edit.Redo", ModifierKeys.Control, Key.Y, "redo", "re", "r")]
    public class RedoCommandCommand : ICommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        public bool Execute(object arg)
        {
            if (UndoRedoService.RedoHistorySize == 0)
                return false;

            UndoRedoService.Redo();
            return true;
        }

        public string DisplayName
        {
            get { return "REDO"; }
        }
    }
}
