using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Union", "UNION", "union", "un")]
    public class UnionCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var inputService = workspace.InputService;
            var allEntities = new List<Polyline>();
            while (true)
            {
                var entities = await inputService.GetEntities("Select polylines");
                if (entities.Cancel || !entities.HasValue || entities.Value.Count() == 0)
                {
                    break;
                }

                allEntities.AddRange(entities.Value.OfType<Polyline>());
            }

            if (allEntities.Count == 2)
            {
                // TODO: handle multiples
                var first = allEntities[0];
                var second = allEntities[1];
                var union = first.Union(second);
                var drawing = workspace.Drawing.Remove(first).Remove(second);
                foreach (var part in union.Cast<PrimitiveLine>())
                {
                    drawing = drawing.AddToCurrentLayer(part.ToEntity());
                }

                workspace.Update(drawing: drawing);
                return true;
            }
            else
            {
                workspace.OutputService.WriteLine("Only 2 polylines currently supported.");
                return false;
            }
        }
    }
}
