using System;
using C = IxMilia.Converters;

namespace IxMilia.BCad.Extensions
{
    public static class UnitExtensions
    {
        public static C.DrawingUnits ToConverterDrawingUnits(this DrawingUnits drawingUnits)
        {
            return drawingUnits switch
            {
                DrawingUnits.English => C.DrawingUnits.English,
                DrawingUnits.Metric => C.DrawingUnits.Metric,
                _ => throw new ArgumentOutOfRangeException(nameof(drawingUnits)),
            };
        }

        public static C.UnitFormat ToConverterUnitFormat(this UnitFormat unitFormat)
        {
            return unitFormat switch
            {
                UnitFormat.Architectural => C.UnitFormat.Architectural,
                UnitFormat.Decimal => C.UnitFormat.Decimal,
                UnitFormat.Fractional => C.UnitFormat.Fractional,
                _ => throw new ArgumentOutOfRangeException(nameof(unitFormat)),
            };
        }
    }
}
