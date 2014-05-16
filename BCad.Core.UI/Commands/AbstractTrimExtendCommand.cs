using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    public abstract class AbstractTrimExtendCommand : IUICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var boundaries = await InputService.GetEntities(GetBoundsText());
            if (boundaries.Cancel || !boundaries.HasValue || !boundaries.Value.Any())
            {
                return false;
            }

            var boundaryPrimitives = boundaries.Value.SelectMany(b => b.GetPrimitives());

            var drawing = Workspace.Drawing;
            var directive = new UserDirective(GetTrimExtendText());
            var selected = await InputService.GetEntity(directive);
            IEnumerable<Entity> removed;
            IEnumerable<Entity> added;
            string entityLayerName;
            while (!selected.Cancel && selected.HasValue)
            {
                entityLayerName = drawing.ContainingLayer(selected.Value.Entity).Name;
                DoTrimExtend(selected.Value, boundaryPrimitives, out removed, out added);

                foreach (var ent in removed)
                    drawing = drawing.Remove(ent);

                foreach (var ent in added)
                    drawing = drawing.Add(drawing.Layers.GetValue(entityLayerName), ent);

                // commit the change
                if (Workspace.Drawing != drawing)
                    Workspace.Update(drawing: drawing);

                // get next entity to trim/extend
                selected = await InputService.GetEntity(directive);
            }

            return true;
        }

        protected abstract string GetBoundsText();
        protected abstract string GetTrimExtendText();
        protected abstract void DoTrimExtend(SelectedEntity selectedEntity, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added);
    }
}
