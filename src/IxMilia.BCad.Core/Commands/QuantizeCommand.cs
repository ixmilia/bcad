using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    public class QuantizeCommand : ICadCommand
    {
        private static double LastDistanceQuantum = 0.0;
        private static double LastAngleQuantum = 0.0;

        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            // get entities
            var entities = await workspace.InputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            if (!entities.Value.Any())
            {
                return true;
            }

            // get distance quantum
            var distance = await workspace.InputService.GetDistance(prompt: "Distance quantum", defaultDistance: LastDistanceQuantum);
            if (distance.Cancel)
            {
                return false;
            }

            double distanceQuantum;
            if (distance.HasValue)
            {
                distanceQuantum = distance.Value;
            }
            else
            {
                distanceQuantum = LastDistanceQuantum;
            }

            LastDistanceQuantum = distanceQuantum;

            // get angle quantum
            var angle = await workspace.InputService.GetDistance(prompt: "Angle quantum", defaultDistance: LastAngleQuantum);
            if (angle.Cancel)
            {
                return false;
            }

            double angleQuantum;
            if (angle.HasValue)
            {
                angleQuantum = angle.Value;
            }
            else
            {
                angleQuantum = LastAngleQuantum;
            }

            LastAngleQuantum = angleQuantum;

            // do it
            var settings = new QuantizeSettings(distanceQuantum, angleQuantum);
            var drawing = workspace.Drawing;
            foreach (var entity in entities.Value)
            {
                var quantized = EditUtilities.Quantize(entity, settings);
                var layer = drawing.ContainingLayer(entity);
                var originalLayer = layer;
                layer = layer.Remove(entity);
                layer = layer.Add(quantized);
                drawing = drawing.Remove(originalLayer);
                drawing = drawing.Add(layer);
            }

            workspace.Update(drawing: drawing);

            return true;
        }
    }
}
