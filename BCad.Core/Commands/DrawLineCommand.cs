using System.ComponentModel.Composition;
using BCad.Entities;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Draw.Line", "line", "l")]
    public class DrawLineCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var input = InputService.GetPoint(new UserDirective("From"));
            if (input.Cancel) return false;
            if (!input.HasValue) return true;
            var first = input.Value;
            Point last = first;
            while (true)
            {
                var current = InputService.GetPoint(new UserDirective("Next or [c]lose", "c"), (p) =>
                {
                    return new[] { new PrimitiveLine(last, p, Color.Default) };
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    Workspace.AddToCurrentLayer(new Line(last, current.Value, Color.Default));
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else if (current.Directive == "c")
                {
                    if (last != first)
                    {
                        Workspace.AddToCurrentLayer(new Line(last, first, Color.Default));
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

        public string DisplayName
        {
            get { return "LINE"; }
        }
    }
}
