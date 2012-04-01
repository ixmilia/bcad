using System.ComponentModel.Composition;

namespace BCad.Commands
{
    [ExportCommand("Edit.Redo", "redo", "re")]
    internal class RedoCommandCommand : ICommand
    {
        [Import]
        public IUndoRedoService UndoRedoService { get; set; }

        public bool Execute(params object[] param)
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
