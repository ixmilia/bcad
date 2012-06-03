using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using BCad.Entities;
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
            IEnumerable<Entity> removed;
            IEnumerable<Entity> added;
            string entityLayerName;
            while (!selected.Cancel && selected.HasValue)
            {
                entityLayerName = drawing.ContainingLayer(selected.Value.Entity).Name;
                TrimExtendService.Trim(drawing, selected.Value, boundaryPrimitives, out removed, out added);

                foreach (var ent in removed)
                    drawing = drawing.Remove(ent);

                foreach (var ent in added)
                    drawing = drawing.Add(drawing.Layers[entityLayerName], ent);

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
