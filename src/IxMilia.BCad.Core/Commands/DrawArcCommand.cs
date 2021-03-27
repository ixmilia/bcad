using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    internal class DrawArcCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var first = await workspace.InputService.GetPoint(new UserDirective("First point"));
            if (!first.Cancel && first.HasValue)
            {
                var second = await workspace.InputService.GetPoint(new UserDirective("Second point"), (p) =>
                    {
                        return new[]
                        {
                            new PrimitiveLine(first.Value, p)
                        };
                    });
                if (!second.Cancel && second.HasValue)
                {
                    var third = await workspace.InputService.GetPoint(new UserDirective("Third point"), (p) =>
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
                                    new PrimitivePoint(first.Value),
                                    new PrimitivePoint(second.Value)
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
                                primitiveArc.Normal);
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
