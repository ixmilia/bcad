using System.Linq;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Extensions;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Subtract", "SUBTRACT", "subtract", "sub")]
    public class SubtractCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var inputService = workspace.InputService;

            // get the first entity
            var original = await inputService.GetEntity(new UserDirective("Select first entity"));
            if (original.Cancel || !original.HasValue)
            {
                return false;
            }

            if (original.Value.Entity.Kind != EntityKind.Polyline)
            {
                workspace.OutputService.WriteLine("You must select a polyline");
                return false;
            }

            // get the other entities
            var others = await inputService.GetEntities("Select other polylines");
            if (others.Cancel || !others.HasValue)
            {
                return false;
            }

            var polys = others.Value.OfType<Polyline>();
            if (polys.Count() == 0)
            {
                return false;
            }

            var drawing = workspace.Drawing;

            // remove the old entities
            drawing = drawing.Remove(original.Value.Entity);
            foreach (var other in others.Value)
            {
                drawing = drawing.Remove(other);
            }

            // perform the subtraction and add the new entities
            var result = ((Polyline)original.Value.Entity).Subtract(polys);
            foreach (var poly in result)
            {
                drawing = drawing.AddToCurrentLayer(poly);
            }

            workspace.Update(drawing: drawing);
            return true;
        }
    }
}
