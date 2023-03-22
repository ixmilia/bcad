using System;

namespace IxMilia.BCad
{
    public class DimensionStyle
    {
        public const string DefaultDimensionStyleName = "STANDARD";

        public string Name { get; }

        public double ArrowSize { get; }
        public double ExtensionLineOffset { get; }
        public double ExtensionLineExtension { get; }
        public double TextHeight { get; }
        public double LineGap { get; }
        public CadColor? LineColor { get; }
        public CadColor? TextColor { get; }

        public DimensionStyle(string name)
            : this(name,
                  arrowSize: 0.18,
                  extensionLineOffset: 0.0625,
                  extensionLineExtension: 0.18,
                  textHeight: 0.18,
                  lineGap: 0.09,
                  lineColor: null,
                  textColor: null)
        {
        }

        public DimensionStyle(
            string name,
            double arrowSize,
            double extensionLineOffset,
            double extensionLineExtension,
            double textHeight,
            double lineGap,
            CadColor? lineColor,
            CadColor? textColor)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ArrowSize = arrowSize;
            ExtensionLineOffset = extensionLineOffset;
            ExtensionLineExtension = extensionLineExtension;
            TextHeight = textHeight;
            LineGap = lineGap;
            LineColor = lineColor;
            TextColor = textColor;
        }

        public DimensionStyle Update(
            Optional<string> name = default,
            Optional<double> arrowSize = default,
            Optional<double> extensionLineOffset = default,
            Optional<double> extensionLineExtension = default,
            Optional<double> textHeight = default,
            Optional<double> lineGap = default,
            Optional<CadColor?> lineColor = default,
            Optional<CadColor?> textColor = default)
        {
            var newName = name.GetValue(Name) ?? throw new ArgumentNullException(nameof(name));
            var newArrowSize = arrowSize.GetValue(ArrowSize);
            var newExtensionLineOffset = extensionLineOffset.GetValue(ExtensionLineOffset);
            var newExtensionLineExtension = extensionLineExtension.GetValue(ExtensionLineExtension);
            var newTextHeight = textHeight.GetValue(TextHeight);
            var newLineGap = lineGap.GetValue(LineGap);
            var newLineColor = lineColor.GetValue(LineColor);
            var newTextColor = textColor.GetValue(TextColor);

            if (newName == Name &&
                newArrowSize == ArrowSize &&
                newExtensionLineOffset == ExtensionLineOffset &&
                newExtensionLineExtension == ExtensionLineExtension &&
                newTextHeight == TextHeight &&
                newLineGap == LineGap &&
                newLineColor == LineColor &&
                newTextColor == TextColor)
            {
                return this;
            }

            return new DimensionStyle(
                name: newName,
                arrowSize: newArrowSize,
                extensionLineOffset: newExtensionLineOffset,
                extensionLineExtension: newExtensionLineExtension,
                textHeight: newTextHeight,
                lineGap: newLineGap,
                lineColor: newLineColor,
                textColor: newTextColor);
        }

        public static DimensionStyle CreateDefault() => new DimensionStyle(DefaultDimensionStyleName);
    }
}
