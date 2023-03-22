using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.Commands
{
    public abstract class CombinePolylinesCommandBase : ICadCommand
    {
        protected abstract IEnumerable<Entity> Combine(IEnumerable<Entity> entities, DrawingSettings settings);

        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var inputService = workspace.InputService;
            var entities = await inputService.GetEntities("Select polylines", entityKinds: EntityKind.Circle | EntityKind.Polyline);
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            var polys = entities.Value;
            if (polys.Count() <= 1)
            {
                return false;
            }

            var drawing = workspace.Drawing;
            foreach (var poly in polys)
            {
                drawing = drawing.Remove(poly);
            }

            var result = Combine(polys, drawing.Settings);
            foreach (var line in result)
            {
                drawing = drawing.AddToCurrentLayer(line);
            }

            workspace.Update(drawing: drawing);
            return true;
        }
    }
}
