using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Edit.Delete", ModifierKeys.None, Key.Delete, "delete", "d", "del")]
    internal class DeleteCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var entities = InputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            var doc = Workspace.Document;
            foreach (var ent in entities.Value)
            {
                doc = doc.Remove(ent);
            }

            Workspace.Document = doc;
            return true;
        }

        public string DisplayName
        {
            get { return "DELETE"; }
        }
    }
}
