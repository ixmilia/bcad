using System;
using System.Collections.Generic;
using BCad.Dxf.Sections;

namespace BCad.Dxf.Tables
{
    public class DxfDimStyle : DxfSymbolTableFlags
    {
        internal const string AcDbDimStyleTableRecordText = "AcDbDimStyleTableRecord";
        public string Name { get; set; }
        public string DimensioningSuffix { get; set; }
        public string AlternateDimensioningSuffix { get; set; }
        public string ArrowBlockName { get; set; }
        public string FirstArrowBlockname { get; set; }
        public string SecondArrowBlockName { get; set; }
        public double DimensioningScaleFactor { get; set; }
        public double DimensioningArrowSize { get; set; }
        public double DimensionExtensionLineOffset { get; set; }
        public double DimensionLineIncrement { get; set; }
        public double DimensionExtensionLineExtension { get; set; }
        public double DimensionDistanceRoundingValue { get; set; }
        public double DimensionLineExtension { get; set; }
        public double DimensionPlusTolerance { get; set; }
        public double DimensionMinusTolerance { get; set; }
        public double DimensioningTextHeight { get; set; }
        public double CenterMarkSize { get; set; }
        public double DimensioningTickSize { get; set; }
        public double AlternateDimensioningScaleFactor { get; set; }
        public double DimensionLinearMeasurementScaleFactor { get; set; }
        public double DimensionVerticalTextPosition { get; set; }
        public double DimensionToleranceDisplacScaleFactor { get; set; }
        public double DimensionLineGap { get; set; }
        public bool GenerateDimensionTolerances { get; set; }
        public bool GenerateDimensionLimits { get; set; }
        public bool DimensionTextInsideHorizontal { get; set; }
        public bool DimensionTextOutsideHorizontal { get; set; }
        public bool SuppressFirstDimensionExtensionLine { get; set; }
        public bool SuppressSecondDimensionExtensionLine { get; set; }
        public bool TextAboveDimensionLine { get; set; }
        public DxfUnitZeroSuppression DimensionUnitZeroSuppression { get; set; }
        public bool UseAlternateDimensioning { get; set; }
        public short AlternateDimensioningDecimalPlaces { get; set; }
        public bool ForceDimensionLineExtensionsOutsideIfTextExists { get; set; }
        public bool UseSeparateArrowBlocksForDimensions { get; set; }
        public bool ForceDimensionTextInsideExtensions { get; set; }
        public bool SuppressOutsideExtensionDimensionLines { get; set; }
        public DxfColor DimensionLineColor { get; set; }
        public DxfColor DimensionExtensionLineColor { get; set; }
        public DxfColor DimensionTextColor { get; set; }
        public DxfUnitFormat DimensionUnitFormat { get; set; }
        public short DimensionUnitToleranceDecimalPlaces { get; set; }
        public short DimensionToleranceDecimalPlaces { get; set; }
        public DxfUnitFormat AlternateDimensioningUnits { get; set; }
        public short AlternateDimensioningToleranceDecimalPlaces { get; set; }
        public string StyleHandle { get; set; }
        public DxfAngleFormat DimensioningAngleFormat { get; set; }
        public DxfDimensionTextJustification DimensionTextJustification { get; set; }
        public DxfJustification DimensionToleranceVerticalJustification { get; set; }
        public DxfUnitZeroSuppression DimensionToleranceZeroSuppression { get; set; }
        public DxfUnitZeroSuppression AlternateDimensioningZeroSuppression { get; set; }
        public DxfUnitZeroSuppression AlternateDimensioningToleranceZeroSuppression { get; set; }
        public DxfDimensionFit DimensionTextAndArrowPlacement { get; set; }
        public bool DimensionCursorControlsTextPosition { get; set; }

