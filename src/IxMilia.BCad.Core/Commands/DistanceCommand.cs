using System;
using System.Threading.Tasks;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    public class DistanceCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var start = await workspace.InputService.GetPoint(new UserDirective("Distance from"));
            if (start.Cancel || !start.HasValue) return false;
            var first = start.Value;
            var end = await workspace.InputService.GetPoint(new UserDirective("Distance to"), (p) =>
                {
                    return new[] { new PrimitiveLine(first, p) };
                });
            if (end.Cancel || !end.HasValue) return false;
            var between = end.Value - first;
            var settings = workspace.Drawing.Settings;
            workspace.OutputService.WriteLine("Distance: {0} ( dx: {1}, dy: {2}, dz: {3} )",
                Format(settings, between.Length),
                Format(settings, Math.Abs(between.X)),
                Format(settings, Math.Abs(between.Y)),
                Format(settings, Math.Abs(between.Z)));

            return true;
        }

        private string Format(DrawingSettings settings, double value)
        {
            return DrawingSettings.FormatUnits(value, settings.DrawingUnits, settings.UnitFormat, settings.UnitPrecision);
        }
    }
}
