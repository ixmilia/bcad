using System;
using System.Threading.Tasks;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("View.Distance", "DIST", "distance", "di", "dist")]
    public class DistanceCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var inputService = workspace.GetService<IInputService>();
            var outputService = workspace.GetService<IOutputService>();
            var start = await inputService.GetPoint(new UserDirective("Distance from"));
            if (start.Cancel || !start.HasValue) return false;
            var first = start.Value;
            var end = await inputService.GetPoint(new UserDirective("Distance to"), (p) =>
                {
                    return new[] { new PrimitiveLine(first, p, null) };
                });
            if (end.Cancel || !end.HasValue) return false;
            var between = end.Value - first;
            var settings = workspace.Drawing.Settings;
            outputService.WriteLine("Distance: {0} ( dx: {1}, dy: {2}, dz: {3} )",
                Format(settings, between.Length),
                Format(settings, Math.Abs(between.X)),
                Format(settings, Math.Abs(between.Y)),
                Format(settings, Math.Abs(between.Z)));

            return true;
        }

        private string Format(DrawingSettings settings, double value)
        {
            return DrawingSettings.FormatUnits(value, settings.UnitFormat, settings.UnitPrecision);
        }
    }
}
