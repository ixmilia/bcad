using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    public class ScaleCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            // get all inputs
            var selection = await workspace.InputService.GetEntities("Select entities");
            if (selection.Cancel || !selection.HasValue)
            {
                return false;
            }

            var entities = selection.Value;

            var basePointValue = await workspace.InputService.GetPoint(new UserDirective("Select base point"));
            if (basePointValue.Cancel || !basePointValue.HasValue)
            {
                return false;
            }

            var basePoint = basePointValue.Value;

            var scaleFactorValue = await workspace.InputService.GetDistance(new UserDirective("Scale factor or [r]eference", "r"));
            if (scaleFactorValue.Cancel || (!scaleFactorValue.HasValue && string.IsNullOrEmpty(scaleFactorValue.Directive)))
            {
                return false;
            }

            double scaleFactor;
            if (scaleFactorValue.Directive == "r")
            {
                var firstDistanceValue = await workspace.InputService.GetDistance(new UserDirective("First scale distance"));
                if (firstDistanceValue.Cancel ||
                    !firstDistanceValue.HasValue ||
                    firstDistanceValue.Value == 0.0)
                {
                    return false;
                }

                var entityPrimitives = entities.SelectMany(e => e.GetPrimitives(workspace.Drawing.Settings)).ToArray();
                var secondDistanceValue = await workspace.InputService.GetDistance(
                    new UserDirective("Second scale distance"),
                    onCursorMove: distance =>
                    {
                        var scaleFactor = distance / firstDistanceValue.Value;
                        return entityPrimitives.Select(p => EditUtilities.Scale(p, basePoint, scaleFactor));
                    });
                if (secondDistanceValue.Cancel ||
                    !secondDistanceValue.HasValue ||
                    secondDistanceValue.Value == 0.0)
                {
                    return false;
                }

                scaleFactor = secondDistanceValue.Value / firstDistanceValue.Value;
            }
            else
            {
                scaleFactor = scaleFactorValue.Value;
            }

            // now do it
            var drawing = workspace.Drawing.ScaleEntities(entities, basePoint, scaleFactor);
            workspace.Update(drawing: drawing);
            return true;
        }
    }
}
