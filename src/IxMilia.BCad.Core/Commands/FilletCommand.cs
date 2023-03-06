using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    public class FilletCommand : ICadCommand
    {
        private static double LastFilletDistance = 0.0;

        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var entity1 = await workspace.InputService.GetEntity(new UserDirective("Select first entity"));
            if (!entity1.HasValue || entity1.Cancel)
            {
                return false;
            }

            var filletDistance = LastFilletDistance;
            SelectedEntity secondEntity = null;
            while (secondEntity == null)
            {
                var entity2 = await workspace.InputService.GetEntity(new UserDirective($"Select second entity, or [r]adius [{filletDistance}]", "r"));
                if (entity2.Cancel)
                {
                    return false;
                }

                if (entity2.HasValue)
                {
                    secondEntity = entity2.Value;
                }
                else if (entity2.Directive == "r")
                {
                    var radius = await workspace.InputService.GetDistance(new UserDirective($"Fillet radius [{filletDistance}]"), defaultDistance: filletDistance);
                    if (!radius.HasValue || radius.Cancel)
                    {
                        return false;
                    }

                    filletDistance = radius.Value;
                }
            }

            var primitive1 = entity1.Value.Entity.GetPrimitives().First() as PrimitiveLine;
            var primitive2 = secondEntity.Entity.GetPrimitives().First() as PrimitiveLine;
            if (primitive1 is null || primitive2 is null)
            {
                workspace.OutputService.WriteLine("Expected only lines");
                return false;
            }

            var filletOptions = new FilletOptions(workspace.DrawingPlane, primitive1, entity1.Value.SelectionPoint, primitive2, secondEntity.SelectionPoint, filletDistance);
            var success = FilletUtility.TryFillet(filletOptions, out var filletResult);
            if (!success)
            {
                workspace.OutputService.WriteLine("Unable to perform fillet");
                return false;
            }

            var newEntity1 = filletResult.UpdatedLine1.ToEntity(entity1.Value.Entity.LineTypeSpecification);
            var newEntity2 = filletResult.UpdatedLine2.ToEntity(secondEntity.Entity.LineTypeSpecification);

            var drawing = workspace.Drawing;
            drawing = drawing.Replace(entity1.Value.Entity, newEntity1);
            drawing = drawing.Replace(secondEntity.Entity, newEntity2);
            if (filletResult.Fillet != null)
            {
                var newArc = filletResult.Fillet.ToEntity();
                drawing = drawing.AddToCurrentLayer(newArc);
            }

            workspace.Update(drawing: drawing);
            LastFilletDistance = filletDistance;
            return true;
        }
    }
}
