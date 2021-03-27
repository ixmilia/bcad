using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Primitives;

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
                var current = await workspace.InputService.GetPoint(new UserDirective("Next or [c]lose", "c"), (p) =>
                {
                    return new[] { new PrimitiveLine(last, p) };
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    workspace.AddToCurrentLayer(new Line(last, current.Value));
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else if (current.Directive == "c")
                {
                    if (last != first)
                    {
                        workspace.AddToCurrentLayer(new Line(last, first));
                    }
                    break;
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
