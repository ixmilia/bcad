using System.ComponentModel.Composition;
using System.Linq;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Trim", "trim", "tr", "t")]
    public class TrimCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private ITrimExtendService TrimExtendService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var boundaries = InputService.GetEntities("Select cutting edges");
            if (boundaries.Cancel || !boundaries.HasValue || !boundaries.Value.Any())
            {
                return false;
            }

            var boundaryPrimitives = boundaries.Value.SelectMany(b => b.GetPrimitives());
            Workspace.SelectedEntities.Set(boundaries.Value);

            var drawing = Workspace.Drawing;
            var directive = new UserDirective("Entity to trim");
            var selected = InputService.GetEntity(directive);
            while (!selected.Cancel && selected.HasValue)
            {
                drawing = TrimExtendService.Trim(drawing, selected.Value, boundaryPrimitives);

                // commit the change
                if (Workspace.Drawing != drawing)
                    Workspace.Drawing = drawing;

                // get next entity to trim
                selected = InputService.GetEntity(directive);
            }

            Workspace.SelectedEntities.Clear();
            return true;
        }

        public string DisplayName
        {
            get { return "TRIM"; }
        }
    }
}
