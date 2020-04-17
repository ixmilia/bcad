using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Draw.Rectangle", "RECTANGLE", "rectangle", "rect")]
    public class DrawRectangleCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var inputService = workspace.InputService;
            var firstCorner = await inputService.GetPoint(new UserDirective("Select first corner"));
            if (firstCorner.Cancel || !firstCorner.HasValue)
            {
                return false;
            }

            var secondCorner = await inputService.GetPoint(new UserDirective("Select second corner"), p => GetRectangleFromPoints(firstCorner.Value, p));
            if (secondCorner.Cancel || !secondCorner.HasValue)
            {
                return false;
            }

            var lines = GetRectangleFromPoints(firstCorner.Value, secondCorner.Value);
            var poly = lines.GetPolylinesFromSegments().Single();
            workspace.Update(drawing: workspace.Drawing.AddToCurrentLayer(poly));
            return true;
        }

        private static IEnumerable<PrimitiveLine> GetRectangleFromPoints(Point p1, Point p2)
        {
            // TODO: use workspace.DrawingPlane, but for now assume everything is on the plane z = 0
            var other1 = new Point(p1.X, p2.Y, 0.0);
            var other2 = new Point(p2.X, p1.Y, 0.0);
            yield return new PrimitiveLine(p1, other1);
            yield return new PrimitiveLine(other1, p2);
            yield return new PrimitiveLine(p2, other2);
            yield return new PrimitiveLine(other2, p1);
        }
    }
}
