using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Draw.Line", "LINE", "line", "l")]
    public class DrawLineCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var inputService = workspace.GetService<IInputService>();
            var input = await inputService.GetPoint(new UserDirective("From"));
            if (input.Cancel) return false;
            if (!input.HasValue) return true;
            var first = input.Value;
            Point last = first;
            while (true)
            {
                var current = await inputService.GetPoint(new UserDirective("Next or [c]lose", "c"), (p) =>
                {
                    return new[] { new PrimitiveLine(last, p, null) };
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    workspace.AddToCurrentLayer(new Line(last, current.Value, null));
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else if (current.Directive == "c")
                {
                    if (last != first)
                    {
                        workspace.AddToCurrentLayer(new Line(last, first, null));
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
