using BCad.Services;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Edit.Delete", "DELETE", ModifierKeys.None, Key.Delete, "delete", "d", "del")]
    public class DeleteCommand : ICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var entities = await InputService.GetEntities();
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
    }
}
