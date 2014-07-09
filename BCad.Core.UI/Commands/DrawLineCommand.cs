using System.Composition;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportUICommand("Draw.Line", "LINE", "line", "l")]
    public class DrawLineCommand : IUICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var input = await InputService.GetPoint(new UserDirective("From"));
            if (input.Cancel) return false;
            if (!input.HasValue) return true;
            var first = input.Value;
            Point last = first;
            while (true)
            {
                var current = await InputService.GetPoint(new UserDirective("Next or [c]lose", "c"), (p) =>
                {
                    return new[] { new PrimitiveLine(last, p, IndexedColor.Default) };
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    Workspace.AddToCurrentLayer(new Line(last, current.Value, IndexedColor.Default));
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else if (current.Directive == "c")
                {
                    if (last != first)
                    {
                        Workspace.AddToCurrentLayer(new Line(last, first, IndexedColor.Default));
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
