using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
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

        public async Task<bool> Execute(object arg)
        {
            var start = await InputService.GetPoint(new UserDirective("Distance from"));
            if (start.Cancel || !start.HasValue) return false;
            var first = start.Value;
            var end = await InputService.GetPoint(new UserDirective("Distance to"), (p) =>
                {
                    return new[] { new PrimitiveLine(first, p, Color.Default) };
                });
            if (end.Cancel || !end.HasValue) return false;
            var between = end.Value - first;
            var settings = Workspace.Drawing.Settings;
            InputService.WriteLine("Distance: {0} ( dx: {1}, dy: {2}, dz: {3} )",
                Format(between.Length),
                Format(Math.Abs(between.X)),
                Format(Math.Abs(between.Y)),
                Format(Math.Abs(between.Z)));

            return true;
        }

        private string Format(double value)
        {
            var settings = Workspace.Drawing.Settings;
            return DrawingSettings.FormatUnits(value, settings.UnitFormat, settings.UnitPrecision);
        }

        public string DisplayName
        {
            get { return "DIST"; }
        }
    }
}
