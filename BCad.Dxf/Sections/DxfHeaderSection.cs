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
        private const string LUNITS = "$LUNITS";
        private const string LUPREC = "$LUPREC";

        public DxfHeaderSection()
        {
            CurrentEntityColor = DxfColor.ByBlock;
            CurrentEntityLinetypeScale = 1.0;
            CurrentEntityLinetypeName = "BYBLOCK";
            CurrentMultilineScale = 1.0;
            CurrentLayer = null;
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

            Func<bool, short> boolToShort = (value) => (short)(value ? 1 : 0);

            addValue(ACADMAINTVER, 70, MaintenanceVersion);
            addValue(ACADVER, 1, DxfAcadVersionStrings.VersionToString(Version));
            addValue(ANGBASE, 50, AngleZeroDirection);
            addValue(ANGDIR, 70, (short)AngleDirection);
            addValue(ATTDIA, 70, boolToShort(AttributeEntityDialogs));
            addValue(ATTMODE, 70, (short)AttributeVisibility);
            addValue(ATTREQ, 70, boolToShort(AttributePromptDuringInsert));
            addValue(AUNITS, 70, (short)AngleUnitFormat);
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
            addValue(CMLJUST, 70, (short)CurrentMultilineJustification);
            addValue(CMLSCALE, 40, CurrentMultilineScale);
            addValue(CMLSTYLE, 2, CurrentMultilineStyleName);
            addValue(COORDS, 70, (short)CoordinateDisplay);
            addValue(LUNITS, 70, (short)UnitFormat);
            addValue(LUPREC, 70, (short)UnitPrecision);

            return values;
        }

        internal static DxfHeaderSection HeaderSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfHeaderSection();
            string keyName = null;
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfCodePair.IsSectionEnd(pair))
                {
                    // done reading settings
                    break;
                }

                if (keyName == null)
                {
                    // what setting to set
                    if (pair.Code == 9)
                    {
                        keyName = pair.StringValue;
                    }

                    // otherwise, ignore values until another 9 code
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
                            section.AttributeEntityDialogs = pair.ShortValue == 1;
                            break;
                        case ATTMODE:
                            EnsureCode(pair, 70);
                            section.AttributeVisibility = (DxfAttributeVisibility)pair.ShortValue;
                            break;
                        case ATTREQ:
                            EnsureCode(pair, 70);
                            section.AttributePromptDuringInsert = pair.ShortValue == 1;
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
                            section.BlipMode = pair.ShortValue != 0;
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

                    keyName = null; // reset for next read
                }
            }

            if (keyName != null)
            {
                throw new DxfReadException("Expected value for key " + keyName);
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
