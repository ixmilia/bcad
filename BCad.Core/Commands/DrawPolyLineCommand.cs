using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.PolyLine", "POLYLINE", "polyline", "pl")]
    public class DrawPolyLineCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var points = new List<Point>();
            var segments = new List<PrimitiveLine>();

            var input = await workspace.InputService.GetPoint(new UserDirective("Start"));
            if (input.Cancel) return false;
            if (!input.HasValue) return true;
            var first = input.Value;
            points.Add(first);
            Point last = first;
            while (true)
            {
                var current = await workspace.InputService.GetPoint(new UserDirective("Next or [c]lose", "c"), (p) =>
                {
                    return segments.Concat(new[] { new PrimitiveLine(last, p) });
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    segments.Add(new PrimitiveLine(last, current.Value));
                    points.Add(current.Value);
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else if (current.Directive == "c")
                {
                    if (last != first)
                    {
                        points.Add(first);
                    }
                    break;
                }
                else
                {
                    break;
                }
            }

            var polyline = new Polyline(points, null);
            workspace.AddToCurrentLayer(polyline);
            return true;
        }
    }
}
