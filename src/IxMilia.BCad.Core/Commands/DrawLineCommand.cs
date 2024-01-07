using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    public class DrawLineCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var input = await workspace.InputService.GetPoint(new UserDirective("From"));
            if (input.Cancel) return false;
            if (!input.HasValue) return true;
            var first = input.Value;
            Point last = first;
            while (true)
            {
                var current = await workspace.InputService.GetPoint(new UserDirective("Next point, [t]angent, or [c]lose", "c", "t"), (p) =>
                {
                    return new[] { new PrimitiveLine(last, p) };
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    workspace.AddToCurrentLayer(new Line(last, current.Value, lineTypeSpecification: workspace.Drawing.Settings.CurrentLineTypeSpecification));
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else if (current.Directive == "c")
                {
                    if (last != first)
                    {
                        workspace.AddToCurrentLayer(new Line(last, first, lineTypeSpecification: workspace.Drawing.Settings.CurrentLineTypeSpecification));
                    }
                    break;
                }
                else if (current.Directive == "t")
                {
                    var selectedEntity = await workspace.InputService.GetEntity(new UserDirective("Select circle for tangent line"), (p) =>
                    {
                        return new[] { new PrimitiveLine(last, p) };
                    });
                    if (selectedEntity.Cancel) break;
                    if (!selectedEntity.HasValue) break;
                    if (selectedEntity.Value.Entity is not Circle circle) break;
                    var primitiveEllipse = circle.GetPrimitives(workspace.Drawing.Settings).Single() as PrimitiveEllipse;
                    if (primitiveEllipse is null) break;
                    var candidateTangentLines = EditUtilities.TangentLine(last, primitiveEllipse);
                    if (candidateTangentLines is null) break;
                    var dist1 = (candidateTangentLines.Value.Item1.P2 - selectedEntity.Value.SelectionPoint).LengthSquared;
                    var dist2 = (candidateTangentLines.Value.Item2.P2 - selectedEntity.Value.SelectionPoint).LengthSquared;
                    var newPrimitiveLine = dist1 < dist2 ? candidateTangentLines.Value.Item1 : candidateTangentLines.Value.Item2;
                    workspace.AddToCurrentLayer(new Line(newPrimitiveLine.P1, newPrimitiveLine.P2, lineTypeSpecification: workspace.Drawing.Settings.CurrentLineTypeSpecification));
                    last = newPrimitiveLine.P2;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }
    }
}