        public DxfDimStyle()
        {
            DimensioningScaleFactor = 1.0;
            DimensioningArrowSize = 1.0;
            DimensionLineColor = DxfColor.ByBlock;
            DimensionExtensionLineColor = DxfColor.ByBlock;
            DimensionTextColor = DxfColor.ByBlock;
        }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            var list = new List<DxfCodePair>();
            Action<int, object> add = (code, value) => list.Add(new DxfCodePair(code, value));
            Func<bool, short> toShort = (value) => (short)(value ? 1 : 0);
            add(100, AcDbDimStyleTableRecordText);
            add(2, Name);
            add(70, (short)Flags);
            add(3, DimensioningSuffix);
            add(4, AlternateDimensioningSuffix);
            add(5, ArrowBlockName);
            add(6, FirstArrowBlockname);
            add(7, SecondArrowBlockName);
            add(40, DimensioningScaleFactor);
            add(41, DimensioningArrowSize);
            add(42, DimensionExtensionLineOffset);
            add(43, DimensionLineIncrement);
            add(44, DimensionExtensionLineExtension);
            add(45, DimensionDistanceRoundingValue);
            add(46, DimensionLineExtension);
            add(47, DimensionPlusTolerance);
            add(48, DimensionMinusTolerance);
            add(140, DimensioningTextHeight);
            add(141, CenterMarkSize);
            add(142, DimensioningTickSize);
            add(143, AlternateDimensioningScaleFactor);
            add(144, DimensionLinearMeasurementScaleFactor);
            add(145, DimensionVerticalTextPosition);
            add(146, DimensionToleranceDisplacScaleFactor);
            add(147, DimensionLineGap);
            add(71, toShort(GenerateDimensionTolerances));
            add(72, toShort(GenerateDimensionLimits));
            add(73, toShort(DimensionTextInsideHorizontal));
            add(74, toShort(DimensionTextOutsideHorizontal));
            add(75, toShort(SuppressFirstDimensionExtensionLine));
            add(76, toShort(SuppressSecondDimensionExtensionLine));
            add(77, toShort(TextAboveDimensionLine));
            add(78, (short)DimensionUnitZeroSuppression);
            add(170, toShort(UseAlternateDimensioning));
            add(171, AlternateDimensioningDecimalPlaces);
            add(172, toShort(ForceDimensionLineExtensionsOutsideIfTextExists));
            add(173, toShort(UseSeparateArrowBlocksForDimensions));
            add(174, toShort(ForceDimensionTextInsideExtensions));
            add(175, toShort(SuppressOutsideExtensionDimensionLines));
            add(176, DimensionLineColor.RawValue);
            add(177, DimensionExtensionLineColor.RawValue);
            add(178, DimensionTextColor.RawValue);
            add(270, (short)DimensionUnitFormat);
            add(271, DimensionUnitToleranceDecimalPlaces);
            add(272, DimensionToleranceDecimalPlaces);
            add(273, (short)AlternateDimensioningUnits);
            add(274, AlternateDimensioningToleranceDecimalPlaces);
            add(340, StyleHandle);
            add(275, (short)DimensioningAngleFormat);
            add(280, (short)DimensionTextJustification);
            add(281, toShort(SuppressFirstDimensionExtensionLine));
            add(282, toShort(SuppressSecondDimensionExtensionLine));
            add(283, (short)DimensionToleranceVerticalJustification);
            add(284, (short)DimensionToleranceZeroSuppression);
            add(285, (short)AlternateDimensioningZeroSuppression);
            add(286, (short)AlternateDimensioningToleranceZeroSuppression);
            add(287, (short)DimensionTextAndArrowPlacement);
            add(288, toShort(DimensionCursorControlsTextPosition));

            return list;
        }

