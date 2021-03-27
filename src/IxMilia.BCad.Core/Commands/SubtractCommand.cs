using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.Commands
{
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

            if (original.Value.Entity.Kind != EntityKind.Circle && original.Value.Entity.Kind != EntityKind.Polyline)
            {
                workspace.OutputService.WriteLine("You must select a circle or polyline");
                return false;
            }

            // get the other entities
            var others = await inputService.GetEntities("Select other polylines", entityKinds: EntityKind.Circle | EntityKind.Polyline);
            if (others.Cancel || !others.HasValue)
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
            var result = original.Value.Entity.Subtract(others.Value);
            foreach (var poly in result)
            {
                drawing = drawing.AddToCurrentLayer(poly);
            }

            workspace.Update(drawing: drawing);
            return true;
        }
    }
}
