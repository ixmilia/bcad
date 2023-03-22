using System;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.Rpc
{
    public static class Extensions
    {
        public static bool IsWhite(this CadColor color)
        {
            return color == CadColor.White;
        }

        public static bool IsAutoOrWhite(this CadColor? color)
        {
            return color == null || color == CadColor.White;
        }

        public static Drawing UpdateColors(this Drawing drawing, PlotColorType colorType)
        {
            return (colorType switch
            {
                PlotColorType.Exact => drawing,
                _ => drawing.Map(
                    l => l.Update(color: l.Color.UpdateColor(colorType)),
                    e => e.UpdateColors(colorType)),
            }).Update(settings: drawing.Settings.UpdateColors(colorType));
        }

        private static Entity UpdateColors(this Entity entity, PlotColorType colorType)
        {
            return entity switch
            {
                AbstractDimension dim => dim.WithTextColor(dim.TextColor.UpdateColor(colorType)).WithColor(dim.Color.UpdateColor(colorType)),
                _ => entity.WithColor(entity.Color.UpdateColor(colorType)),
            };
        }

        private static DrawingSettings UpdateColors(this DrawingSettings settings, PlotColorType colorType)
        {
            return colorType switch
            {
                PlotColorType.Exact => settings,
                _ => settings.Update(dimStyles: DimensionStyleCollection.FromEnumerable(settings.DimensionStyles.Select(ds => ds.UpdateColors(colorType)))),
            };
        }

        private static DimensionStyle UpdateColors(this DimensionStyle dimStyle, PlotColorType colorType)
        {
            return dimStyle.Update(
                lineColor: dimStyle.LineColor.UpdateColor(colorType),
                textColor: dimStyle.TextColor.UpdateColor(colorType));
        }

        private static CadColor? UpdateColor(this CadColor? color, PlotColorType colorType)
        {
            return colorType switch
            {
                PlotColorType.Exact => color,
                PlotColorType.Contrast => color.IsAutoOrWhite() ? CadColor.Black : color,
                PlotColorType.Black => CadColor.Black,
                _ => throw new ArgumentOutOfRangeException(nameof(colorType))
            };
        }
    }
}
