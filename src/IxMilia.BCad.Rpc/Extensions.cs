using System;
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
            switch (colorType)
            {
                case PlotColorType.Exact:
                    return drawing;
                case PlotColorType.Contrast:
                    return drawing.Map(
                        l => l.Color.IsAutoOrWhite() ? l.Update(color: CadColor.Black) : l,
                        e => e.Color.IsAutoOrWhite() ? e.WithColor(color: CadColor.Black) : e);
                case PlotColorType.Black:
                    return drawing.Map(
                        l => l.Update(color: CadColor.Black),
                        e => e.WithColor(color: CadColor.Black));
                default:
                    throw new InvalidOperationException($"Color type {colorType} not recognized");
            }
        }
    }
}
