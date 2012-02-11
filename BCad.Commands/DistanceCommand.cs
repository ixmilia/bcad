using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.Commands
{
    [ExportCommand("View.Distance", "distance", "di", "dist")]
    internal class DistanceCommand : ICommand
    {
        [Import]
        public IUserConsole UserConsole { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public bool Execute(params object[] parameters)
        {
            var start = UserConsole.GetPoint(new UserDirective("Distance from"));
            if (start.Cancel) return false;
            var first = start.Value;
            var end = UserConsole.GetPoint(new UserDirective("Distance to"), (p) =>
                {
                    return new[] { new Line(first, p, Color.Default) };
                });
            if (end.Cancel) return false;
            var between = end.Value - first;
            UserConsole.WriteLine("Distance: {0} ( dx: {1}, dy: {2}, dz: {3} )",
                between.Length, Math.Abs(between.X), Math.Abs(between.Y), Math.Abs(between.Z));
            return true;
        }

        public string DisplayName
        {
            get { return "DIST"; }
        }
    }
}
