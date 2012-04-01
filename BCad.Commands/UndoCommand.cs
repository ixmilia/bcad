using System.ComponentModel.Composition;

namespace BCad.Commands
{
    [ExportCommand("Edit.Undo", "undo", "u")]
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
