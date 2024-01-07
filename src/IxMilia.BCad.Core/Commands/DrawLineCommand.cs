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
            var input = await workspace.InputService.GetPoint(new UserDirective("From or [t]angent", "t"));
            if (input.Cancel) return false;

            Point first, last;
            if (input.Directive == "t")
            {
                var tangentEntity = await workspace.InputService.GetEntity(new UserDirective("Select circle for tangent line"));
                if (tangentEntity.Cancel) return false;
                if (!tangentEntity.HasValue) return false;
                if (tangentEntity.Value.Entity is not Circle tangentCircle) return false;
                var primitiveEllipse = tangentCircle.GetPrimitives(workspace.Drawing.Settings).Single() as PrimitiveEllipse;
                if (primitiveEllipse is null) return false;

                var nextTangentSelection = await workspace.InputService.GetPoint(new UserDirective("Next point or [t]angent circle", "t"));
                if (nextTangentSelection.Cancel) return false;
                if (nextTangentSelection.HasValue)
                {
                    // line from circle tangent to point
                    var candidateTangentLines = EditUtilities.TangentLine(nextTangentSelection.Value, primitiveEllipse);
                    if (candidateTangentLines is null) return false;
                    var dist1 = (candidateTangentLines.Value.Item1.P2 - tangentEntity.Value.SelectionPoint).LengthSquared;
                    var dist2 = (candidateTangentLines.Value.Item2.P2 - tangentEntity.Value.SelectionPoint).LengthSquared;
                    var newPrimitiveLine = dist1 < dist2 ? candidateTangentLines.Value.Item1 : candidateTangentLines.Value.Item2;

                    // need to add it backwards since we selected it backwards
                    workspace.AddToCurrentLayer(new Line(newPrimitiveLine.P2, newPrimitiveLine.P1, lineTypeSpecification: workspace.Drawing.Settings.CurrentLineTypeSpecification));
                    first = newPrimitiveLine.P2;
                    last = newPrimitiveLine.P1;
                }
                else if (nextTangentSelection.Directive == "t")
                {
                    // line from circle tangent to circle tangent
                    var secondTangentEntity = await workspace.InputService.GetEntity(new UserDirective("Next circle for tangent line"));
                    if (secondTangentEntity.Cancel) return false;
                    if (!secondTangentEntity.HasValue) return false;
                    if (secondTangentEntity.Value.Entity is not Circle secondCircle) return false;
                    var secondPrimitiveEllipse = secondCircle.GetPrimitives(workspace.Drawing.Settings).Single() as PrimitiveEllipse;
                    if (secondPrimitiveEllipse is null) return false;
                    var candidateTangentLines = EditUtilities.TangentLine(primitiveEllipse, secondPrimitiveEllipse);
                    if (candidateTangentLines is null) return false;
                    var dist1 = (candidateTangentLines.Value.Item1.P1 - tangentEntity.Value.SelectionPoint).LengthSquared;
                    var dist2 = (candidateTangentLines.Value.Item2.P1 - tangentEntity.Value.SelectionPoint).LengthSquared;
                    var newPrimitiveLine = dist1 < dist2 ? candidateTangentLines.Value.Item1 : candidateTangentLines.Value.Item2;
                    workspace.AddToCurrentLayer(new Line(newPrimitiveLine.P1, newPrimitiveLine.P2, lineTypeSpecification: workspace.Drawing.Settings.CurrentLineTypeSpecification));
                    first = newPrimitiveLine.P1;
                    last = newPrimitiveLine.P2;
                }
                else
                {
                    return false;
                }
            }
            else if (input.HasValue)
            {
                first = input.Value;
                last = first;
            }
            else
            {
                return true;
            }

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
