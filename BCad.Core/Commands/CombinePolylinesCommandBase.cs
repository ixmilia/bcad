using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;

namespace BCad.Commands
{
    public abstract class CombinePolylinesCommandBase : ICadCommand
    {
        protected abstract IEnumerable<IPrimitive> Combine(IEnumerable<Polyline> polylines);

        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var inputService = workspace.InputService;
            var entities = await inputService.GetEntities("Select polylines");
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            var polys = entities.Value.OfType<Polyline>();
            if (polys.Count() <= 1)
            {
                return false;
            }

            var drawing = workspace.Drawing;
            foreach (var poly in polys)
            {
                drawing = drawing.Remove(poly);
            }

            var result = Combine(polys);
            foreach (var line in result)
            {
                drawing = drawing.AddToCurrentLayer(line.ToEntity());
            }

            workspace.Update(drawing: drawing);
            return true;
        }
    }
}
