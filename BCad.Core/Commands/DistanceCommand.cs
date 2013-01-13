using System;
using System.ComponentModel.Composition;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("View.Distance", "distance", "di", "dist")]
    public class DistanceCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var start = InputService.GetPoint(new UserDirective("Distance from"));
            if (start.Cancel || !start.HasValue) return false;
            var first = start.Value;
            var end = InputService.GetPoint(new UserDirective("Distance to"), (p) =>
                {
                    return new[] { new PrimitiveLine(first, p, Color.Default) };
                });
            if (end.Cancel || !end.HasValue) return false;
            var between = end.Value - first;
            InputService.WriteLine("Distance: {0} ( dx: {1}, dy: {2}, dz: {3} )",
                Workspace.FormatUnits(between.Length),
                Workspace.FormatUnits(Math.Abs(between.X)),
                Workspace.FormatUnits(Math.Abs(between.Y)),
                Workspace.FormatUnits(Math.Abs(between.Z)));

            return true;
        }

        public string DisplayName
        {
            get { return "DIST"; }
        }
    }
}
