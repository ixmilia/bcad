namespace IxMilia.BCad.Dialogs
{
    public class DimensionStylesDialogEntry
    {
        public bool IsDeleted { get; }
        public string OriginalName { get; }
        public string Name { get; }
        public double ArrowSize { get; }
        public double ExtensionLineOffset { get; }
        public double ExtensionLineExtension { get; }
        public double TextHeight { get; }
        public double LineGap { get; }
        public CadColor? LineColor { get; }
        public CadColor? TextColor { get; }

        public DimensionStylesDialogEntry(
            bool isDeleted,
            string originalName,
            string name,
            double arrowSize,
            double extensionLineOffset,
            double extensionLineExtension,
            double textHeight,
            double lineGap,
            CadColor? lineColor,
            CadColor? textColor)
        {
            IsDeleted = isDeleted;
            OriginalName = originalName;
            Name = name;
            ArrowSize = arrowSize;
            ExtensionLineOffset = extensionLineOffset;
            ExtensionLineExtension = extensionLineExtension;
            TextHeight = textHeight;
            LineGap = lineGap;
            LineColor = lineColor;
            TextColor = textColor;
        }

        public DimensionStyle ToDimensionStyle()
        {
            return new DimensionStyle(
                name: Name,
                arrowSize: ArrowSize,
                extensionLineOffset: ExtensionLineOffset,
                extensionLineExtension: ExtensionLineExtension,
                textHeight: TextHeight,
                lineGap: LineGap,
                lineColor: LineColor,
                textColor: TextColor);
        }

        public static DimensionStylesDialogEntry FromDimensionStyle(DimensionStyle dimensionStyle)
        {
            return new DimensionStylesDialogEntry(
                isDeleted: false,
                originalName: dimensionStyle.Name,
                name: dimensionStyle.Name,
                arrowSize: dimensionStyle.ArrowSize,
                extensionLineOffset: dimensionStyle.ExtensionLineOffset,
                extensionLineExtension: dimensionStyle.ExtensionLineExtension,
                textHeight: dimensionStyle.TextHeight,
                lineGap: dimensionStyle.LineGap,
                lineColor: dimensionStyle.LineColor,
                textColor: dimensionStyle.TextColor);
        }
    }
}
