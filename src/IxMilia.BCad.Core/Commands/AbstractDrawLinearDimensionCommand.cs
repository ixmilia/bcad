using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;
using IxMilia.Converters;

namespace IxMilia.BCad.Commands
{
    public abstract class AbstractDrawLinearDimensionCommand : ICadCommand
    {
        public abstract bool DrawAligned { get; }

        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var firstPoint = await workspace.InputService.GetPoint(new UserDirective("First dimension point"));
            if (!firstPoint.HasValue || firstPoint.Cancel)
            {
                return false;
            }

            var first = firstPoint.Value;
            var secondPoint = await workspace.InputService.GetPoint(new UserDirective("Second dimension point"), currentPoint =>
            {
                return new IPrimitive[]
                {
                    new PrimitiveLine(first, currentPoint)
                };
            }, first);
            if (!secondPoint.HasValue || secondPoint.Cancel)
            {
                return false;
            }

            var second = secondPoint.Value;
            var dimensionSettings = workspace.Drawing.Settings.CurrentDimensionStyle.ToDimensionSettings();
            var dimensionLineLocation = await workspace.InputService.GetPoint(new UserDirective("Dimension location"), currentPoint =>
            {
                var dimensionProperties = LinearDimensionProperties.BuildFromValues(
                    first.ToConverterVector(),
                    second.ToConverterVector(),
                    currentPoint.ToConverterVector(),
                    DrawAligned,
                    workspace.Drawing.Settings.DrawingUnits.ToConverterDrawingUnits(),
                    workspace.Drawing.Settings.UnitFormat.ToConverterUnitFormat(),
                    workspace.Drawing.Settings.UnitPrecision,
                    dimensionSettings,
                    text => dimensionSettings.TextHeight * text.Length * 0.6 // this is really bad
                );
                var textHeight = workspace.Drawing.Settings.CurrentDimensionStyle.TextHeight;
                var primitives = LinearDimension.GetPrimitives(
                    dimensionProperties,
                    textHeight,
                    workspace.Drawing.Settings.CurrentDimensionStyle.LineColor,
                    workspace.Drawing.Settings.CurrentDimensionStyle.TextColor,
                    workspace.Drawing.Settings.CurrentDimensionStyle);
                return primitives;
            });
            if (!dimensionLineLocation.HasValue || dimensionLineLocation.Cancel)
            {
                return false;
            }

            var dimensionProperties = LinearDimensionProperties.BuildFromValues(
                first.ToConverterVector(),
                second.ToConverterVector(),
                dimensionLineLocation.Value.ToConverterVector(),
                DrawAligned,
                workspace.Drawing.Settings.DrawingUnits.ToConverterDrawingUnits(),
                workspace.Drawing.Settings.UnitFormat.ToConverterUnitFormat(),
                workspace.Drawing.Settings.UnitPrecision,
                dimensionSettings,
                text => dimensionSettings.TextHeight * text.Length * 0.6 // this is really bad
            );
            var textMidPoint = (dimensionProperties.DimensionLineStart + dimensionProperties.DimensionLineEnd).ToPoint() / 2.0;
            var dimension = new LinearDimension(
                first,
                second,
                dimensionProperties.DimensionLineStart.ToPoint(),
                DrawAligned,
                textMidPoint,
                workspace.Drawing.Settings.CurrentDimensionStyleName);
            workspace.Update(drawing: workspace.Drawing.AddToCurrentLayer(dimension));
            return true;
        }
    }
}
