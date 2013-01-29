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

    public class DxfHeaderSection : DxfSection
    {
        public short MaintenanceVersion { get; set; }
        public DxfAcadVersion Version { get; set; }
        public double AngleZeroDirection { get; set; }
        public DxfAngleDirection AngleDirection { get; set; }
        public DxfAttributeVisibility AttributeVisibility { get; set; }
        public DxfUnitFormat AngleUnitFormat { get; set; }
        public short AngleUnitPrecision { get; set; }
        public string CurrentLayer { get; set; }
        public DxfUnitFormat UnitFormat { get; set; }
        public short UnitPrecision { get; set; }

        private const string ACADMAINTVER = "$ACADMAINTVER";
        private const string ACADVER = "$ACADVER";
        private const string ANGBASE = "$ANGBASE";
        private const string ANGDIR = "$ANGDIR";
        private const string ATTMODE = "$ATTMODE";
        private const string AUNITS = "$AUNITS";
        private const string AUPREC = "$AUPREC";
        private const string CLAYER = "$CLAYER";
        private const string LUNITS = "$LUNITS";
        private const string LUPREC = "$LUPREC";

        public DxfHeaderSection()
        {
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

            if (MaintenanceVersion != 0) addValue(ACADMAINTVER, 70, MaintenanceVersion);
            if (Version != DxfAcadVersion.R14) addValue(ACADVER, 1, DxfAcadVersionStrings.VersionToString(Version));
            if (AngleZeroDirection != 0.0) addValue(ANGBASE, 50, AngleZeroDirection);
            if (AngleDirection != DxfAngleDirection.CounterClockwise) addValue(ANGDIR, 70, (short)AngleDirection);
            if (AttributeVisibility != DxfAttributeVisibility.None) addValue(ATTMODE, 70, (short)AttributeVisibility);
            if (AngleUnitFormat != DxfUnitFormat.None) addValue(AUNITS, 70, (short)AngleUnitFormat);
            if (AngleUnitPrecision != 0) addValue(AUPREC, 70, AngleUnitPrecision);
            if (!string.IsNullOrEmpty(CurrentLayer)) addValue(CLAYER, 8, CurrentLayer);
            if (UnitFormat != DxfUnitFormat.None) addValue(LUNITS, 70, (short)UnitFormat);
            if (UnitPrecision != 0) addValue(LUPREC, 70, (short)UnitPrecision);

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
                        case ATTMODE:
                            EnsureCode(pair, 70);
                            section.AttributeVisibility = (DxfAttributeVisibility)pair.ShortValue;
                            break;
                        case AUNITS:
                            EnsureCode(pair, 70);
                            section.AngleUnitFormat = (DxfUnitFormat)pair.ShortValue;
                            break;
                        case AUPREC:
                            EnsureCode(pair, 70);
                            section.AngleUnitPrecision = pair.ShortValue;
                            break;
                        case CLAYER:
                            EnsureCode(pair, 8);
                            section.CurrentLayer = pair.StringValue;
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
