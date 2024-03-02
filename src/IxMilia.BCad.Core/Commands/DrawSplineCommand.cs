using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    public class DrawSplineCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var p1Result = await workspace.InputService.GetPoint(new UserDirective("First point"));
            if (p1Result.Cancel || !p1Result.HasValue)
            {
                return false;
            }

            var p1 = p1Result.Value;
            var p2Result = await workspace.InputService.GetPoint(new UserDirective("Second point"), p =>
            {
                return new[] { new PrimitiveLine(p1, p) };
            });
            if (p2Result.Cancel || !p2Result.HasValue)
            {
                return false;
            }

            var p2 = p2Result.Value;
            var p3Result = await workspace.InputService.GetPoint(new UserDirective("Third point"), p =>
            {
                return new[]
                {
                    new PrimitiveLine(p1, p2),
                    new PrimitiveLine(p2, p)
                };
            });
            if (p3Result.Cancel || !p3Result.HasValue)
            {
                return false;
            }

            var p3 = p3Result.Value;
            var p4Result = await workspace.InputService.GetPoint(new UserDirective("Fourth point"), p =>
            {
                return new IPrimitive[]
                {
                    new PrimitiveLine(p1, p2),
                    new PrimitiveLine(p2, p3),
                    new PrimitiveLine(p3, p),
                    new PrimitiveBezier(p1, p2, p3, p)
                };
            });
            if (p4Result.Cancel || !p4Result.HasValue)
            {
                return false;
            }

            var p4 = p4Result.Value;
            var bezier = Spline.FromBezier(new PrimitiveBezier(p1, p2, p3, p4), workspace.Drawing.Settings.CurrentLineTypeSpecification);
            workspace.AddToCurrentLayer(bezier);
            return true;
        }
    }
}
