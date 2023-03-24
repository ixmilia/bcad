using IxMilia.Converters;

namespace IxMilia.BCad.Extensions
{
    public static class DimensionStyleExtensions
    {
        public static DimensionSettings ToDimensionSettings(this DimensionStyle dimStyle)
        {
            return new DimensionSettings(
                textHeight: dimStyle.TextHeight,
                extensionLineOffset: dimStyle.ExtensionLineOffset,
                extensionLineExtension: dimStyle.ExtensionLineExtension,
                dimensionLineGap: dimStyle.LineGap,
                arrowSize: dimStyle.ArrowSize,
                tickSize: dimStyle.TickSize);
        }
    }
}
