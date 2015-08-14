using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Arc", "ARC", "arc", "a")]
    internal class DrawArcCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var inputService = workspace.GetService<IInputService>();
            var first = await inputService.GetPoint(new UserDirective("First point"));
            if (!first.Cancel && first.HasValue)
            {
                var second = await inputService.GetPoint(new UserDirective("Second point"), (p) =>
                    {
                        return new[]
                        {
                            new PrimitiveLine(first.Value, p)
                        };
                    });
                if (!second.Cancel && second.HasValue)
                {
                    var third = await inputService.GetPoint(new UserDirective("Third point"), (p) =>
                        {
                            var a = PrimitiveEllipse.ThreePointArc(first.Value, second.Value, p, workspace.DrawingPlane.Normal);
                            if (a == null)
                            {
                                return new IPrimitive[0];
                            }
                            else
                            {
                                return new IPrimitive[]
                                {
                                    a,
                                    new PrimitivePoint(first.Value, null),
                                    new PrimitivePoint(second.Value, null)
                                };
                            }
                        });
                    if (!third.Cancel && third.HasValue)
                    {
                        var primitiveArc = PrimitiveEllipse.ThreePointArc(first.Value, second.Value, third.Value, workspace.DrawingPlane.Normal);
                        if (primitiveArc != null)
                        {
                            var arc = new Arc(
                                primitiveArc.Center,
                                primitiveArc.MajorAxis.Length,
                                primitiveArc.StartAngle,
                                primitiveArc.EndAngle,
                                primitiveArc.Normal,
                                null);
                            workspace.AddToCurrentLayer(arc);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