        internal static DxfDimStyle FromBuffer(DxfCodePairBufferReader buffer)
        {
            var dimStyle = new DxfDimStyle();
            Func<short, bool> fromShort = (value) => value != 0;
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                switch (pair.Code)
                {
                    case 2:
                        dimStyle.Name = pair.StringValue;
                        break;
                    case 3:
                        dimStyle.DimensioningSuffix = pair.StringValue;
                        break;
                    case 4:
                        dimStyle.AlternateDimensioningSuffix = pair.StringValue;
                        break;
                    case 5:
                        dimStyle.ArrowBlockName = pair.StringValue;
                        break;
                    case 6:
                        dimStyle.FirstArrowBlockname = pair.StringValue;
                        break;
                    case 7:
                        dimStyle.SecondArrowBlockName = pair.StringValue;
                        break;
                    case 40:
                        dimStyle.DimensioningScaleFactor = pair.DoubleValue;
                        break;
                    case 41:
                        dimStyle.DimensioningArrowSize = pair.DoubleValue;
                        break;
                    case 42:
                        dimStyle.DimensionExtensionLineOffset = pair.DoubleValue;
                        break;
                    case 43:
                        dimStyle.DimensionLineIncrement = pair.DoubleValue;
                        break;
                    case 44:
                        dimStyle.DimensionExtensionLineExtension = pair.DoubleValue;
                        break;
                    case 45:
                        dimStyle.DimensionDistanceRoundingValue = pair.DoubleValue;
                        break;
                    case 46:
                        dimStyle.DimensionLineExtension = pair.DoubleValue;
                        break;
                    case 47:
                        dimStyle.DimensionPlusTolerance = pair.DoubleValue;
                        break;
                    case 48:
                        dimStyle.DimensionMinusTolerance = pair.DoubleValue;
                        break;
                    case 70:
                        dimStyle.Flags = pair.ShortValue;
                        break;
                    case 71:
                        dimStyle.GenerateDimensionTolerances = fromShort(pair.ShortValue);
                        break;
                    case 72:
                        dimStyle.GenerateDimensionLimits = fromShort(pair.ShortValue);
                        break;
                    case 73:
                        dimStyle.DimensionTextInsideHorizontal = fromShort(pair.ShortValue);
                        break;
                    case 74:
                        dimStyle.DimensionTextOutsideHorizontal = fromShort(pair.ShortValue);
                        break;
                    case 75:
                        dimStyle.SuppressFirstDimensionExtensionLine = fromShort(pair.ShortValue);
                        break;
                    case 76:
                        dimStyle.SuppressSecondDimensionExtensionLine = fromShort(pair.ShortValue);
                        break;
                    case 77:
                        dimStyle.TextAboveDimensionLine = fromShort(pair.ShortValue);
                        break;
                    case 78:
                        dimStyle.DimensionUnitZeroSuppression = (DxfUnitZeroSuppression)pair.ShortValue;
                        break;
                    case 140:
                        dimStyle.DimensioningTextHeight = pair.DoubleValue;
                        break;
                    case 141:
                        dimStyle.CenterMarkSize = pair.DoubleValue;
                        break;
                    case 142:
                        dimStyle.DimensioningTickSize = pair.DoubleValue;
                        break;
                    case 143:
                        dimStyle.AlternateDimensioningScaleFactor = pair.DoubleValue;
                        break;
                    case 144:
                        dimStyle.DimensionLinearMeasurementScaleFactor = pair.DoubleValue;
                        break;
                    case 145:
                        dimStyle.DimensionVerticalTextPosition = pair.DoubleValue;
                        break;
                    case 146:
                        dimStyle.DimensionToleranceDisplacScaleFactor = pair.DoubleValue;
                        break;
                    case 147:
                        dimStyle.DimensionLineGap = pair.DoubleValue;
                        break;
                    case 170:
                        dimStyle.UseAlternateDimensioning = fromShort(pair.ShortValue);
                        break;
                    case 171:
                        dimStyle.AlternateDimensioningDecimalPlaces = pair.ShortValue;
                        break;
                    case 172:
                        dimStyle.ForceDimensionLineExtensionsOutsideIfTextExists = fromShort(pair.ShortValue);
                        break;
                    case 173:
                        dimStyle.UseSeparateArrowBlocksForDimensions = fromShort(pair.ShortValue);
                        break;
                    case 174:
                        dimStyle.ForceDimensionTextInsideExtensions = fromShort(pair.ShortValue);
                        break;
                    case 175:
                        dimStyle.SuppressOutsideExtensionDimensionLines = fromShort(pair.ShortValue);
                        break;
                    case 176:
                        dimStyle.DimensionLineColor = DxfColor.FromRawValue(pair.ShortValue);
                        break;
                    case 177:
                        dimStyle.DimensionExtensionLineColor = DxfColor.FromRawValue(pair.ShortValue);
                        break;
                    case 178:
                        dimStyle.DimensionTextColor = DxfColor.FromRawValue(pair.ShortValue);
                        break;
                    case 270:
                        dimStyle.DimensionUnitFormat = (DxfUnitFormat)pair.ShortValue;
                        break;
                    case 271:
                        dimStyle.DimensionUnitToleranceDecimalPlaces = pair.ShortValue;
                        break;
                    case 272:
                        dimStyle.DimensionToleranceDecimalPlaces = pair.ShortValue;
                        break;
                    case 273:
                        dimStyle.AlternateDimensioningUnits = (DxfUnitFormat)pair.ShortValue;
                        break;
                    case 274:
                        dimStyle.AlternateDimensioningToleranceDecimalPlaces = pair.ShortValue;
                        break;
                    case 275:
                        dimStyle.DimensioningAngleFormat = (DxfAngleFormat)pair.ShortValue;
                        break;
                    case 280:
                        dimStyle.DimensionTextJustification = (DxfDimensionTextJustification)pair.ShortValue;
                        break;
                    case 281:
                        dimStyle.SuppressFirstDimensionExtensionLine = fromShort(pair.ShortValue);
                        break;
                    case 282:
                        dimStyle.SuppressSecondDimensionExtensionLine = fromShort(pair.ShortValue);
                        break;
                    case 283:
                        dimStyle.DimensionToleranceVerticalJustification = (DxfJustification)pair.ShortValue;
                        break;
                    case 284:
                        dimStyle.DimensionToleranceZeroSuppression = (DxfUnitZeroSuppression)pair.ShortValue;
                        break;
                    case 285:
                        dimStyle.AlternateDimensioningZeroSuppression = (DxfUnitZeroSuppression)pair.ShortValue;
                        break;
                    case 286:
                        dimStyle.AlternateDimensioningToleranceZeroSuppression = (DxfUnitZeroSuppression)pair.ShortValue;
                        break;
                    case 287:
                        dimStyle.DimensionTextAndArrowPlacement = (DxfDimensionFit)pair.ShortValue;
                        break;
                    case 288:
                        dimStyle.DimensionCursorControlsTextPosition = fromShort(pair.ShortValue);
                        break;
                    case 340:
                        dimStyle.StyleHandle = pair.StringValue;
                        break;
                }
            }

            return dimStyle;
        }
    }
}
