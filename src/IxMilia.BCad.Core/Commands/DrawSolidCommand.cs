using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    public class DrawSolidCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var firstInput = await workspace.InputService.GetPoint(new UserDirective("First point"));
            if (!firstInput.HasValue || firstInput.Cancel)
            {
                return false;
            }

            var first = firstInput.Value;
            var secondInput = await workspace.InputService.GetPoint(new UserDirective("Second point"), (second) =>
            {
                return new IPrimitive[]
                {
                    new PrimitiveLine(first, second),
                };
            });
            if (!secondInput.HasValue || secondInput.Cancel)
            {
                return false;
            }

            var second = secondInput.Value;

            var thirdInput = await workspace.InputService.GetPoint(new UserDirective("Third point"), (third) =>
            {
                return new IPrimitive[]
                {
                    new PrimitiveTriangle(first, second, third),
                };
            });
            if (!thirdInput.HasValue || thirdInput.Cancel)
            {
                return false;
            }

            var third = thirdInput.Value;
            var fourthInput = await workspace.InputService.GetPoint(new UserDirective("Fourth point"), (fourth) =>
            {
                return new IPrimitive[]
                {
                    new PrimitiveTriangle(first, second, third),
                    new PrimitiveTriangle(third, fourth, first),
                };
            });
            if (!fourthInput.HasValue || fourthInput.Cancel)
            {
                return false;
            }

            var fourth = fourthInput.Value;
            workspace.Update(drawing: workspace.Drawing.AddToCurrentLayer(new Solid(first, second, third, fourth)));
            return true;
        }
    }
}
