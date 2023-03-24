using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;
using IxMilia.Converters;

namespace IxMilia.BCad.Entities
{
    public class LinearDimension : AbstractDimension
    {
        public override EntityKind Kind => EntityKind.LinearDimension;

        private BoundingBox _boundingBox;
        public override BoundingBox BoundingBox => _boundingBox;

        public Point DefinitionPoint1 { get; }
        public Point DefinitionPoint2 { get; }
        public Point DimensionLineLocation { get; }
        public bool IsAligned { get; }
        public Point TextMidPoint { get; }
        public string TextOverride { get; }

        private SnapPoint[] _snapPoints;

        public LinearDimension(
            Point definitionPoint1,
            Point definitionPoint2,
            Point dimensionLineLocation,
            bool isAligned,
            Point textMidPoint,
            string dimensionStyleName,
            string textOverride = null,
            CadColor? textColor = null,
            CadColor? color = null,
            LineTypeSpecification lineTypeSpecification = null,
            object tag = null)
            : base(dimensionStyleName, textColor, color, lineTypeSpecification, tag)
        {
            DefinitionPoint1 = definitionPoint1;
            DefinitionPoint2 = definitionPoint2;
            DimensionLineLocation = dimensionLineLocation;
            IsAligned = isAligned;
            TextMidPoint = textMidPoint;
            TextOverride = textOverride;

            _snapPoints = new SnapPoint[]
            {
                new EndPoint(DefinitionPoint1),
                new EndPoint(DefinitionPoint2),
                // TODO: `DimensionLineLocation` on both sides
            };
        }

        public override IEnumerable<IPrimitive> GetPrimitives(DrawingSettings settings)
        {
            return GetOrCreatePrimitives(settings, () =>
            {
                var dimStyle = settings.DimensionStyles[DimensionStyleName];
                var dimensionSettings = dimStyle.ToDimensionSettings();
                var dimensionProperties = LinearDimensionProperties.BuildFromValues(
                    DefinitionPoint1.ToConverterVector(),
                    DefinitionPoint2.ToConverterVector(),
                    DimensionLineLocation.ToConverterVector(),
                    IsAligned,
                    TextOverride,
                    0.0,
                    dimensionSettings);
                // recompute with formatted display
                var displayText = TextOverride ?? DimensionExtensions.GenerateLinearDimensionText(
                    dimensionProperties.DimensionLength,
                    settings.DrawingUnits.ToConverterDrawingUnits(),
                    settings.UnitFormat.ToConverterUnitFormat(),
                    settings.UnitPrecision);
                var textWidth = dimensionSettings.TextHeight * displayText.Length * 0.6; // this is really bad
                dimensionProperties = LinearDimensionProperties.BuildFromValues(
                    DefinitionPoint1.ToConverterVector(),
                    DefinitionPoint2.ToConverterVector(),
                    DimensionLineLocation.ToConverterVector(),
                    IsAligned,
                    displayText,
                    textWidth,
                    dimensionSettings);
                var primitives = GetPrimitives(dimensionProperties, dimensionSettings.TextHeight, Color, TextColor, dimStyle);
                var boundingPoints = primitives.SelectMany(p => p.GetInterestingPoints()).ToArray();
                _boundingBox = BoundingBox.FromPoints(boundingPoints);
                ActualMeasurement = dimensionProperties.DimensionLength;
                return primitives;
            });
        }

        public override IEnumerable<SnapPoint> GetSnapPoints() => _snapPoints;

        public LinearDimension Update(
            Optional<Point> definitionPoint1 = default,
            Optional<Point> definitionPoint2 = default,
            Optional<Point> dimensionLineLocation = default,
            Optional<bool> isAligned = default,
            Optional<Point> textMidPoint = default,
            Optional<string> dimensionStyleName = default,
            Optional<string> textOverride = default,
            Optional<CadColor?> textColor = default,
            Optional<CadColor?> color = default,
            Optional<LineTypeSpecification> lineTypeSpecification = default,
            Optional<object> tag = default)
        {
            var newDefinitionPoint1 = definitionPoint1.GetValue(DefinitionPoint1);
            var newDefinitionPoint2 = definitionPoint2.GetValue(DefinitionPoint2);
            var newDimensionLineLocation = dimensionLineLocation.GetValue(DimensionLineLocation);
            var newIsAligned = isAligned.GetValue(IsAligned);
            var newTextMidPoint = textMidPoint.GetValue(TextMidPoint);
            var newDimensionStyleName = dimensionStyleName.GetValue(DimensionStyleName) ?? throw new ArgumentNullException(nameof(dimensionStyleName));
            var newTextOverride = textOverride.GetValue(TextOverride);
            var newTextColor = textColor.GetValue(TextColor);
            var newColor = color.GetValue(Color);
            var newLineTypeSpecification = lineTypeSpecification.GetValue(LineTypeSpecification);
            var newTag = tag.GetValue(Tag);

            if (newDefinitionPoint1 == DefinitionPoint1 &&
                newDefinitionPoint2 == DefinitionPoint2 &&
                newDimensionLineLocation == DimensionLineLocation &&
                newIsAligned == IsAligned &&
                newTextMidPoint == TextMidPoint &&
                newDimensionStyleName == DimensionStyleName &&
                newTextOverride == TextOverride &&
                newTextColor == TextColor &&
                newColor == Color &&
                newLineTypeSpecification == LineTypeSpecification &&
                newTag == Tag)
            {
                return this;
            }

            return new LinearDimension(
                newDefinitionPoint1,
                newDefinitionPoint2,
                newDimensionLineLocation,
                newIsAligned,
                newTextMidPoint,
                newDimensionStyleName,
                newTextOverride,
                newTextColor,
                newColor,
                newLineTypeSpecification,
                newTag);
        }

        internal static IPrimitive[] GetPrimitives(
            LinearDimensionProperties properties,
            double textHeight,
            CadColor? lineColor,
            CadColor? textColor,
            DimensionStyle dimStyle)
        {
            var text = properties.DisplayText ?? string.Empty;
            var textWidth = textHeight * text.Length * 0.6; // this is really wrong
            var primitives = new List<IPrimitive>();
            primitives.AddRange(properties.DimensionLineSegments.Select(s =>
                new PrimitiveLine(s.Start.ToPoint(), s.End.ToPoint(), lineColor ?? dimStyle.LineColor)));
            primitives.AddRange(properties.DimensionTriangles.Select(t =>
                new PrimitiveTriangle(t.P1.ToPoint(), t.P2.ToPoint(), t.P3.ToPoint(), lineColor ?? dimStyle.LineColor)));
            primitives.Add(
                new PrimitiveText(
                    text,
                    properties.TextLocation.ToPoint(),
                    dimStyle.TextHeight,
                    Vector.ZAxis,
                    properties.DimensionLineAngle * MathHelper.RadiansToDegrees,
                    textColor ?? dimStyle.TextColor)
            );

            return primitives.ToArray();
        }
    }
}
