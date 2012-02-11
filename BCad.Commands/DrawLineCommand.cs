using System.ComponentModel.Composition;
using BCad.Objects;

namespace BCad.Commands
{
    [ExportCommand("Draw.Line", "line", "l")]
    internal class DrawLineCommand : ICommand
    {
        [Import]
        public IUserConsole UserConsole { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public bool Execute(params object[] parameters)
        {
            var input = UserConsole.GetPoint(new UserDirective("From"));
            if (input.Cancel) return false;
            var first = input.Value;
            Point last = first;
            while (true)
            {
                var current = UserConsole.GetPoint(new UserDirective("Next or [c]lose", "c"), (p) =>
                {
                    return new[] { new Line(last, p, Color.Default) };
                });
                if (current.Cancel) break;
                if (current.HasValue)
                {
                    Workspace.Document.AddObject(new Line(last, current.Value, Color.Default, Workspace.Document.CurrentLayer));
                    last = current.Value;
                    if (last == first) break; // closed
                }
                else
                {
                    if (current.Directive == "c")
                    {
                        if (last != first)
                            Workspace.Document.AddObject(new Line(last, first, Color.Default, Workspace.Document.CurrentLayer));
                        break;
                    }
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
