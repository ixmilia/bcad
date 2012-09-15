using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Delete", ModifierKeys.None, Key.Delete, "delete", "d", "del")]
    public class DeleteCommand : ICommand
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

            if (!entities.Value.Any())
            {
                return true;
            }

            var dwg = Workspace.Drawing;
            foreach (var ent in entities.Value)
            {
                dwg = dwg.Remove(ent);
            }

            Workspace.Update(drawing: dwg);
            return true;
        }

        public string DisplayName
        {
            get { return "DELETE"; }
        }
    }
}
