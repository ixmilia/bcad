using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Sections
{
    public enum DxfUnitFormat
    {
        None = 0,
        Scientific = 1,
        Decimal = 2,
        Engineering = 3,
        ArchitecturalStacked = 4,
        FractionalStacked = 5,
        Architectural = 6,
        Fractional = 7,
    }

    public enum DxfAngleDirection
    {
        CounterClockwise = 0,
        Clockwise = 1
    }

    public enum DxfAttributeVisibility
    {
        None = 0,
        Normal = 1,
        All = 2
    }

    public enum DxfJustification
    {
        Top = 0,
        Middle = 1,
        Bottom = 2
    }

    public enum DxfCoordinateDisplay
    {
        Static = 0,
        ContinuousUpdate = 1,
        DistanceAngleFormat = 2
    }

    public enum DxfUnitZeroSuppression
    {
        SuppressZeroFeetAndZeroInches = 0,
        IncludeZeroFeetAndZeroInches = 1,
        IncludeZeroFeetAndSuppressZeroInches = 2,
        IncludeZeroInchesAndSuppressZeroFeet = 3
    }

    public enum DxfAngleFormat
    {
        DecimalDegrees = 0,
        DegreesMinutesSeconds = 1,
        Gradians = 2,
        Radians = 3,
        SurveyorsUnits = 4
    }

    public enum DxfDimensionFit
    {
        TextAndArrowsOutsideLines = 0,
        MoveArrowsFirst = 1,
        MoveTextFirst = 2,
        MoveEitherForBestFit = 3
    }

    public class DxfHeaderSection : DxfSection
    {
        public short MaintenanceVersion { get; set; }
        public DxfAcadVersion Version { get; set; }
        public double AngleZeroDirection { get; set; }
        public DxfAngleDirection AngleDirection { get; set; }
        public bool AttributeEntityDialogs { get; set; }
        public DxfAttributeVisibility AttributeVisibility { get; set; }
        public bool AttributePromptDuringInsert { get; set; }
        public DxfUnitFormat AngleUnitFormat { get; set; }
        public short AngleUnitPrecision { get; set; }
        public bool BlipMode { get; set; }
        public DxfColor CurrentEntityColor { get; set; }
        public double CurrentEntityLinetypeScale { get; set; }
        public string CurrentEntityLinetypeName { get; set; }
        public double FirstChamferDistance { get; set; }
        public double SecondChamferDistance { get; set; }
        public double ChamferLength { get; set; }
        public double ChamferAngle { get; set; }
        public string CurrentLayer { get; set; }
        public DxfJustification CurrentMultilineJustification { get; set; }
        public double CurrentMultilineScale { get; set; }
        public string CurrentMultilineStyleName { get; set; }
        public DxfCoordinateDisplay CoordinateDisplay { get; set; }
        public bool RetainDeletedObjects { get; set; }
        public bool UseAlternateUnitDimensioning { get; set; }
        public short AlternateUnitDecimalPlaces { get; set; }
        public double AlternateUnitScaleFactor { get; set; }
        public short AlternateUnitToleranceDecimalPlaces { get; set; }
        public DxfUnitZeroSuppression AlternateUnitToleranceZeroSuppression { get; set; }
        public DxfUnitFormat AlternateUnitFormat { get; set; }
        public DxfUnitZeroSuppression AlternateUnitZeroSuppression { get; set; }
        public string AlternateDimensioningSuffix { get; set; }
        public bool CreateAssociativeDimensioning { get; set; }
        public double DimensioningArrowSize { get; set; }
        public DxfAngleFormat AngularDimensionFormat { get; set; }
        public string ArrowBlockName { get; set; }
        public string FirstArrowBlockName { get; set; }
        public string SecondArrowBlockName { get; set; }
        public double CenterMarkAndLineSize { get; set; }
        public DxfColor DimensionLineColor { get; set; }
        public DxfColor DimensionExtensionLineColor { get; set; }
        public DxfColor DimensionTextColor { get; set; }
        public short DimensionUnitToleranceDecimalPlaces { get; set; }
        public double DimensionLineExtension { get; set; }
        public double DimensionLineIncrement { get; set; }
        public double DimensionExtensionLineExtension { get; set; }
        public double DimensionExtensionLineOffset { get; set; }
        public DxfDimensionFit DimensionFit { get; set; }

        public DxfPoint DrawingExtentsMaximum { get; set; }
        public DxfPoint DrawingExtentsMinimum { get; set; }
        public DxfUnitFormat UnitFormat { get; set; }
        public short UnitPrecision { get; set; }

        private const string ACADMAINTVER = "$ACADMAINTVER";
        private const string ACADVER = "$ACADVER";
        private const string ANGBASE = "$ANGBASE";
        private const string ANGDIR = "$ANGDIR";
        private const string ATTDIA = "$ATTDIA";
        private const string ATTMODE = "$ATTMODE";
        private const string ATTREQ = "$ATTREQ";
        private const string AUNITS = "$AUNITS";
        private const string AUPREC = "$AUPREC";
        private const string BLIPMODE = "$BLIPMODE";
        private const string CECOLOR = "$CECOLOR";
        private const string CELTSCALE = "$CELTSCALE";
        private const string CELTYPE = "$CELTYPE";
        private const string CHAMFERA = "$CHAMFERA";
        private const string CHAMFERB = "$CHAMFERB";
        private const string CHAMFERC = "$CHAMFERC";
        private const string CHAMFERD = "$CHAMFERD";
        private const string CLAYER = "$CLAYER";
        private const string CMLJUST = "$CMLJUST";
        private const string CMLSCALE = "$CMLSCALE";
        private const string CMLSTYLE = "$CMLSTYLE";
        private const string COORDS = "$COORDS";
        private const string DELOBJ = "$DELOBJ";
        private const string DIMALT = "$DIMALT";
        private const string DIMALTD = "$DIMALTD";
        private const string DIMALTF = "$DIMALTF";
        private const string DIMALTTD = "$DIMALTTD";
        private const string DIMALTTZ = "$DIMALTTZ";
        private const string DIMALTU = "$DIMALTU";
        private const string DIMALTZ = "$DIMALTZ";
        private const string DIMAPOST = "$DIMAPOST";
        private const string DIMASO = "$DIMASO";
        private const string DIMASZ = "$DIMASZ";
        private const string DIMAUNIT = "$DIMAUNIT";
        private const string DIMBLK = "$DIMBLK";
        private const string DIMBLK1 = "$DIMBLK1";
        private const string DIMBLK2 = "$DIMBLK2";
        private const string DIMCEN = "$DIMCEN";
        private const string DIMCLRD = "$DIMCLRD";
        private const string DIMCLRE = "$DIMCLRE";
        private const string DIMCLRT = "$DIMCLRT";
        private const string DIMDEC = "$DIMDEC";
        private const string DIMDLE = "$DIMDLE";
        private const string DIMDLI = "$DIMDLI";
        private const string DIMEXE = "$DIMEXE";
        private const string DIMEXO = "$DIMEXO";
        private const string DIMFIT = "$DIMFIT";

        private const string EXTMAX = "$EXTMAX";
        private const string EXTMIN = "$EXTMIN";

        private const string LUNITS = "$LUNITS";
        private const string LUPREC = "$LUPREC";

        public DxfHeaderSection()
        {
            CurrentEntityColor = DxfColor.ByBlock;
            CurrentEntityLinetypeScale = 1.0;
            CurrentEntityLinetypeName = "BYBLOCK";
            CurrentMultilineScale = 1.0;
            CurrentLayer = null;
            AlternateUnitScaleFactor = 1.0;
            DrawingExtentsMaximum = new DxfPoint();
            DrawingExtentsMinimum = new DxfPoint();
            AlternateUnitFormat = DxfUnitFormat.None;
            DimensioningArrowSize = 1.0;
            CenterMarkAndLineSize = 1.0;
            Version = DxfAcadVersion.R14;
            UnitFormat = DxfUnitFormat.None;
        }

        public override DxfSectionType Type
        {
            get { return DxfSectionType.Header; }
        }

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs()
        {
            var values = new List<DxfCodePair>();
            Action<string, int, object> addValue = (name, code, value) =>
                {
                    values.Add(new DxfCodePair(9, name));
                    values.Add(new DxfCodePair(code, value));
                };

            Action<string, DxfPoint> addValuePoint3 = (name, point) =>
                {
                    values.Add(new DxfCodePair(9, name));
                    values.Add(new DxfCodePair(10, point.X));
                    values.Add(new DxfCodePair(20, point.Y));
                    values.Add(new DxfCodePair(30, point.Z));
                };

            Func<bool, short> boolToShort = (value) => (short)(value ? 1 : 0);

            addValue(ACADMAINTVER, 70, MaintenanceVersion);
            addValue(ACADVER, 1, DxfAcadVersionStrings.VersionToString(Version));
            addValue(ANGBASE, 50, AngleZeroDirection);
            addValue(ANGDIR, 70, AngleDirection);
            addValue(ATTDIA, 70, boolToShort(AttributeEntityDialogs));
            addValue(ATTMODE, 70, AttributeVisibility);
            addValue(ATTREQ, 70, boolToShort(AttributePromptDuringInsert));
            addValue(AUNITS, 70, AngleUnitFormat);
            addValue(AUPREC, 70, AngleUnitPrecision);
            addValue(BLIPMODE, 70, boolToShort(BlipMode));
            addValue(CECOLOR, 62, CurrentEntityColor.RawValue);
            addValue(CELTSCALE, 40, CurrentEntityLinetypeScale);
            addValue(CELTYPE, 6, CurrentEntityLinetypeName);
            addValue(CHAMFERA, 40, FirstChamferDistance);
            addValue(CHAMFERB, 40, SecondChamferDistance);
            addValue(CHAMFERC, 40, ChamferLength);
            addValue(CHAMFERD, 40, ChamferAngle);
            addValue(CLAYER, 8, CurrentLayer);
            addValue(CMLJUST, 70, CurrentMultilineJustification);
            addValue(CMLSCALE, 40, CurrentMultilineScale);
            addValue(CMLSTYLE, 2, CurrentMultilineStyleName);
            addValue(COORDS, 70, CoordinateDisplay);
            addValue(DELOBJ, 70, boolToShort(RetainDeletedObjects));
            addValue(DIMALT, 70, boolToShort(UseAlternateUnitDimensioning));
            addValue(DIMALTD, 70, AlternateUnitDecimalPlaces);
            addValue(DIMALTF, 40, AlternateUnitScaleFactor);
            addValue(DIMALTTD, 70, AlternateUnitToleranceDecimalPlaces);
            addValue(DIMALTTZ, 70, AlternateUnitToleranceZeroSuppression);
            addValue(DIMALTU, 70, AlternateUnitFormat);
            addValue(DIMALTZ, 70, AlternateUnitZeroSuppression);
            addValue(DIMAPOST, 1, AlternateDimensioningSuffix);
            addValue(DIMASO, 70, boolToShort(CreateAssociativeDimensioning));
            addValue(DIMASZ, 40, DimensioningArrowSize);
            addValue(DIMAUNIT, 70, AngularDimensionFormat);
            addValue(DIMBLK, 1, ArrowBlockName);
            addValue(DIMBLK1, 1, FirstArrowBlockName);
            addValue(DIMBLK2, 1, SecondArrowBlockName);
            addValue(DIMCEN, 40, CenterMarkAndLineSize);
            addValue(DIMCLRD, 70, DimensionLineColor.RawValue);
            addValue(DIMCLRE, 70, DimensionExtensionLineColor.RawValue);
            addValue(DIMCLRT, 70, DimensionTextColor.RawValue);
            addValue(DIMDEC, 70, DimensionUnitToleranceDecimalPlaces);
            addValue(DIMDLE, 40, DimensionLineExtension);
            addValue(DIMDLI, 40, DimensionLineIncrement);
            addValue(DIMEXE, 40, DimensionExtensionLineExtension);
            addValue(DIMEXO, 40, DimensionExtensionLineOffset);
            addValue(DIMFIT, 70, DimensionFit);

            addValuePoint3(EXTMAX, DrawingExtentsMaximum);
            addValuePoint3(EXTMIN, DrawingExtentsMinimum);

            addValue(LUNITS, 70, UnitFormat);
            addValue(LUPREC, 70, UnitPrecision);

            return values;
        }

        internal static DxfHeaderSection HeaderSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfHeaderSection();
            string keyName = null;
            Func<short, bool> shortToBool = value => value != 0;

            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfCodePair.IsSectionEnd(pair))
                {
                    // done reading settings
                    break;
                }

                if (pair.Code == 9)
                {
                    // what setting to get
                    keyName = pair.StringValue;
                }
                else
                {
                    // the value of the setting
                    switch (keyName)
                    {
                        case ACADMAINTVER:
                            EnsureCode(pair, 70);
                            section.MaintenanceVersion = pair.ShortValue;
                            break;
                        case ACADVER:
                            EnsureCode(pair, 1);
                            section.Version = DxfAcadVersionStrings.StringToVersion(pair.StringValue);
                            break;
                        case ANGBASE:
                            EnsureCode(pair, 50);
                            section.AngleZeroDirection = pair.DoubleValue;
                            break;
                        case ANGDIR:
                            EnsureCode(pair, 70);
                            section.AngleDirection = (DxfAngleDirection)pair.ShortValue;
                            break;
                        case ATTDIA:
                            EnsureCode(pair, 70);
                            section.AttributeEntityDialogs = shortToBool(pair.ShortValue);
                            break;
                        case ATTMODE:
                            EnsureCode(pair, 70);
                            section.AttributeVisibility = (DxfAttributeVisibility)pair.ShortValue;
                            break;
                        case ATTREQ:
                            EnsureCode(pair, 70);
                            section.AttributePromptDuringInsert = shortToBool(pair.ShortValue);
                            break;
                        case AUNITS:
                            EnsureCode(pair, 70);
                            section.AngleUnitFormat = (DxfUnitFormat)pair.ShortValue;
                            break;
                        case AUPREC:
                            EnsureCode(pair, 70);
                            section.AngleUnitPrecision = pair.ShortValue;
                            break;
                        case BLIPMODE:
                            EnsureCode(pair, 70);
                            section.BlipMode = shortToBool(pair.ShortValue);
                            break;
                        case CECOLOR:
                            EnsureCode(pair, 62);
                            section.CurrentEntityColor = DxfColor.FromRawValue(pair.ShortValue);
                            break;
                        case CELTSCALE:
                            EnsureCode(pair, 40);
                            section.CurrentEntityLinetypeScale = pair.DoubleValue;
                            break;
                        case CELTYPE:
                            EnsureCode(pair, 6);
                            section.CurrentEntityLinetypeName = pair.StringValue;
                            break;
                        case CHAMFERA:
                            EnsureCode(pair, 40);
                            section.FirstChamferDistance = pair.DoubleValue;
                            break;
                        case CHAMFERB:
                            EnsureCode(pair, 40);
                            section.SecondChamferDistance = pair.DoubleValue;
                            break;
                        case CHAMFERC:
                            EnsureCode(pair, 40);
                            section.ChamferLength = pair.DoubleValue;
                            break;
                        case CHAMFERD:
                            EnsureCode(pair, 40);
                            section.ChamferAngle = pair.DoubleValue;
                            break;
                        case CLAYER:
                            EnsureCode(pair, 8);
                            section.CurrentLayer = pair.StringValue;
                            break;
                        case CMLJUST:
                            EnsureCode(pair, 70);
                            section.CurrentMultilineJustification = (DxfJustification)pair.ShortValue;
                            break;
                        case CMLSCALE:
                            EnsureCode(pair, 40);
                            section.CurrentMultilineScale = pair.DoubleValue;
                            break;
                        case CMLSTYLE:
                            EnsureCode(pair, 2);
                            section.CurrentMultilineStyleName = pair.StringValue;
                            break;
                        case COORDS:
                            EnsureCode(pair, 70);
                            section.CoordinateDisplay = (DxfCoordinateDisplay)pair.ShortValue;
                            break;
                        case DELOBJ:
                            EnsureCode(pair, 70);
                            section.RetainDeletedObjects = shortToBool(pair.ShortValue);
                            break;
                        case DIMALT:
                            EnsureCode(pair, 70);
                            section.UseAlternateUnitDimensioning = shortToBool(pair.ShortValue);
                            break;
                        case DIMALTD:
                            EnsureCode(pair, 70);
                            section.AlternateUnitDecimalPlaces = pair.ShortValue;
                            break;
                        case DIMALTF:
                            EnsureCode(pair, 40);
                            section.AlternateUnitScaleFactor = pair.DoubleValue;
                            break;
                        case DIMALTTD:
                            EnsureCode(pair, 70);
                            section.AlternateUnitToleranceDecimalPlaces = pair.ShortValue;
                            break;
                        case DIMALTTZ:
                            EnsureCode(pair, 70);
                            section.AlternateUnitToleranceZeroSuppression = (DxfUnitZeroSuppression)pair.ShortValue;
                            break;
                        case DIMALTU:
                            EnsureCode(pair, 70);
                            section.AlternateUnitFormat = (DxfUnitFormat)pair.ShortValue;
                            break;
                        case DIMALTZ:
                            EnsureCode(pair, 70);
                            section.AlternateUnitZeroSuppression = (DxfUnitZeroSuppression)pair.ShortValue;
                            break;
                        case DIMAPOST:
                            EnsureCode(pair, 1);
                            section.AlternateDimensioningSuffix = pair.StringValue;
                            break;
                        case DIMASO:
                            EnsureCode(pair, 70);
                            section.CreateAssociativeDimensioning = shortToBool(pair.ShortValue);
                            break;
                        case DIMASZ:
                            EnsureCode(pair, 40);
                            section.DimensioningArrowSize = pair.DoubleValue;
                            break;
                        case DIMAUNIT:
                            EnsureCode(pair, 70);
                            section.AngularDimensionFormat = (DxfAngleFormat)pair.ShortValue;
                            break;
                        case DIMBLK:
                            EnsureCode(pair, 1);
                            section.ArrowBlockName = pair.StringValue;
                            break;
                        case DIMBLK1:
                            EnsureCode(pair, 1);
                            section.FirstArrowBlockName = pair.StringValue;
                            break;
                        case DIMBLK2:
                            EnsureCode(pair, 1);
                            section.SecondArrowBlockName = pair.StringValue;
                            break;
                        case DIMCEN:
                            EnsureCode(pair, 40);
                            section.CenterMarkAndLineSize = pair.DoubleValue;
                            break;
                        case DIMCLRD:
                            EnsureCode(pair, 70);
                            section.DimensionLineColor = DxfColor.FromRawValue(pair.ShortValue);
                            break;
                        case DIMCLRE:
                            EnsureCode(pair, 70);
                            section.DimensionExtensionLineColor = DxfColor.FromRawValue(pair.ShortValue);
                            break;
                        case DIMCLRT:
                            EnsureCode(pair, 70);
                            section.DimensionTextColor = DxfColor.FromRawValue(pair.ShortValue);
                            break;
                        case DIMDEC:
                            EnsureCode(pair, 70);
                            section.DimensionUnitToleranceDecimalPlaces = pair.ShortValue;
                            break;
                        case DIMDLE:
                            EnsureCode(pair, 40);
                            section.DimensionLineExtension = pair.DoubleValue;
                            break;
                        case DIMDLI:
                            EnsureCode(pair, 40);
                            section.DimensionLineIncrement = pair.DoubleValue;
                            break;
                        case DIMEXE:
                            EnsureCode(pair, 40);
                            section.DimensionExtensionLineExtension = pair.DoubleValue;
                            break;
                        case DIMEXO:
                            EnsureCode(pair, 40);
                            section.DimensionExtensionLineOffset = pair.DoubleValue;
                            break;
                        case DIMFIT:
                            EnsureCode(pair, 70);
                            section.DimensionFit = (DxfDimensionFit)pair.ShortValue;
                            break;
                        case EXTMAX:
                            switch (pair.Code)
                            {
                                case 10:
                                    section.DrawingExtentsMaximum.X = pair.DoubleValue;
                                    break;
                                case 20:
                                    section.DrawingExtentsMaximum.Y = pair.DoubleValue;
                                    break;
                                case 30:
                                    section.DrawingExtentsMaximum.Z = pair.DoubleValue;
                                    break;
                            }
                            break;
                        case EXTMIN:
                            switch (pair.Code)
                            {
                                case 10:
                                    section.DrawingExtentsMinimum.X = pair.DoubleValue;
                                    break;
                                case 20:
                                    section.DrawingExtentsMinimum.Y = pair.DoubleValue;
                                    break;
                                case 30:
                                    section.DrawingExtentsMinimum.Z = pair.DoubleValue;
                                    break;
                            }
                            break;
                        case LUNITS:
                            EnsureCode(pair, 70);
                            section.UnitFormat = (DxfUnitFormat)pair.ShortValue;
                            break;
                        case LUPREC:
                            EnsureCode(pair, 70);
                            section.UnitPrecision = pair.ShortValue;
                            break;
                        default:
                            // unsupported variable
                            break;
                    }
                }
            }

            return section;
        }

        private static void EnsureCode(DxfCodePair pair, int code)
        {
            if (pair.Code != code)
            {
                throw new DxfReadException(string.Format("Expected code {0}, got {1}", code, pair.Code));
            }
        }
    }
}
