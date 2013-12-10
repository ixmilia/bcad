using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BCad.Dxf
{
    public enum DxfUnitFormat
    {
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

    public enum DxfDimensionTextJustification
    {
        AboveLineCenter = 0,
        AboveLineNextToFirstExtension = 1,
        AboveLineNextToSecondExtension = 2,
        AboveLineCenteredOnFirstExtension = 3,
        AboveLineCenteredOnSecondExtension = 4
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

    public enum DxfDragMode
    {
        Off = 0,
        On = 1,
        Auto = 2
    }

    public enum DxfDrawingUnits
    {
        English = 0,
        Metric = 1
    }

    public enum DxfPickStyle
    {
        None = 0,
        Group = 1,
        AssociativeHatch = 2,
        GroupAndAssociativeHatch = 3
    }

    public enum DxfShadeEdgeMode
    {
        FacesShadedEdgeNotHighlighted = 0,
        FacesShadedEdgesHighlightedInBlack = 1,
        FacesNotFilledEdgesInEntityColor = 2,
        FacesInEntityColorEdgesInBlack = 3
    }

    public enum DxfPolySketchMode
    {
        SketchLines = 0,
        SketchPolylines = 1
    }
}

namespace BCad.Dxf.Sections
{
    internal class DxfHeaderSection : DxfSection
    {
        public DxfHeader Header { get; private set; }

        public DxfHeaderSection()
        {
            Header = new DxfHeader();
        }

        public override DxfSectionType Type
        {
            get { return DxfSectionType.Header; }
        }

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs(DxfAcadVersion version)
        {
            var values = new List<DxfCodePair>();
            DxfHeader.AddValueToList(values, this.Header, version);
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
                    DxfHeader.SetHeaderVariable(keyName, pair, section.Header);
                }
            }

            return section;
        }
    }
}
