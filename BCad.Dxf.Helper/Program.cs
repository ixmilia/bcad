using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace BCad.Dxf.Helper
{
    class Program
    {
        static string Xmlns = "http://IxMilia.com/Dxf/DxfSpec";
        static XName VariableElement = XName.Get("Variable", Xmlns);
        static string NameAttribute = "Name";
        static string MinVersionAttribute = "MinVersion";
        static string MaxVersionAttribute = "MaxVersion";

        static void Main(string[] args)
        {
            var supportedVariables = new Dictionary<string, Tuple<DxfAcadVersion, DxfAcadVersion>>();
            var pairs = new List<Tuple<DxfAcadVersion, string>>
            {
                Tuple.Create(DxfAcadVersion.R14, R14Variables),
                Tuple.Create(DxfAcadVersion.R2000, R2000Variables),
                Tuple.Create(DxfAcadVersion.R2004, R2004Variables),
                Tuple.Create(DxfAcadVersion.R2007, R2007Variables),
                Tuple.Create(DxfAcadVersion.R2010, R2010Variables),
                Tuple.Create(DxfAcadVersion.R2013, R2013Variables),
            };

            foreach (var pair in pairs)
            {
                var version = pair.Item1;
                var variables = pair.Item2.Split('\n')
                    .Select(v => v.Trim())
                    .Where(v => v.StartsWith("$"))
                    .Select(v => TrimVariable(v).Substring(1).ToUpper());
                foreach (var variable in variables)
                {
                    if (!supportedVariables.ContainsKey(variable))
                    {
                        // add the variable
                        supportedVariables.Add(variable, Tuple.Create(version, version));
                    }
                    else
                    {
                        // update the max version
                        supportedVariables[variable] = Tuple.Create(supportedVariables[variable].Item1, version);
                    }
                }
            }

            var existing = XDocument.Load("HeaderVariablesSpec.xml");
            var root = existing.Root;
            foreach (var variable in supportedVariables.Keys)
            {
                EnsureVariable(existing.Root, variable, supportedVariables[variable].Item1, supportedVariables[variable].Item2);
            }

            var writer = XmlWriter.Create("output.xml", new XmlWriterSettings() { Indent = true });
            var newBody = existing.Root.Elements().OrderBy(e => e.Attribute(NameAttribute).Value);
            existing.Root.ReplaceNodes(newBody);
            existing.WriteTo(writer);
            writer.Flush();
            Process.Start("notepad.exe", "output.xml");
        }

        private static void EnsureVariable(XElement root, string variableName, DxfAcadVersion minVersion, DxfAcadVersion maxVersion)
        {
            var element = root.Elements().SingleOrDefault(e => e.Attribute(NameAttribute).Value == variableName);
            if (element == null)
            {
                element = new XElement(VariableElement, new XAttribute(NameAttribute, variableName));
                root.Add(element);
            }

            if (minVersion != DxfAcadVersion.Min)
                EnsureAttribute(element, MinVersionAttribute, minVersion);
            else
                EnsureNotAttribute(element, MinVersionAttribute);

            if (maxVersion != DxfAcadVersion.Max)
                EnsureAttribute(element, MaxVersionAttribute, maxVersion);
            else
                EnsureNotAttribute(element, MaxVersionAttribute);
        }

        private static void EnsureAttribute(XElement element, string attributeName, object attributeValue)
        {
            var attr = element.Attribute(attributeName);
            if (attr == null)
                element.Add(new XAttribute(attributeName, attributeValue));
            else
                attr.Value = attributeValue.ToString();
        }

        private static void EnsureNotAttribute(XElement element, string attributeName)
        {
            var attr = element.Attribute(attributeName);
            if (attr != null)
                attr.Remove();
        }

        private static string TrimVariable(string text)
        {
            var space = text.IndexOf(' ');
            if (space < 0)
                return text;
            else
                return text.Substring(0, space);
        }

        static string R14Variables = @"
$ACADMAINTVER
$ACADVER
$ANGBASE
$ANGDIR
$ATTDIA
$ATTMODE
$ATTREQ
$AUNITS
$AUPREC
$BLIPMODE
$CECOLOR
$CELTSCALE
$CELTYPE
$CHAMFERA
$CHAMFERB
$CHAMFERC
$CHAMFERD
$CLAYER
$CMLJUST
$CMLSCALE
$CMLSTYLE
$COORDS
$DELOBJ
$DIMALT
$DIMALTD
$DIMALTF
$DIMALTTD
$DIMALTTZ
$DIMALTU
$DIMALTZ
$DIMAPOST
$DIMASO
$DIMASZ
$DIMAUNIT
$DIMBLK
$DIMBLK1
$DIMBLK2
$DIMCEN
$DIMCLRD
$DIMCLRE
$DIMCLRT
$DIMDEC 
$DIMDLE
$DIMDLI
$DIMEXE
$DIMEXO
$DIMFIT 
$DIMGAP
$DIMJUST 
$DIMLFAC
$DIMLIM
$DIMPOST
$DIMRND
$DIMSAH
$DIMSCALE
$DIMSD1
$DIMSD2
$DIMSE1
$DIMSE2
$DIMSHO
$DIMSOXD
$DIMSTYLE
$DIMTAD
$DIMTDEC
$DIMTFAC
$DIMTIH
$DIMTIX
$DIMTM
$DIMTOFL
$DIMTOH
$DIMTOL
$DIMTOLJ
$DIMTP
$DIMTSZ
$DIMTVP
$DIMTXSTY 
$DIMTXT
$DIMTZIN
$DIMUNIT
$DIMUPT
$DIMZIN
$DISPSILH 
$DRAGMODE
$DWGCODEPAGE
$ELEVATION
$EXTMAX
$EXTMIN
$FILLETRAD
$FILLMODE
$HANDLING
$HANDSEED
$INSBASE
$LIMCHECK
$LIMMAX
$LIMMIN
$LTSCALE
$LUNITS
$LUPREC
$MAXACTVP
$MEASUREMENT
$MENU
$MIRRTEXT
$ORTHOMODE
$OSMODE
$PDMODE
$PDSIZE
$PELEVATION
$PEXTMAX
$PEXTMIN
$PICKSTYLE 
$PINSBASE
$PLIMCHECK
$PLIMMAX
$PLIMMIN
$PLINEGEN
$PLINEWID
$PROXYGRAPHICS
$PSLTSCALE
$PUCSNAME
$PUCSORG
$PUCSXDIR
$PUCSYDIR
$QTEXTMODE
$REGENMODE
$SHADEDGE
$SHADEDIF
$SKETCHINC
$SKPOLY
$SPLFRAME
$SPLINESEGS
$SPLINETYPE
$SURFTAB1
$SURFTAB2
$SURFTYPE
$SURFU
$SURFV
$TDCREATE
$TDINDWG
$TDUPDATE
$TDUSRTIMER
$TEXTSIZE
$TEXTSTYLE
$THICKNESS
$TILEMODE
$TRACEWID
$TREEDEPTH
$UCSNAME
$UCSORG
$UCSXDIR
$UCSYDIR
$UNITMODE
$USERI1
$USERI2
$USERI3
$USERI4
$USERI5
$USERR1
$USERR2
$USERR3
$USERR4
$USERR5
$USRTIMER
$VISRETAIN
$WORLDVIEW";

        static string R2000Variables = @"$ACADMAINTVER
$ACADVER
$ANGBASE
$ANGDIR
$ATTMODE
$AUNITS
$AUPREC
$CECOLOR
$CELTSCALE
$CELTYPE
$CELWEIGHT 
$CEPSNTYPE 
$CHAMFERA
$CHAMFERB
$CHAMFERC
$CHAMFERD
$CLAYER
$CMLJUST
$CMLSCALE
$CMLSTYLE
$CPSNID 
$DIMADEC 
$DIMALT
$DIMALTD
$DIMALTF
$DIMALTRND 
$DIMALTTD
$DIMALTTZ
$DIMALTU
$DIMALTZ
$DIMAPOST
$DIMASO
$DIMASZ
$DIMATFIT 
$DIMAUNIT
$DIMAZIN 
$DIMBLK
$DIMBLK1
$DIMBLK2
$DIMCEN
$DIMCLRD
$DIMCLRE
$DIMCLRT
$DIMDEC 
$DIMDLE
$DIMDLI
$DIMDSEP 
$DIMEXE
$DIMEXO
$DIMFAC 
$DIMGAP
$DIMJUST 
$DIMLDRBLK 
$DIMLFAC
$DIMLIM
$DIMLUNIT 
$DIMLWD 
$DIMLWE 
$DIMPOST
$DIMRND
$DIMSAH
$DIMSCALE
$DIMSD1
$DIMSD2
$DIMSE1
$DIMSE2
$DIMSHO
$DIMSOXD
$DIMSTYLE
$DIMTAD
$DIMTDEC
$DIMTFAC
$DIMTIH
$DIMTIX
$DIMTM
$DIMTMOVE 
$DIMTOFL
$DIMTOH
$DIMTOL
$DIMTOLJ
$DIMTP
$DIMTSZ
$DIMTVP
$DIMTXSTY
$DIMTXT
$DIMTZIN
$DIMUPT
$DIMZIN
$DISPSILH 
$DWGCODEPAGE
$ELEVATION
$ENDCAPS 
$EXTMAX
$EXTMIN
$EXTNAMES 
$FILLETRAD
$FILLMODE
$FINGERPRINTGUID
$HANDSEED
$HYPERLINKBASE 
$INSBASE
$INSUNITS
$JOINSTYLE
$LIMCHECK
$LIMMAX
$LIMMIN
$LTSCALE
$LUNITS
$LUPREC
$LWDISPLAY
$MAXACTVP
$MEASUREMENT
$MENU
$MIRRTEXT
$ORTHOMODE
$PDMODE
$PDSIZE
$PELEVATION
$PEXTMAX
$PEXTMIN
$PINSBASE
$PLIMCHECK
$PLIMMAX
$PLIMMIN
$PLINEGEN
$PLINEWID
$PROXYGRAPHICS
$PSLTSCALE
$PSTYLEMODE
$PSVPSCALE
$PUCSBASE
$PUCSNAME
$PUCSORG
$PUCSORGBACK
$PUCSORGBOTTOM
$PUCSORGFRONT
$PUCSORGLEFT
$PUCSORGRIGHT
$PUCSORGTOP
$PUCSORTHOREF
$PUCSORTHOVIEW
$PUCSXDIR
$PUCSYDIR
$QTEXTMODE
$REGENMODE
$SHADEDGE
$SHADEDIF
$SKETCHINC
$SKPOLY
$SPLFRAME
$SPLINESEGS
$SPLINETYPE
$SURFTAB1
$SURFTAB2
$SURFTYPE
$SURFU
$SURFV
$TDCREATE
$TDINDWG
$TDUCREATE
$TDUPDATE
$TDUSRTIMER
$TDUUPDATE
$TEXTSIZE
$TEXTSTYLE
$THICKNESS
$TILEMODE
$TRACEWID
$TREEDEPTH
$UCSBASE
$UCSNAME
$UCSORG
$UCSORGBACK
$UCSORGBOTTOM
$UCSORGFRONT
$UCSORGLEFT
$UCSORGRIGHT
$UCSORGTOP
$UCSORTHOREF
$UCSORTHOVIEW
$UCSXDIR
$UCSYDIR
$UNITMODE
$USERI1
$USERI2
$USERI3
$USERI4
$USERI5
$USERR1
$USERR2
$USERR3
$USERR4
$USERR5
$USRTIMER
$VERSIONGUID
$VISRETAIN
$WORLDVIEW
$XEDIT";

        static string R2004Variables = @"$ACADMAINTVER 70 Maintenance version number (should be ignored)
$ACADVER 1 The AutoCAD® drawing database version number:
AC1006 = R10; AC1009 = R11 and R12;
AC1012 = R13; AC1014 = R14; AC1015 = AutoCAD 2000;
AC1018 = AutoCAD 2004
$ANGBASE 50 Angle 0 direction
$ANGDIR 70 1 = Clockwise angles
0 = Counterclockwise angles
$ATTMODE 70 Attribute visibility:
0 = None
1 = Normal
2 = All
$AUNITS 70 Units format for angles
$AUPREC 70 Units precision for angles
$CECOLOR 62 Current entity color number:
0 = BYBLOCK; 256 = BYLAYER
$CELTSCALE 40 Current entity linetype scale
$CELTYPE 6 Entity linetype name, or BYBLOCK or BYLAYER
$CELWEIGHT 370 Lineweight of new objects
$CEPSNID 390 Plotstyle handle of new objects; if CEPSNTYPE is 3, then this
value indicates the handle
$CEPSNTYPE 380 Plot style type of new objects:
0 = Plot style by layer
1 = Plot style by block
2 = Plot style by dictionary default
3 = Plot style by object ID/handle
HEADER Section Group Codes | 15
$CHAMFERA 40 First chamfer distance
$CHAMFERB 40 Second chamfer distance
$CHAMFERC 40 Chamfer length
$CHAMFERD 40 Chamfer angle
$CLAYER 8 Current layer name
$CMLJUST 70 Current multiline justification:
0 = Top; 1 = Middle; 2 = Bottom
$CMLSCALE 40 Current multiline scale
$CMLSTYLE 2 Current multiline style name
$DIMADEC 70 Number of precision places displayed in angular dimensions
$DIMALT 70 Alternate unit dimensioning performed if nonzero
$DIMALTD 70 Alternate unit decimal places
$DIMALTF 40 Alternate unit scale factor
$DIMALTRND 40 Determines rounding of alternate units
$DIMALTTD 70 Number of decimal places for tolerance values of an alternate
units dimension
$DIMALTTZ 70 Controls suppression of zeros for alternate tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMALTU 70 Units format for alternate units of all dimension style family
members except angular:
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural (stacked); 5 = Fractional (stacked);
6 = Architectural; 7 = Fractional
DXF header variables (continued)
Variable Group code Description
16 | Chapter 2 HEADER Section
$DIMALTZ 70 Controls suppression of zeros for alternate unit dimension
values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMAPOST 1 Alternate dimensioning suffix
$DIMASO 70 1 = Create associative dimensioning
0 = Draw individual entities
$DIMASSOC 280 Controls the associativity of dimension objects
0 = Creates exploded dimensions; there is no association
between elements of the dimension, and the lines, arcs,
arrowheads, and text of a dimension are drawn as separate
objects
1 = Creates non-associative dimension objects; the elements
of the dimension are formed into a single object, and if the
definition point on the object moves, then the dimension
value is updated
2 = Creates associative dimension objects; the elements of
the dimension are formed into a single object and one or
more definition points of the dimension are coupled with
association points on geometric objects
$DIMASZ 40 Dimensioning arrow size
$DIMATFIT 70 Controls dimension text and arrow placement when space is
not sufficient to place both within the extension lines:
0 = Places both text and arrows outside extension lines
1 = Moves arrows first, then text
2 = Moves text first, then arrows
3 = Moves either text or arrows, whichever fits best
AutoCAD adds a leader to moved dimension text when
DIMTMOVE is set to 1
$DIMAUNIT 70 Angle format for angular dimensions:
0 = Decimal degrees; 1 = Degrees/minutes/seconds;
2 = Gradians; 3 = Radians; 4 = Surveyor’s units
$DIMAZIN 70 Controls suppression of zeros for angular dimensions:
0 = Displays all leading and trailing zeros
1 = Suppresses leading zeros in decimal dimensions
2 = Suppresses trailing zeros in decimal dimensions
3 = Suppresses leading and trailing zeros
$DIMBLK 1 Arrow block name
DXF header variables (continued)
Variable Group code Description
HEADER Section Group Codes | 17
$DIMBLK1 1 First arrow block name
$DIMBLK2 1 Second arrow block name
$DIMCEN 40 Size of center mark/lines
$DIMCLRD 70 Dimension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMCLRE 70 Dimension extension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMCLRT 70 Dimension text color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMDEC 70 Number of decimal places for the tolerance values of a
primary units dimension
$DIMDLE 40 Dimension line extension
$DIMDLI 40 Dimension line increment
$DIMDSEP 70 Single-character decimal separator used when creating
dimensions whose unit format is decimal
$DIMEXE 40 Extension line extension
$DIMEXO 40 Extension line offset
$DIMFAC 40 Scale factor used to calculate the height of text for dimension
fractions and tolerances. AutoCAD multiplies DIMTXT by
DIMTFAC to set the fractional or tolerance text height
$DIMGAP 40 Dimension line gap
$DIMJUST 70 Horizontal dimension text position:
0 = Above dimension line and center-justified between
extension lines
1 = Above dimension line and next to first extension line
2 = Above dimension line and next to second extension line
3 = Above and center-justified to first extension line
4 = Above and center-justified to second extension line
$DIMLDRBLK 1 Arrow block name for leaders
$DIMLFAC 40 Linear measurements scale factor
DXF header variables (continued)
Variable Group code Description
18 | Chapter 2 HEADER Section
$DIMLIM 70 Dimension limits generated if nonzero
$DIMLUNIT 70 Sets units for all dimension types except Angular:
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural; 5 = Fractional; 6 = Windows desktop
$DIMLWD 70 Dimension line lineweight:
–3 = Standard
–2 = ByLayer
–1 = ByBlock
0–211 = an integer representing 100th of mm
$DIMLWE 70 Extension line lineweight:
–3 = Standard
–2 = ByLayer
–1 = ByBlock
0–211 = an integer representing 100th of mm
$DIMPOST 1 General dimensioning suffix
$DIMRND 40 Rounding value for dimension distances
$DIMSAH 70 Use separate arrow blocks if nonzero
$DIMSCALE 40 Overall dimensioning scale factor
$DIMSD1 70 Suppression of first extension line:
0 = Not suppressed; 1 = Suppressed
$DIMSD2 70 Suppression of second extension line:
0 = Not suppressed; 1 = Suppressed
$DIMSE1 70 First extension line suppressed if nonzero
$DIMSE2 70 Second extension line suppressed if nonzero
$DIMSHO 70 1 = Recompute dimensions while dragging
0 = Drag original image
$DIMSOXD 70 Suppress outside-extensions dimension lines if nonzero
$DIMSTYLE 2 Dimension style name
$DIMTAD 70 Text above dimension line if nonzero
$DIMTDEC 70 Number of decimal places to display the tolerance values
DXF header variables (continued)
Variable Group code Description
HEADER Section Group Codes | 19
$DIMTFAC 40 Dimension tolerance display scale factor
$DIMTIH 70 Text inside horizontal if nonzero
$DIMTIX 70 Force text inside extensions if nonzero
$DIMTM 40 Minus tolerance
$DIMTMOVE 70 Dimension text movement rules:
0 = Moves the dimension line with dimension text
1 = Adds a leader when dimension text is moved
2 = Allows text to be moved freely without a leader
$DIMTOFL 70 If text is outside extensions, force line extensions between
extensions if nonzero
$DIMTOH 70 Text outside horizontal if nonzero
$DIMTOL 70 Dimension tolerances generated if nonzero
$DIMTOLJ 70 Vertical justification for tolerance values:
0 = Top; 1 = Middle; 2 = Bottom
$DIMTP 40 Plus tolerance
$DIMTSZ 40 Dimensioning tick size:
0 = No ticks
$DIMTVP 40 Text vertical position
$DIMTXSTY 7 Dimension text style
$DIMTXT 40 Dimensioning text height
$DIMTZIN 70 Controls suppression of zeros for tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMUPT 70 Cursor functionality for user-positioned text:
0 = Controls only the dimension line location
1 = Controls the text position as well as the dimension line
location
DXF header variables (continued)
Variable Group code Description
20 | Chapter 2 HEADER Section
$DIMZIN 70 Controls suppression of zeros for primary unit values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DISPSILH 70 Controls the display of silhouette curves of body objects in
Wireframe mode:
0 = Off; 1 = On
$DWGCODEPAGE 3 Drawing code page; set to the system code page when a
new drawing is created, but not otherwise maintained by
AutoCAD
$ELEVATION 40 Current elevation set by ELEV command
$ENDCAPS 280 Lineweight endcaps setting for new objects:
0 = none; 1 = round; 2 = angle; 3 = square
$EXTMAX 10, 20, 30 X, Y, and Z drawing extents upper-right corner (in WCS)
$EXTMIN 10, 20, 30 X, Y, and Z drawing extents lower-left corner (in WCS)
$EXTNAMES 290 Controls symbol table naming:
0 = Release 14 compatibility. Limits names to 31 characters in
length. Names can include the letters A to Z, the numerals 0
to 9, and the special characters dollar sign ($), underscore
(_), and hyphen (–).
1 = AutoCAD 2000. Names can be up to 255 characters in
length, and can include the letters A to Z, the numerals 0 to
9, spaces, and any special characters not used for other
purposes by Microsoft Windows and AutoCAD
$FILLETRAD 40 Fillet radius
$FILLMODE 70 Fill mode on if nonzero
$FINGERPRINTGUID 2 Set at creation time, uniquely identifies a particular drawing
$HALOGAP 280 Specifies a gap to be displayed where an object is hidden by
another object; the value is specified as a percent of one unit
and is independent of the zoom level. A haloed line is
shortened at the point where it is hidden when HIDE or the
Hidden option of SHADEMODE is used
$HANDSEED 5 Next available handle
DXF header variables (continued)
Variable Group code Description
HEADER Section Group Codes | 21
$HIDETEXT 290 Specifies HIDETEXT system variable:
0 = HIDE ignores text objects when producing the hidden
view
1 = HIDE does not ignore text objects
$HYPERLINKBASE 1 Path for all relative hyperlinks in the drawing. If null, the
drawing path is used
$INDEXCTL 280 Controls whether layer and spatial indexes are created and
saved in drawing files:
0 = No indexes are created
1 = Layer index is created
2 = Spatial index is created
3 = Layer and spatial indexes are created
$INSBASE 10, 20, 30 Insertion base set by BASE command (in WCS)
$INSUNITS 70 Default drawing units for AutoCAD DesignCenter blocks:
0 = Unitless; 1 = Inches; 2 = Feet; 3 = Miles; 4 = Millimeters;
5 = Centimeters; 6 = Meters; 7 = Kilometers; 8 = Microinches;
9 = Mils; 10 = Yards; 11 = Angstroms; 12 = Nanometers;
13 = Microns; 14 = Decimeters; 15 = Decameters;
16 = Hectometers; 17 = Gigameters; 18 = Astronomical units;
19 = Light years; 20 = Parsecs
$INTERSECTIONCOLOR 70 Specifies the entity color of intersection polylines:
Values 1-255 designate an AutoCAD color index (ACI)
0 = Color BYBLOCK
256 = Color BYLAYER
257 = Color BYENTITY
$INTERSECTIONDISPLAY 290 Specifies the display of intersection polylines:
0 = Turns off the display of intersection polylines
1 = Turns on the display of intersection polylines
$JOINSTYLE 280 Lineweight joint setting for new objects:
0=none; 1= round; 2 = angle; 3 = flat
$LIMCHECK 70 Nonzero if limits checking is on
$LIMMAX 10, 20 XY drawing limits upper-right corner (in WCS)
$LIMMIN 10, 20 XY drawing limits lower-left corner (in WCS)
$LTSCALE 40 Global linetype scale
$LUNITS 70 Units format for coordinates and distances
DXF header variables (continued)
Variable Group code Description
22 | Chapter 2 HEADER Section
$LUPREC 70 Units precision for coordinates and distances
$LWDISPLAY 290 Controls the display of lineweights on the Model or Layout
tab:
0 = Lineweight is not displayed
1 = Lineweight is displayed
$MAXACTVP 70 Sets maximum number of viewports to be regenerated
$MEASUREMENT 70 Sets drawing units: 0 = English; 1 = Metric
$MENU 1 Name of menu file
$MIRRTEXT 70 Mirror text if nonzero
$OBSCOLOR 70 Specifies the color of obscured lines. An obscured line is a
hidden line made visible by changing its color and linetype
and is visible only when the HIDE or SHADEMODE command
is used. The OBSCUREDCOLOR setting is visible only if the
OBSCUREDLTYPE is turned ON by setting it to a value other
than 0.
0 and 256 = Entity color
1-255 = An AutoCAD color index (ACI)
$OBSLTYPE 280 Specifies the linetype of obscured lines. Obscured linetypes
are independent of zoom level, unlike regular AutoCAD
linetypes. Value 0 turns off display of obscured lines and is
the default. Linetype values are defined as follows:
0 = Off
1 = Solid
2 = Dashed
3 = Dotted
4 = Short Dash
5 = Medium Dash
6 = Long Dash
7 = Double Short Dash
8 = Double Medium Dash
9 = Double Long Dash
10 = Medium Long Dash
11 = Sparse Dot
$ORTHOMODE 70 Ortho mode on if nonzero
$PDMODE 70 Point display mode
$PDSIZE 40 Point display size
$PELEVATION 40 Current paper space elevation
DXF header variables (continued)
Variable Group code Description
HEADER Section Group Codes | 23
$PEXTMAX 10, 20, 30 Maximum X, Y, and Z extents for paper space
$PEXTMIN 10, 20, 30 Minimum X, Y, and Z extents for paper space
$PINSBASE 10, 20, 30 Paper space insertion base point
$PLIMCHECK 70 Limits checking in paper space when nonzero
$PLIMMAX 10, 20 Maximum X and Y limits in paper space
$PLIMMIN 10, 20 Minimum X and Y limits in paper space
$PLINEGEN 70 Governs the generation of linetype patterns around the
vertices of a 2D polyline:
1 = Linetype is generated in a continuous pattern around
vertices of the polyline
0 = Each segment of the polyline starts and ends with a dash
$PLINEWID 40 Default polyline width
$PROJECTNAME 1 Assigns a project name to the current drawing. Used when
an external reference or image is not found on its original
path. The project name points to a section in the registry that
can contain one or more search paths for each project name
defined. Project names and their search directories are
created from the Files tab of the Options dialog box
$PROXYGRAPHICS 70 Controls the saving of proxy object images
$PSLTSCALE 70 Controls paper space linetype scaling:
1 = No special linetype scaling
0 = Viewport scaling governs linetype scaling
$PSTYLEMODE 290 Indicates whether the current drawing is in a Color-
Dependent or Named Plot Style mode:
0 =Uses named plot style tables in the current drawing
1 = Uses color-dependent plot style tables in the current
drawing
$PSVPSCALE 40 View scale factor for new viewports:
0 = Scaled to fit
>0 = Scale factor (a positive real value)
$PUCSBASE 2 Name of the UCS that defines the origin and orientation of
orthographic UCS settings (paper space only)
$PUCSNAME 2 Current paper space UCS name
DXF header variables (continued)
Variable Group code Description
24 | Chapter 2 HEADER Section
$PUCSORG 10, 20, 30 Current paper space UCS origin
$PUCSORGBACK 10, 20, 30 Point which becomes the new UCS origin after changing
paper space UCS to BACK when PUCSBASE is set to WORLD
$PUCSORGBOTTOM 10, 20, 30 Point which becomes the new UCS origin after changing
paper space UCS to BOTTOM when PUCSBASE is set to
WORLD
$PUCSORGFRONT 10, 20, 30 Point which becomes the new UCS origin after changing
paper space UCS to FRONT when PUCSBASE is set to WORLD
$PUCSORGLEFT 10, 20, 30 Point which becomes the new UCS origin after changing
paper space UCS to LEFT when PUCSBASE is set to WORLD
$PUCSORGRIGHT 10, 20, 30 Point which becomes the new UCS origin after changing
paper space UCS to RIGHT when PUCSBASE is set to WORLD
$PUCSORGTOP 10, 20, 30 Point which becomes the new UCS origin after changing
paper space UCS to TOP when PUCSBASE is set to WORLD
$PUCSORTHOREF 2 If paper space UCS is orthographic (PUCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to. If blank, UCS is relative to WORLD
$PUCSORTHOVIEW 70 Orthographic view type of paper space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
$PUCSXDIR 10, 20, 30 Current paper space UCS X axis
$PUCSYDIR 10, 20, 30 Current paper space UCS Y axis
$QTEXTMODE 70 Quick Text mode on if nonzero
$REGENMODE 70 REGENAUTO mode on if nonzero
$SHADEDGE 70 0 = Faces shaded, edges not highlighted
1 = Faces shaded, edges highlighted in black
2 = Faces not filled, edges in entity color
3 = Faces in entity color, edges in black
$SHADEDIF 70 Percent ambient/diffuse light; range 1–100; default 70
$SKETCHINC 40 Sketch record increment
DXF header variables (continued)
Variable Group code Description
HEADER Section Group Codes | 25
$SKPOLY 70 0 = Sketch lines; 1 = Sketch polylines
$SORTENTS 280 Controls the object sorting methods; accessible from the
Options dialog box User Preferences tab. SORTENTS uses the
following bitcodes:
0 = Disables SORTENTS
1 = Sorts for object selection
2 = Sorts for object snap
4 = Sorts for redraws
8 = Sorts for MSLIDE command slide creation
16 = Sorts for REGEN commands
32 = Sorts for plotting
64 = Sorts for PostScript output
$SPLFRAME 70 Spline control polygon display: 1 = On; 0 = Off
$SPLINESEGS 70 Number of line segments per spline patch
$SPLINETYPE 70 Spline curve type for PEDIT Spline
$SURFTAB1 70 Number of mesh tabulations in first direction
$SURFTAB2 70 Number of mesh tabulations in second direction
$SURFTYPE 70 Surface type for PEDIT Smooth
$SURFU 70 Surface density (for PEDIT Smooth) in M direction
$SURFV 70 Surface density (for PEDIT Smooth) in N direction
$TDCREATE 40 Local date/time of drawing creation (see “Special Handling of
Date/Time Variables”)
$TDINDWG 40 Cumulative editing time for this drawing (see “Special
Handling of Date/Time Variables”)
$TDUCREATE 40 Universal date/time the drawing was created (see “Special
Handling of Date/Time Variables”)
$TDUPDATE 40 Local date/time of last drawing update (see “Special
Handling of Date/Time Variables”)
$TDUSRTIMER 40 User-elapsed timer
$TDUUPDATE 40 Universal date/time of the last update/save (see “Special
Handling of Date/Time Variables”)
DXF header variables (continued)
Variable Group code Description
26 | Chapter 2 HEADER Section
$TEXTSIZE 40 Default text height
$TEXTSTYLE 7 Current text style name
$THICKNESS 40 Current thickness set by ELEV command
$TILEMODE 70 1 for previous release compatibility mode; 0 otherwise
$TRACEWID 40 Default trace width
$TREEDEPTH 70 Specifies the maximum depth of the spatial index
$UCSBASE 2 Name of the UCS that defines the origin and orientation of
orthographic UCS settings
$UCSNAME 2 Name of current UCS
$UCSORG 10, 20, 30 Origin of current UCS (in WCS)
$UCSORGBACK 10, 20, 30 Point which becomes the new UCS origin after changing
model space UCS to BACK when UCSBASE is set to WORLD
$UCSORGBOTTOM 10, 20, 30 Point which becomes the new UCS origin after changing
model space UCS to BOTTOM when UCSBASE is set to
WORLD
$UCSORGFRONT 10, 20, 30 Point which becomes the new UCS origin after changing
model space UCS to FRONT when UCSBASE is set to WORLD
$UCSORGLEFT 10, 20, 30 Point which becomes the new UCS origin after changing
model space UCS to LEFT when UCSBASE is set to WORLD
$UCSORGRIGHT 10, 20, 30 Point which becomes the new UCS origin after changing
model space UCS to RIGHT when UCSBASE is set to WORLD
$UCSORGTOP 10, 20, 30 Point which becomes the new UCS origin after changing
model space UCS to TOP when UCSBASE is set to WORLD
$UCSORTHOREF 2 If model space UCS is orthographic (UCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to. If blank, UCS is relative to WORLD
$UCSORTHOVIEW 70 Orthographic view type of model space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
DXF header variables (continued)
Variable Group code Description
HEADER Section Group Codes | 27
$UCSXDIR 10, 20, 30 Direction of the current UCS X axis (in WCS)
$UCSYDIR 10, 20, 30 Direction of the current UCS Y axis (in WCS)
$UNITMODE 70 Low bit set = Display fractions, feet-and-inches, and
surveyor’s angles in input format
$USERI1 – 5 70 Five integer variables intended for use by third-party
$USERI2
$USERI3
$USERI4
$USERI5
developers
$USERR1 – 5 40 Five real variables intended for use by third-party developers
$USERR2
$USERR3
$USERR4
$USERR5
$USRTIMER 70 0 = Timer off; 1 = Timer on
$VERSIONGUID 2 Uniquely identifies a particular version of a drawing. Updated
when the drawing is modified
$VISRETAIN 70 0 = Don’t retain xref-dependent visibility settings
1 = Retain xref-dependent visibility settings
$WORLDVIEW 70 1 = Set UCS to WCS during DVIEW/VPOINT
0 = Don’t change UCS
$XCLIPFRAME 290 Controls the visibility of xref clipping boundaries:
0 = Clipping boundary is not visible
1 = Clipping boundary is visible
$XEDIT 290 Controls whether the current drawing can be edited in-place
when being referenced by another drawing.";

        static string R2007Variables = @"$ACADMAINTVER 70 Maintenance version number (should be ignored)
$ACADVER 1 The AutoCAD drawing database version number:
AC1006 = R10; AC1009 = R11 and R12;
AC1012 = R13; AC1014 = R14; AC1015 = AutoCAD 2000;
AC1018 = AutoCAD 2004
$ANGBASE 50 Angle 0 direction
$ANGDIR 70 1 = Clockwise angles
0 = Counterclockwise angles
$ATTMODE 70 Attribute visibility:
0 = None
1 = Normal
2 = All
$AUNITS 70 Units format for angles
$AUPREC 70 Units precision for angles
$CECOLOR 62 Current entity color number:
0 = BYBLOCK; 256 = BYLAYER
$CELTSCALE 40 Current entity linetype scale
$CELTYPE 6 Entity linetype name, or BYBLOCK or BYLAYER
$CELWEIGHT 370 Lineweight of new objects
14 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
Plotstyle handle of new objects; if CEPSNTYPE is 3, then
this value indicates the handle
$CEPSNID 390
$CEPSNTYPE 380 Plot style type of new objects:
0 = Plot style by layer
1 = Plot style by block
2 = Plot style by dictionary default
3 = Plot style by object ID/handle
$CHAMFERA 40 First chamfer distance
$CHAMFERB 40 Second chamfer distance
$CHAMFERC 40 Chamfer length
$CHAMFERD 40 Chamfer angle
$CLAYER 8 Current layer name
$CMLJUST 70 Current multiline justification:
0 = Top; 1 = Middle; 2 = Bottom
$CMLSCALE 40 Current multiline scale
$CMLSTYLE 2 Current multiline style name
$CSHADOW 280 Shadow mode for a 3D object:
0 = Casts and receives shadows
1 = Casts shadows
2 = Receives shadows
3 = Ignores shadows
$DIMADEC 70 Number of precision places displayed in angular dimensions
$DIMALT 70 Alternate unit dimensioning performed if nonzero
HEADER Section Group Codes | 15
DXF header variables
Variable Group code Description
$DIMALTD 70 Alternate unit decimal places
$DIMALTF 40 Alternate unit scale factor
$DIMALTRND 40 Determines rounding of alternate units
Number of decimal places for tolerance values of an alternate
units dimension
$DIMALTTD 70
$DIMALTTZ 70 Controls suppression of zeros for alternate tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
Units format for alternate units of all dimension style family
members except angular:
$DIMALTU 70
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural (stacked); 5 = Fractional (stacked);
6 = Architectural; 7 = Fractional
Controls suppression of zeros for alternate unit dimension
values:
$DIMALTZ 70
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMAPOST 1 Alternate dimensioning suffix
$DIMASO 70 1 = Create associative dimensioning
0 = Draw individual entities
$DIMASSOC 280 Controls the associativity of dimension objects
0 = Creates exploded dimensions; there is no association
between elements of the dimension, and the lines, arcs,
16 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
arrowheads, and text of a dimension are drawn as separate
objects
1 = Creates non-associative dimension objects; the elements
of the dimension are formed into a single object, and if the
definition point on the object moves, then the dimension
value is updated
2 = Creates associative dimension objects; the elements of
the dimension are formed into a single object and one or
more definition points of the dimension are coupled with
association points on geometric objects
$DIMASZ 40 Dimensioning arrow size
Controls dimension text and arrow placement when space
is not sufficient to place both within the extension lines:
$DIMATFIT 70
0 = Places both text and arrows outside extension lines
1 = Moves arrows first, then text
2 = Moves text first, then arrows
3 = Moves either text or arrows, whichever fits best
AutoCAD adds a leader to moved dimension text when
DIMTMOVE is set to 1
$DIMAUNIT 70 Angle format for angular dimensions:
0 = Decimal degrees; 1 = Degrees/minutes/seconds;
2 = Gradians; 3 = Radians; 4 = Surveyor's units
$DIMAZIN 70 Controls suppression of zeros for angular dimensions:
0 = Displays all leading and trailing zeros
1 = Suppresses leading zeros in decimal dimensions
2 = Suppresses trailing zeros in decimal dimensions
3 = Suppresses leading and trailing zeros
$DIMBLK 1 Arrow block name
$DIMBLK1 1 First arrow block name
$DIMBLK2 1 Second arrow block name
HEADER Section Group Codes | 17
DXF header variables
Variable Group code Description
$DIMCEN 40 Size of center mark/lines
$DIMCLRD 70 Dimension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMCLRE 70 Dimension extension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMCLRT 70 Dimension text color:
range is 0 = BYBLOCK; 256 = BYLAYER
Number of decimal places for the tolerance values of a
primary units dimension
$DIMDEC 70
$DIMDLE 40 Dimension line extension
$DIMDLI 40 Dimension line increment
Single-character decimal separator used when creating dimensions
whose unit format is decimal
$DIMDSEP 70
$DIMEXE 40 Extension line extension
$DIMEXO 40 Extension line offset
Scale factor used to calculate the height of text for dimension
fractions and tolerances. AutoCAD multiplies DIMTXT
by DIMTFAC to set the fractional or tolerance text height
$DIMFAC 40
$DIMGAP 40 Dimension line gap
$DIMJUST 70 Horizontal dimension text position:
0 = Above dimension line and center-justified between extension
lines
1 = Above dimension line and next to first extension line
18 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
2 = Above dimension line and next to second extension
line
3 = Above and center-justified to first extension line
4 = Above and center-justified to second extension line
$DIMLDRBLK 1 Arrow block name for leaders
$DIMLFAC 40 Linear measurements scale factor
$DIMLIM 70 Dimension limits generated if nonzero
$DIMLUNIT 70 Sets units for all dimension types except Angular:
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural; 5 = Fractional; 6 = Windows desktop
$DIMLWD 70 Dimension line lineweight:
-3 = Standard
-2 = ByLayer
-1 = ByBlock
0-211 = an integer representing 100th of mm
$DIMLWE 70 Extension line lineweight:
-3 = Standard
-2 = ByLayer
-1 = ByBlock
0-211 = an integer representing 100th of mm
$DIMPOST 1 General dimensioning suffix
$DIMRND 40 Rounding value for dimension distances
$DIMSAH 70 Use separate arrow blocks if nonzero
$DIMSCALE 40 Overall dimensioning scale factor
$DIMSD1 70 Suppression of first extension line:
HEADER Section Group Codes | 19
DXF header variables
Variable Group code Description
0 = Not suppressed; 1 = Suppressed
$DIMSD2 70 Suppression of second extension line:
0 = Not suppressed; 1 = Suppressed
$DIMSE1 70 First extension line suppressed if nonzero
$DIMSE2 70 Second extension line suppressed if nonzero
$DIMSHO 70 1 = Recompute dimensions while dragging
0 = Drag original image
$DIMSOXD 70 Suppress outside-extensions dimension lines if nonzero
$DIMSTYLE 2 Dimension style name
$DIMTAD 70 Text above dimension line if nonzero
$DIMTDEC 70 Number of decimal places to display the tolerance values
$DIMTFAC 40 Dimension tolerance display scale factor
$DIMTIH 70 Text inside horizontal if nonzero
$DIMTIX 70 Force text inside extensions if nonzero
$DIMTM 40 Minus tolerance
$DIMTMOVE 70 Dimension text movement rules:
0 = Moves the dimension line with dimension text
1 = Adds a leader when dimension text is moved
2 = Allows text to be moved freely without a leader
If text is outside extensions, force line extensions between
extensions if nonzero
$DIMTOFL 70
20 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$DIMTOH 70 Text outside horizontal if nonzero
$DIMTOL 70 Dimension tolerances generated if nonzero
$DIMTOLJ 70 Vertical justification for tolerance values:
0 = Top; 1 = Middle; 2 = Bottom
$DIMTP 40 Plus tolerance
$DIMTSZ 40 Dimensioning tick size:
0 = No ticks
$DIMTVP 40 Text vertical position
$DIMTXSTY 7 Dimension text style
$DIMTXT 40 Dimensioning text height
$DIMTZIN 70 Controls suppression of zeros for tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMUPT 70 Cursor functionality for user-positioned text:
0 = Controls only the dimension line location
1 = Controls the text position as well as the dimension line
location
$DIMZIN 70 Controls suppression of zeros for primary unit values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
HEADER Section Group Codes | 21
DXF header variables
Variable Group code Description
Controls the display of silhouette curves of body objects in
Wireframe mode:
$DISPSILH 70
0 = Off; 1 = On
Hard-pointer ID to visual style while creating 3D solid
primitives. The defualt value is NULL
$DRAGVS 349
Drawing code page; set to the system code page when a
new drawing is created, but not otherwise maintained by
AutoCAD
$DWGCODEPAGE 3
$ELEVATION 40 Current elevation set by ELEV command
$ENDCAPS 280 Lineweight endcaps setting for new objects:
0 = none; 1 = round; 2 = angle; 3 = square
$EXTMAX 10, 20, 30 X, Y, and Z drawing extents upper-right corner (in WCS)
$EXTMIN 10, 20, 30 X, Y, and Z drawing extents lower-left corner (in WCS)
$EXTNAMES 290 Controls symbol table naming:
0 = Release 14 compatibility. Limits names to 31 characters
in length. Names can include the letters A to Z, the numerals
0 to 9, and the special characters dollar sign ($), underscore
(_), and hyphen (-).
1 = AutoCAD 2000. Names can be up to 255 characters in
length, and can include the letters A to Z, the numerals 0
to 9, spaces, and any special characters not used for other
purposes by Microsoft Windows and AutoCAD
$FILLETRAD 40 Fillet radius
$FILLMODE 70 Fill mode on if nonzero
$FINGERPRINTGUID 2 Set at creation time, uniquely identifies a particular drawing
22 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
Specifies a gap to be displayed where an object is hidden
by another object; the value is specified as a percent of one
$HALOGAP 280
unit and is independent of the zoom level. A haloed line is
shortened at the point where it is hidden when HIDE or
the Hidden option of SHADEMODE is used
$HANDSEED 5 Next available handle
$HIDETEXT 290 Specifies HIDETEXT system variable:
0 = HIDE ignores text objects when producing the hidden
view
1 = HIDE does not ignore text objects
Path for all relative hyperlinks in the drawing. If null, the
drawing path is used
$HYPERLINKBASE 1
Controls whether layer and spatial indexes are created and
saved in drawing files:
$INDEXCTL 280
0 = No indexes are created
1 = Layer index is created
2 = Spatial index is created
3 = Layer and spatial indexes are created
$INSBASE 10, 20, 30 Insertion base set by BASE command (in WCS)
$INSUNITS 70 Default drawing units for AutoCAD DesignCenter blocks:
0 = Unitless; 1 = Inches; 2 = Feet; 3 = Miles; 4 = Millimeters;
5 = Centimeters; 6 = Meters; 7 = Kilometers; 8 = Microinches;
9 = Mils; 10 = Yards; 11 = Angstroms; 12 = Nanometers;
13 = Microns; 14 = Decimeters; 15 = Decameters;
16 = Hectometers; 17 = Gigameters; 18 = Astronomical
units;
19 = Light years; 20 = Parsecs
HEADER Section Group Codes | 23
DXF header variables
Variable Group code Description
Represents the ACI color index of the interference objects
created during the interfere command.Default value is 1
$INTERFERECOLOR 62
Hard-pointer ID to the visual style for interference objects.
Default visual style is Conceptual.
$INTERFEREOBJVS 345
Hard-pointer ID to the visual style for the viewport during
interference checking.Default visual style is 3d Wireframe.
$INTERFEREVPVS 346
$INTERSECTIONCOLOR 70 Specifies the entity color of intersection polylines:
Values 1-255 designate an AutoCAD color index (ACI)
0 = Color BYBLOCK
256 = Color BYLAYER
257 = Color BYENTITY
$INTERSECTIONDISPLAY 290 Specifies the display of intersection polylines:
0 = Turns off the display of intersection polylines
1 = Turns on the display of intersection polylines
$JOINSTYLE 280 Lineweight joint setting for new objects:
0=none; 1= round; 2 = angle; 3 = flat
$LIMCHECK 70 Nonzero if limits checking is on
$LIMMAX 10, 20 XY drawing limits upper-right corner(in WCS)
$LIMMIN 10, 20 XY drawing limits lower-left corner(in WCS)
$LTSCALE 40 Global linetype scale
$LUNITS 70 Units format for coordinates and distances
$LUPREC 70 Units precision for coordinates and distances
24 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
Controls the display of lineweights on the Model or Layout
tab:
$LWDISPLAY 290
0 = Lineweight is not displayed
1 = Lineweight is displayed
$MAXACTVP 70 Sets maximum number of viewports to be regenerated
$MEASUREMENT 70 Sets drawing units: 0 = English; 1 = Metric
$MENU 1 Name of menu file
$MIRRTEXT 70 Mirror text if nonzero
Specifies the color of obscured lines.An obscured line is a
hidden line made visible by changing its color and linetype
$OBSCOLOR 70
and is visible only when the HIDE or SHADEMODE command
is used.The OBSCUREDCOLOR setting is visible only
if the OBSCUREDLTYPE is turned ON by setting it to a value
other than 0.
0 and 256 = Entity color
1-255 = An AutoCAD color index(ACI)
Specifies the linetype of obscured lines.Obscured linetypes
are independent of zoom level, unlike regular AutoCAD
$OBSLTYPE 280
linetypes.Value 0 turns off display of obscured lines and is
the default. Linetype values are defined as follows:
0 = Off
1 = Solid
2 = Dashed
3 = Dotted
4 = Short Dash
5 = Medium Dash
6 = Long Dash
7 = Double Short Dash
8 = Double Medium Dash
9 = Double Long Dash
10 = Medium Long Dash
HEADER Section Group Codes | 25
DXF header variables
Variable Group code Description
11 = Sparse Dot
$ORTHOMODE 70 Ortho mode on if nonzero
$PDMODE 70 Point display mode
$PDSIZE 40 Point display size
$PELEVATION 40 Current paper space elevation
$PEXTMAX 10, 20, 30 Maximum X, Y, and Z extents for paper space
$PEXTMIN 10, 20, 30 Minimum X, Y, and Z extents for paper space
$PINSBASE 10, 20, 30 Paper space insertion base point
$PLIMCHECK 70 Limits checking in paper space when nonzero
$PLIMMAX 10, 20 Maximum X and Y limits in paper space
$PLIMMIN 10, 20 Minimum X and Y limits in paper space
Governs the generation of linetype patterns around the
vertices of a 2D polyline:
$PLINEGEN 70
1 = Linetype is generated in a continuous pattern around
vertices of the polyline
0 = Each segment of the polyline starts and ends with a
dash
$PLINEWID 40 Default polyline width
Assigns a project name to the current drawing.Used when
an external reference or image is not found on its original
$PROJECTNAME 1
path.The project name points to a section in the registry
that can contain one or more search paths for each project
26 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
name defined.Project names and their search directories
are created from the Files tab of the Options dialog box
$PROXYGRAPHICS 70 Controls the saving of proxy object images
$PSLTSCALE 70 Controls paper space linetype scaling:
1 = No special linetype scaling
0 = Viewport scaling governs linetype scaling
Indicates whether the current drawing is in a Color-Dependent
or Named Plot Style mode:
$PSTYLEMODE 290
0 = Uses named plot style tables in the current drawing
1 = Uses color-dependent plot style tables in the current
drawing
$PSVPSCALE 40 View scale factor for new viewports:
0 = Scaled to fit
>0 = Scale factor(a positive real value)
Name of the UCS that defines the origin and orientation
of orthographic UCS settings(paper space only)
$PUCSBASE 2
$PUCSNAME 2 Current paper space UCS name
$PUCSORG 10, 20, 30 Current paper space UCS origin
Point which becomes the new UCS origin after changing
paper space UCS to BACK when PUCSBASE is set to WORLD
$PUCSORGBACK 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to BOTTOM when PUCSBASE is set to
WORLD
$PUCSORGBOTTOM 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to FRONT when PUCSBASE is set to
WORLD
$PUCSORGFRONT 10, 20, 30
HEADER Section Group Codes | 27
DXF header variables
Variable Group code Description
Point which becomes the new UCS origin after changing
paper space UCS to LEFT when PUCSBASE is set to WORLD
$PUCSORGLEFT 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to RIGHT when PUCSBASE is set to
WORLD
$PUCSORGRIGHT 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to TOP when PUCSBASE is set to WORLD
$PUCSORGTOP 10, 20, 30
If paper space UCS is orthographic(PUCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to.If blank, UCS is relative to WORLD
$PUCSORTHOREF 2
$PUCSORTHOVIEW 70 Orthographic view type of paper space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
$PUCSXDIR 10, 20, 30 Current paper space UCS X axis
$PUCSYDIR 10, 20, 30 Current paper space UCS Y axis
$QTEXTMODE 70 Quick Text mode on if nonzero
$REGENMODE 70 REGENAUTO mode on if nonzero
$SHADEDGE 70 0 = Faces shaded, edges not highlighted
1 = Faces shaded, edges highlighted in black
2 = Faces not filled, edges in entity color
3 = Faces in entity color, edges in black
$SHADEDIF 70 Percent ambient/diffuse light; range 1-100; default 70
28 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
Location of the ground shadow plane.This is a Z axis ordinate.
$SHADOWPLANELOCATION 40
$SKETCHINC 40 Sketch record increment
$SKPOLY 70 0 = Sketch lines; 1 = Sketch polylines
Controls the object sorting methods; accessible from the
Options dialog box User Preferences tab.SORTENTS uses
the following bitcodes:
$SORTENTS 280
0 = Disables SORTENTS
1 = Sorts for object selection
2 = Sorts for object snap
4 = Sorts for redraws
8 = Sorts for MSLIDE command slide creation
16 = Sorts for REGEN commands
32 = Sorts for plotting
64 = Sorts for PostScript output
$SPLFRAME 70 Spline control polygon display: 1 = On; 0 = Off
$SPLINESEGS 70 Number of line segments per spline patch
$SPLINETYPE 70 Spline curve type for PEDIT Spline
$SURFTAB1 70 Number of mesh tabulations in first direction
$SURFTAB2 70 Number of mesh tabulations in second direction
$SURFTYPE 70 Surface type for PEDIT Smooth
$SURFU 70 Surface density(for PEDIT Smooth) in M direction
$SURFV 70 Surface density(for PEDIT Smooth) in N direction
HEADER Section Group Codes | 29
DXF header variables
Variable Group code Description
Local date/time of drawing creation(see “Special Handling
of Date/Time Variables”)
$TDCREATE 40
Cumulative editing time for this drawing(see “Special
Handling of Date/Time Variables”)
$TDINDWG 40
Universal date/time the drawing was created(see “Special
Handling of Date/Time Variables”)
$TDUCREATE 40
Local date/time of last drawing update(see “Special
Handling of Date/Time Variables”)
$TDUPDATE 40
$TDUSRTIMER 40 User-elapsed timer
Universal date/time of the last update/save(see “Special
Handling of Date/Time Variables”)
$TDUUPDATE 40
$TEXTSIZE 40 Default text height
$TEXTSTYLE 7 Current text style name
$THICKNESS 40 Current thickness set by ELEV command
$TILEMODE 70 1 for previous release compatibility mode; 0 otherwise
$TRACEWID 40 Default trace width
$TREEDEPTH 70 Specifies the maximum depth of the spatial index
Name of the UCS that defines the origin and orientation
of orthographic UCS settings
$UCSBASE 2
$UCSNAME 2 Name of current UCS
30 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$UCSORG 10, 20, 30 Origin of current UCS(in WCS)
Point which becomes the new UCS origin after changing
model space UCS to BACK when UCSBASE is set to WORLD
$UCSORGBACK 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to BOTTOM when UCSBASE is set to
WORLD
$UCSORGBOTTOM 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to FRONT when UCSBASE is set to
WORLD
$UCSORGFRONT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to LEFT when UCSBASE is set to WORLD
$UCSORGLEFT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to RIGHT when UCSBASE is set to WORLD
$UCSORGRIGHT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to TOP when UCSBASE is set to WORLD
$UCSORGTOP 10, 20, 30
If model space UCS is orthographic(UCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to.If blank, UCS is relative to WORLD
$UCSORTHOREF 2
$UCSORTHOVIEW 70 Orthographic view type of model space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
$UCSXDIR 10, 20, 30 Direction of the current UCS X axis(in WCS)
$UCSYDIR 10, 20, 30 Direction of the current UCS Y axis(in WCS)
HEADER Section Group Codes | 31
DXF header variables
Variable Group code Description
Low bit set = Display fractions, feet-and-inches, and surveyor's
angles in input format
$UNITMODE 70
Five integer variables intended for use by third-party developers
$USERI1 - 5 70
$USERR1 - 5 40 Five real variables intended for use by third-party developers
$USRTIMER 70 0 = Timer off; 1 = Timer on
Uniquely identifies a particular version of a drawing.Updated
when the drawing is modified
$VERSIONGUID 2
$VISRETAIN 70 0 = Don't retain xref-dependent visibility settings
1 = Retain xref-dependent visibility settings
$WORLDVIEW 70 1 = Set UCS to WCS during DVIEW/VPOINT
0 = Don't change UCS
$XCLIPFRAME 290 Controls the visibility of xref clipping boundaries:
0 = Clipping boundary is not visible
1 = Clipping boundary is visible
Controls whether the current drawing can be edited inplace
when being referenced by another drawing.
$XEDIT 290
0 = Can't use in-place reference editing";

        static string R2010Variables = @"$ACADMAINTVER 70 Maintenance version number (should be ignored)
$ACADVER 1 The AutoCAD drawing database version number:
AC1006 = R10;
AC1009 = R11 and R12;
AC1012 = R13; AC1014 = R14;
AC1015 = AutoCAD 2000;
AC1018 = AutoCAD 2004;
AC1021 = AutoCAD 2007;
AC1024 = AutoCAD 2010
$ANGBASE 50 Angle 0 direction
$ANGDIR 70 1 = Clockwise angles
0 = Counterclockwise angles
2
11
DXF header variables
Variable Group code Description
$ATTMODE 70 Attribute visibility:
0 = None
1 = Normal
2 = All
$AUNITS 70 Units format for angles
$AUPREC 70 Units precision for angles
$CECOLOR 62 Current entity color number:
0 = BYBLOCK; 256 = BYLAYER
$CELTSCALE 40 Current entity linetype scale
$CELTYPE 6 Entity linetype name, or BYBLOCK or BYLAYER
$CELWEIGHT 370 Lineweight of new objects
Plotstyle handle of new objects; if CEPSNTYPE is 3, then
this value indicates the handle
$CEPSNID 390
$CEPSNTYPE 380 Plot style type of new objects:
0 = Plot style by layer
1 = Plot style by block
2 = Plot style by dictionary default
3 = Plot style by object ID/handle
$CHAMFERA 40 First chamfer distance
$CHAMFERB 40 Second chamfer distance
$CHAMFERC 40 Chamfer length
$CHAMFERD 40 Chamfer angle
$CLAYER 8 Current layer name
$CMLJUST 70 Current multiline justification:
0 = Top; 1 = Middle; 2 = Bottom
12 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$CMLSCALE 40 Current multiline scale
$CMLSTYLE 2 Current multiline style name
$CSHADOW 280 Shadow mode for a 3D object:
0 = Casts and receives shadows
1 = Casts shadows
2 = Receives shadows
3 = Ignores shadows
$DIMADEC 70 Number of precision places displayed in angular dimensions
$DIMALT 70 Alternate unit dimensioning performed if nonzero
$DIMALTD 70 Alternate unit decimal places
$DIMALTF 40 Alternate unit scale factor
$DIMALTRND 40 Determines rounding of alternate units
Number of decimal places for tolerance values of an alternate
units dimension
$DIMALTTD 70
$DIMALTTZ 70 Controls suppression of zeros for alternate tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
Units format for alternate units of all dimension style family
members except angular:
$DIMALTU 70
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural (stacked); 5 = Fractional (stacked);
6 = Architectural; 7 = Fractional
Controls suppression of zeros for alternate unit dimension
values:
$DIMALTZ 70
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
HEADER Section Group Codes | 13
DXF header variables
Variable Group code Description
3 = Includes zero inches and suppresses zero feet
$DIMAPOST 1 Alternate dimensioning suffix
$DIMASO 70 1 = Create associative dimensioning
0 = Draw individual entities
$DIMASSOC 280 Controls the associativity of dimension objects
0 = Creates exploded dimensions; there is no association
between elements of the dimension, and the lines, arcs,
arrowheads, and text of a dimension are drawn as separate
objects
1 = Creates non-associative dimension objects; the elements
of the dimension are formed into a single object, and if the
definition point on the object moves, then the dimension
value is updated
2 = Creates associative dimension objects; the elements of
the dimension are formed into a single object and one or
more definition points of the dimension are coupled with
association points on geometric objects
$DIMASZ 40 Dimensioning arrow size
Controls dimension text and arrow placement when space
is not sufficient to place both within the extension lines:
$DIMATFIT 70
0 = Places both text and arrows outside extension lines
1 = Moves arrows first, then text
2 = Moves text first, then arrows
3 = Moves either text or arrows, whichever fits best
AutoCAD adds a leader to moved dimension text when
DIMTMOVE is set to 1
$DIMAUNIT 70 Angle format for angular dimensions:
0 = Decimal degrees; 1 = Degrees/minutes/seconds;
2 = Gradians; 3 = Radians; 4 = Surveyor's units
$DIMAZIN 70 Controls suppression of zeros for angular dimensions:
0 = Displays all leading and trailing zeros
1 = Suppresses leading zeros in decimal dimensions
2 = Suppresses trailing zeros in decimal dimensions
14 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
3 = Suppresses leading and trailing zeros
$DIMBLK 1 Arrow block name
$DIMBLK1 1 First arrow block name
$DIMBLK2 1 Second arrow block name
$DIMCEN 40 Size of center mark/lines
$DIMCLRD 70 Dimension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMCLRE 70 Dimension extension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMCLRT 70 Dimension text color:
range is 0 = BYBLOCK; 256 = BYLAYER
Number of decimal places for the tolerance values of a
primary units dimension
$DIMDEC 70
$DIMDLE 40 Dimension line extension
$DIMDLI 40 Dimension line increment
Single-character decimal separator used when creating dimensions
whose unit format is decimal
$DIMDSEP 70
$DIMEXE 40 Extension line extension
$DIMEXO 40 Extension line offset
Scale factor used to calculate the height of text for dimension
fractions and tolerances. AutoCAD multiplies DIMTXT
by DIMTFAC to set the fractional or tolerance text height
$DIMFAC 40
$DIMGAP 40 Dimension line gap
$DIMJUST 70 Horizontal dimension text position:
HEADER Section Group Codes | 15
DXF header variables
Variable Group code Description
0 = Above dimension line and center-justified between extension
lines
1 = Above dimension line and next to first extension line
2 = Above dimension line and next to second extension
line
3 = Above and center-justified to first extension line
4 = Above and center-justified to second extension line
$DIMLDRBLK 1 Arrow block name for leaders
$DIMLFAC 40 Linear measurements scale factor
$DIMLIM 70 Dimension limits generated if nonzero
$DIMLUNIT 70 Sets units for all dimension types except Angular:
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural; 5 = Fractional; 6 = Windows desktop
$DIMLWD 70 Dimension line lineweight:
-3 = Standard
-2 = ByLayer
-1 = ByBlock
0-211 = an integer representing 100th of mm
$DIMLWE 70 Extension line lineweight:
-3 = Standard
-2 = ByLayer
-1 = ByBlock
0-211 = an integer representing 100th of mm
$DIMPOST 1 General dimensioning suffix
$DIMRND 40 Rounding value for dimension distances
$DIMSAH 70 Use separate arrow blocks if nonzero
$DIMSCALE 40 Overall dimensioning scale factor
$DIMSD1 70 Suppression of first extension line:
0 = Not suppressed; 1 = Suppressed
16 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$DIMSD2 70 Suppression of second extension line:
0 = Not suppressed; 1 = Suppressed
$DIMSE1 70 First extension line suppressed if nonzero
$DIMSE2 70 Second extension line suppressed if nonzero
$DIMSHO 70 1 = Recompute dimensions while dragging
0 = Drag original image
$DIMSOXD 70 Suppress outside-extensions dimension lines if nonzero
$DIMSTYLE 2 Dimension style name
$DIMTAD 70 Text above dimension line if nonzero
$DIMTDEC 70 Number of decimal places to display the tolerance values
$DIMTFAC 40 Dimension tolerance display scale factor
$DIMTIH 70 Text inside horizontal if nonzero
$DIMTIX 70 Force text inside extensions if nonzero
$DIMTM 40 Minus tolerance
$DIMTMOVE 70 Dimension text movement rules:
0 = Moves the dimension line with dimension text
1 = Adds a leader when dimension text is moved
2 = Allows text to be moved freely without a leader
If text is outside extensions, force line extensions between
extensions if nonzero
$DIMTOFL 70
$DIMTOH 70 Text outside horizontal if nonzero
$DIMTOL 70 Dimension tolerances generated if nonzero
$DIMTOLJ 70 Vertical justification for tolerance values:
0 = Top; 1 = Middle; 2 = Bottom
HEADER Section Group Codes | 17
DXF header variables
Variable Group code Description
$DIMTP 40 Plus tolerance
$DIMTSZ 40 Dimensioning tick size:
0 = No ticks
$DIMTVP 40 Text vertical position
$DIMTXSTY 7 Dimension text style
$DIMTXT 40 Dimensioning text height
$DIMTZIN 70 Controls suppression of zeros for tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMUPT 70 Cursor functionality for user-positioned text:
0 = Controls only the dimension line location
1 = Controls the text position as well as the dimension line
location
$DIMZIN 70 Controls suppression of zeros for primary unit values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
Controls the display of silhouette curves of body objects in
Wireframe mode:
$DISPSILH 70
0 = Off; 1 = On
Hard-pointer ID to visual style while creating 3D solid
primitives. The defualt value is NULL
$DRAGVS 349
Drawing code page; set to the system code page when a
new drawing is created, but not otherwise maintained by
AutoCAD
$DWGCODEPAGE 3
$ELEVATION 40 Current elevation set by ELEV command
18 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$ENDCAPS 280 Lineweight endcaps setting for new objects:
0 = none; 1 = round; 2 = angle; 3 = square
$EXTMAX 10, 20, 30 X, Y, and Z drawing extents upper-right corner (in WCS)
$EXTMIN 10, 20, 30 X, Y, and Z drawing extents lower-left corner (in WCS)
$EXTNAMES 290 Controls symbol table naming:
0 = Release 14 compatibility. Limits names to 31 characters
in length. Names can include the letters A to Z, the numerals
0 to 9, and the special characters dollar sign ($), underscore
(_), and hyphen (-).
1 = AutoCAD 2000. Names can be up to 255 characters in
length, and can include the letters A to Z, the numerals 0
to 9, spaces, and any special characters not used for other
purposes by Microsoft Windows and AutoCAD
$FILLETRAD 40 Fillet radius
$FILLMODE 70 Fill mode on if nonzero
$FINGERPRINTGUID 2 Set at creation time, uniquely identifies a particular drawing
Specifies a gap to be displayed where an object is hidden
by another object; the value is specified as a percent of one
$HALOGAP 280
unit and is independent of the zoom level. A haloed line is
shortened at the point where it is hidden when HIDE or
the Hidden option of SHADEMODE is used
$HANDSEED 5 Next available handle
$HIDETEXT 290 Specifies HIDETEXT system variable:
0 = HIDE ignores text objects when producing the hidden
view
1 = HIDE does not ignore text objects
Path for all relative hyperlinks in the drawing. If null, the
drawing path is used
$HYPERLINKBASE 1
HEADER Section Group Codes | 19
DXF header variables
Variable Group code Description
Controls whether layer and spatial indexes are created and
saved in drawing files:
$INDEXCTL 280
0 = No indexes are created
1 = Layer index is created
2 = Spatial index is created
3 = Layer and spatial indexes are created
$INSBASE 10, 20, 30 Insertion base set by BASE command (in WCS)
$INSUNITS 70 Default drawing units for AutoCAD DesignCenter blocks:
0 = Unitless; 1 = Inches; 2 = Feet; 3 = Miles; 4 = Millimeters;
5 = Centimeters; 6 = Meters; 7 = Kilometers; 8 = Microinches;
9 = Mils; 10 = Yards; 11 = Angstroms; 12 = Nanometers;
13 = Microns; 14 = Decimeters; 15 = Decameters;
16 = Hectometers; 17 = Gigameters; 18 = Astronomical
units;
19 = Light years; 20 = Parsecs
Represents the ACI color index of the interference objects
created during the interfere command.Default value is 1
$INTERFERECOLOR 62
Hard-pointer ID to the visual style for interference objects.
Default visual style is Conceptual.
$INTERFEREOBJVS 345
Hard-pointer ID to the visual style for the viewport during
interference checking.Default visual style is 3d Wireframe.
$INTERFEREVPVS 346
$INTERSECTIONCOLOR 70 Specifies the entity color of intersection polylines:
Values 1-255 designate an AutoCAD color index (ACI)
0 = Color BYBLOCK
256 = Color BYLAYER
257 = Color BYENTITY
$INTERSECTIONDISPLAY 290 Specifies the display of intersection polylines:
0 = Turns off the display of intersection polylines
1 = Turns on the display of intersection polylines
$JOINSTYLE 280 Lineweight joint setting for new objects:
0=none; 1= round; 2 = angle; 3 = flat
20 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$LIMCHECK 70 Nonzero if limits checking is on
$LIMMAX 10, 20 XY drawing limits upper-right corner(in WCS)
$LIMMIN 10, 20 XY drawing limits lower-left corner(in WCS)
$LTSCALE 40 Global linetype scale
$LUNITS 70 Units format for coordinates and distances
$LUPREC 70 Units precision for coordinates and distances
Controls the display of lineweights on the Model or Layout
tab:
$LWDISPLAY 290
0 = Lineweight is not displayed
1 = Lineweight is displayed
$MAXACTVP 70 Sets maximum number of viewports to be regenerated
$MEASUREMENT 70 Sets drawing units: 0 = English; 1 = Metric
$MENU 1 Name of menu file
$MIRRTEXT 70 Mirror text if nonzero
Specifies the color of obscured lines.An obscured line is a
hidden line made visible by changing its color and linetype
$OBSCOLOR 70
and is visible only when the HIDE or SHADEMODE command
is used.The OBSCUREDCOLOR setting is visible only
if the OBSCUREDLTYPE is turned ON by setting it to a value
other than 0.
0 and 256 = Entity color
1-255 = An AutoCAD color index(ACI)
Specifies the linetype of obscured lines.Obscured linetypes
are independent of zoom level, unlike regular AutoCAD
$OBSLTYPE 280
linetypes.Value 0 turns off display of obscured lines and is
the default. Linetype values are defined as follows:
0 = Off
1 = Solid
HEADER Section Group Codes | 21
DXF header variables
Variable Group code Description
2 = Dashed
3 = Dotted
4 = Short Dash
5 = Medium Dash
6 = Long Dash
7 = Double Short Dash
8 = Double Medium Dash
9 = Double Long Dash
10 = Medium Long Dash
11 = Sparse Dot
$ORTHOMODE 70 Ortho mode on if nonzero
$PDMODE 70 Point display mode
$PDSIZE 40 Point display size
$PELEVATION 40 Current paper space elevation
$PEXTMAX 10, 20, 30 Maximum X, Y, and Z extents for paper space
$PEXTMIN 10, 20, 30 Minimum X, Y, and Z extents for paper space
$PINSBASE 10, 20, 30 Paper space insertion base point
$PLIMCHECK 70 Limits checking in paper space when nonzero
$PLIMMAX 10, 20 Maximum X and Y limits in paper space
$PLIMMIN 10, 20 Minimum X and Y limits in paper space
Governs the generation of linetype patterns around the
vertices of a 2D polyline:
$PLINEGEN 70
1 = Linetype is generated in a continuous pattern around
vertices of the polyline
0 = Each segment of the polyline starts and ends with a
dash
$PLINEWID 40 Default polyline width
22 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
Assigns a project name to the current drawing.Used when
an external reference or image is not found on its original
$PROJECTNAME 1
path.The project name points to a section in the registry
that can contain one or more search paths for each project
name defined. Project names and their search directories
are created from the Files tab of the Options dialog box
$PROXYGRAPHICS 70 Controls the saving of proxy object images
$PSLTSCALE 70 Controls paper space linetype scaling:
1 = No special linetype scaling
0 = Viewport scaling governs linetype scaling
Indicates whether the current drawing is in a Color-Dependent
or Named Plot Style mode:
$PSTYLEMODE 290
0 = Uses named plot style tables in the current drawing
1 = Uses color-dependent plot style tables in the current
drawing
$PSVPSCALE 40 View scale factor for new viewports:
0 = Scaled to fit
>0 = Scale factor(a positive real value)
Name of the UCS that defines the origin and orientation
of orthographic UCS settings(paper space only)
$PUCSBASE 2
$PUCSNAME 2 Current paper space UCS name
$PUCSORG 10, 20, 30 Current paper space UCS origin
Point which becomes the new UCS origin after changing
paper space UCS to BACK when PUCSBASE is set to WORLD
$PUCSORGBACK 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to BOTTOM when PUCSBASE is set to
WORLD
$PUCSORGBOTTOM 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to FRONT when PUCSBASE is set to
WORLD
$PUCSORGFRONT 10, 20, 30
HEADER Section Group Codes | 23
DXF header variables
Variable Group code Description
Point which becomes the new UCS origin after changing
paper space UCS to LEFT when PUCSBASE is set to WORLD
$PUCSORGLEFT 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to RIGHT when PUCSBASE is set to
WORLD
$PUCSORGRIGHT 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to TOP when PUCSBASE is set to WORLD
$PUCSORGTOP 10, 20, 30
If paper space UCS is orthographic(PUCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to.If blank, UCS is relative to WORLD
$PUCSORTHOREF 2
$PUCSORTHOVIEW 70 Orthographic view type of paper space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
$PUCSXDIR 10, 20, 30 Current paper space UCS X axis
$PUCSYDIR 10, 20, 30 Current paper space UCS Y axis
$QTEXTMODE 70 Quick Text mode on if nonzero
$REGENMODE 70 REGENAUTO mode on if nonzero
$SHADEDGE 70 0 = Faces shaded, edges not highlighted
1 = Faces shaded, edges highlighted in black
2 = Faces not filled, edges in entity color
3 = Faces in entity color, edges in black
$SHADEDIF 70 Percent ambient/diffuse light; range 1-100; default 70
Location of the ground shadow plane.This is a Z axis ordinate.
$SHADOWPLANELOCA- 40
TION
$SKETCHINC 40 Sketch record increment
24 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$SKPOLY 70 0 = Sketch lines; 1 = Sketch polylines
Controls the object sorting methods; accessible from the
Options dialog box User Preferences tab.SORTENTS uses
the following bitcodes:
$SORTENTS 280
0 = Disables SORTENTS
1 = Sorts for object selection
2 = Sorts for object snap
4 = Sorts for redraws
8 = Sorts for MSLIDE command slide creation
16 = Sorts for REGEN commands
32 = Sorts for plotting
64 = Sorts for PostScript output
$SPLFRAME 70 Spline control polygon display: 1 = On; 0 = Off
$SPLINESEGS 70 Number of line segments per spline patch
$SPLINETYPE 70 Spline curve type for PEDIT Spline
$SURFTAB1 70 Number of mesh tabulations in first direction
$SURFTAB2 70 Number of mesh tabulations in second direction
$SURFTYPE 70 Surface type for PEDIT Smooth
$SURFU 70 Surface density(for PEDIT Smooth) in M direction
$SURFV 70 Surface density(for PEDIT Smooth) in N direction
Local date/time of drawing creation(see “Special Handling
of Date/Time Variables”)
$TDCREATE 40
Cumulative editing time for this drawing(see “Special
Handling of Date/Time Variables”)
$TDINDWG 40
Universal date/time the drawing was created(see “Special
Handling of Date/Time Variables”)
$TDUCREATE 40
HEADER Section Group Codes | 25
DXF header variables
Variable Group code Description
Local date/time of last drawing update(see “Special
Handling of Date/Time Variables”)
$TDUPDATE 40
$TDUSRTIMER 40 User-elapsed timer
Universal date/time of the last update/save(see “Special
Handling of Date/Time Variables”)
$TDUUPDATE 40
$TEXTSIZE 40 Default text height
$TEXTSTYLE 7 Current text style name
$THICKNESS 40 Current thickness set by ELEV command
$TILEMODE 70 1 for previous release compatibility mode; 0 otherwise
$TRACEWID 40 Default trace width
$TREEDEPTH 70 Specifies the maximum depth of the spatial index
Name of the UCS that defines the origin and orientation
of orthographic UCS settings
$UCSBASE 2
$UCSNAME 2 Name of current UCS
$UCSORG 10, 20, 30 Origin of current UCS(in WCS)
Point which becomes the new UCS origin after changing
model space UCS to BACK when UCSBASE is set to WORLD
$UCSORGBACK 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to BOTTOM when UCSBASE is set to
WORLD
$UCSORGBOTTOM 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to FRONT when UCSBASE is set to
WORLD
$UCSORGFRONT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to LEFT when UCSBASE is set to WORLD
$UCSORGLEFT 10, 20, 30
26 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
Point which becomes the new UCS origin after changing
model space UCS to RIGHT when UCSBASE is set to WORLD
$UCSORGRIGHT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to TOP when UCSBASE is set to WORLD
$UCSORGTOP 10, 20, 30
If model space UCS is orthographic(UCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to.If blank, UCS is relative to WORLD
$UCSORTHOREF 2
$UCSORTHOVIEW 70 Orthographic view type of model space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
$UCSXDIR 10, 20, 30 Direction of the current UCS X axis(in WCS)
$UCSYDIR 10, 20, 30 Direction of the current UCS Y axis(in WCS)
Low bit set = Display fractions, feet-and-inches, and surveyor's
angles in input format
$UNITMODE 70
Five integer variables intended for use by third-party developers
$USERI1 - 5 70
$USERR1 - 5 40 Five real variables intended for use by third-party developers
$USRTIMER 70 0 = Timer off; 1 = Timer on
Uniquely identifies a particular version of a drawing.Updated
when the drawing is modified
$VERSIONGUID 2
$VISRETAIN 70 0 = Don't retain xref-dependent visibility settings
1 = Retain xref-dependent visibility settings
$WORLDVIEW 70 1 = Set UCS to WCS during DVIEW/VPOINT
0 = Don't change UCS
$XCLIPFRAME 290 Controls the visibility of xref clipping boundaries:
HEADER Section Group Codes | 27
DXF header variables
Variable Group code Description
0 = Clipping boundary is not visible
1 = Clipping boundary is visible
Controls whether the current drawing can be edited inplace
when being referenced by another drawing.
$XEDIT 290
0 = Can't use in-place reference editing
1 = Can use in-place reference editing";

        static string R2013Variables = @"$ACADMAINTVER 70 Maintenance version number (should be ignored)
$ACADVER 1 The AutoCAD drawing database version number:
AC1006 = R10
AC1009 = R11 and R12
AC1012 = R13
AC1014 = R14
AC1015 = AutoCAD 2000
AC1018 = AutoCAD 2004
AC1021 = AutoCAD 2007
AC1024 = AutoCAD 2010
AC1027 = AutoCAD 2013
2
13
DXF header variables
Variable Group code Description
$ANGBASE 50 Angle 0 direction
$ANGDIR 70 1 = Clockwise angles
0 = Counterclockwise angles
$ATTMODE 70 Attribute visibility:
0 = None
1 = Normal
2 = All
$AUNITS 70 Units format for angles
$AUPREC 70 Units precision for angles
$CECOLOR 62 Current entity color number:
0 = BYBLOCK; 256 = BYLAYER
$CELTSCALE 40 Current entity linetype scale
$CELTYPE 6 Entity linetype name, or BYBLOCK or BYLAYER
$CELWEIGHT 370 Lineweight of new objects
Plotstyle handle of new objects; if CEPSNTYPE is 3, then this
value indicates the handle
$CEPSNID 390
$CEPSNTYPE 380 Plot style type of new objects:
0 = Plot style by layer
1 = Plot style by block
2 = Plot style by dictionary default
3 = Plot style by object ID/handle
$CHAMFERA 40 First chamfer distance
14 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$CHAMFERB 40 Second chamfer distance
$CHAMFERC 40 Chamfer length
$CHAMFERD 40 Chamfer angle
$CLAYER 8 Current layer name
$CMLJUST 70 Current multiline justification:
0 = Top; 1 = Middle; 2 = Bottom
$CMLSCALE 40 Current multiline scale
$CMLSTYLE 2 Current multiline style name
$CSHADOW 280 Shadow mode for a 3D object:
0 = Casts and receives shadows
1 = Casts shadows
2 = Receives shadows
3 = Ignores shadows
$DIMADEC 70 Number of precision places displayed in angular dimensions
$DIMALT 70 Alternate unit dimensioning performed if nonzero
$DIMALTD 70 Alternate unit decimal places
$DIMALTF 40 Alternate unit scale factor
$DIMALTRND 40 Determines rounding of alternate units
Number of decimal places for tolerance values of an alternate
units dimension
$DIMALTTD 70
HEADER Section Group Codes (DXF) | 15
DXF header variables
Variable Group code Description
$DIMALTTZ 70 Controls suppression of zeros for alternate tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
Units format for alternate units of all dimension style family
members except angular:
$DIMALTU 70
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural (stacked); 5 = Fractional (stacked);
6 = Architectural; 7 = Fractional
Controls suppression of zeros for alternate unit dimension
values:
$DIMALTZ 70
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMAPOST 1 Alternate dimensioning suffix
$DIMASO 70 1 = Create associative dimensioning
0 = Draw individual entities
$DIMASSOC 280 Controls the associativity of dimension objects
0 = Creates exploded dimensions; there is no association
between elements of the dimension, and the lines, arcs,
arrowheads, and text of a dimension are drawn as separate
objects
1 = Creates non-associative dimension objects; the elements
of the dimension are formed into a single object, and if the
definition point on the object moves, then the dimension
value is updated
2 = Creates associative dimension objects; the elements of
the dimension are formed into a single object and one or
more definition points of the dimension are coupled with
association points on geometric objects
16 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$DIMASZ 40 Dimensioning arrow size
Controls dimension text and arrow placement when space
is not sufficient to place both within the extension lines:
$DIMATFIT 70
0 = Places both text and arrows outside extension lines
1 = Moves arrows first, then text
2 = Moves text first, then arrows
3 = Moves either text or arrows, whichever fits best
AutoCAD adds a leader to moved dimension text when
DIMTMOVE is set to 1
$DIMAUNIT 70 Angle format for angular dimensions:
0 = Decimal degrees; 1 = Degrees/minutes/seconds;
2 = Gradians; 3 = Radians; 4 = Surveyor's units
$DIMAZIN 70 Controls suppression of zeros for angular dimensions:
0 = Displays all leading and trailing zeros
1 = Suppresses leading zeros in decimal dimensions
2 = Suppresses trailing zeros in decimal dimensions
3 = Suppresses leading and trailing zeros
$DIMBLK 1 Arrow block name
$DIMBLK1 1 First arrow block name
$DIMBLK2 1 Second arrow block name
$DIMCEN 40 Size of center mark/lines
$DIMCLRD 70 Dimension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
$DIMCLRE 70 Dimension extension line color:
range is 0 = BYBLOCK; 256 = BYLAYER
HEADER Section Group Codes (DXF) | 17
DXF header variables
Variable Group code Description
$DIMCLRT 70 Dimension text color:
range is 0 = BYBLOCK; 256 = BYLAYER
Number of decimal places for the tolerance values of a
primary units dimension
$DIMDEC 70
$DIMDLE 40 Dimension line extension
$DIMDLI 40 Dimension line increment
Single-character decimal separator used when creating dimensions
whose unit format is decimal
$DIMDSEP 70
$DIMEXE 40 Extension line extension
$DIMEXO 40 Extension line offset
Scale factor used to calculate the height of text for dimension
fractions and tolerances. AutoCAD multiplies DIMTXT
by DIMTFAC to set the fractional or tolerance text height
$DIMFAC 40
$DIMGAP 40 Dimension line gap
$DIMJUST 70 Horizontal dimension text position:
0 = Above dimension line and center-justified between extension
lines
1 = Above dimension line and next to first extension line
2 = Above dimension line and next to second extension line
3 = Above and center-justified to first extension line
4 = Above and center-justified to second extension line
$DIMLDRBLK 1 Arrow block name for leaders
$DIMLFAC 40 Linear measurements scale factor
18 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$DIMLIM 70 Dimension limits generated if nonzero
$DIMLUNIT 70 Sets units for all dimension types except Angular:
1 = Scientific; 2 = Decimal; 3 = Engineering;
4 = Architectural; 5 = Fractional; 6 = Windows desktop
$DIMLWD 70 Dimension line lineweight:
-3 = Standard
-2 = ByLayer
-1 = ByBlock
0-211 = an integer representing 100th of mm
$DIMLWE 70 Extension line lineweight:
-3 = Standard
-2 = ByLayer
-1 = ByBlock
0-211 = an integer representing 100th of mm
$DIMPOST 1 General dimensioning suffix
$DIMRND 40 Rounding value for dimension distances
$DIMSAH 70 Use separate arrow blocks if nonzero
$DIMSCALE 40 Overall dimensioning scale factor
$DIMSD1 70 Suppression of first extension line:
0 = Not suppressed; 1 = Suppressed
$DIMSD2 70 Suppression of second extension line:
0 = Not suppressed; 1 = Suppressed
$DIMSE1 70 First extension line suppressed if nonzero
$DIMSE2 70 Second extension line suppressed if nonzero
HEADER Section Group Codes (DXF) | 19
DXF header variables
Variable Group code Description
$DIMSHO 70 1 = Recompute dimensions while dragging
0 = Drag original image
$DIMSOXD 70 Suppress outside-extensions dimension lines if nonzero
$DIMSTYLE 2 Dimension style name
$DIMTAD 70 Text above dimension line if nonzero
$DIMTDEC 70 Number of decimal places to display the tolerance values
$DIMTFAC 40 Dimension tolerance display scale factor
$DIMTIH 70 Text inside horizontal if nonzero
$DIMTIX 70 Force text inside extensions if nonzero
$DIMTM 40 Minus tolerance
$DIMTMOVE 70 Dimension text movement rules:
0 = Moves the dimension line with dimension text
1 = Adds a leader when dimension text is moved
2 = Allows text to be moved freely without a leader
If text is outside extensions, force line extensions between
extensions if nonzero
$DIMTOFL 70
$DIMTOH 70 Text outside horizontal if nonzero
$DIMTOL 70 Dimension tolerances generated if nonzero
$DIMTOLJ 70 Vertical justification for tolerance values:
0 = Top; 1 = Middle; 2 = Bottom
20 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$DIMTP 40 Plus tolerance
$DIMTSZ 40 Dimensioning tick size:
0 = No ticks
$DIMTVP 40 Text vertical position
$DIMTXSTY 7 Dimension text style
$DIMTXT 40 Dimensioning text height
$DIMTZIN 70 Controls suppression of zeros for tolerance values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
$DIMUPT 70 Cursor functionality for user-positioned text:
0 = Controls only the dimension line location
1 = Controls the text position as well as the dimension line
location
$DIMZIN 70 Controls suppression of zeros for primary unit values:
0 = Suppresses zero feet and precisely zero inches
1 = Includes zero feet and precisely zero inches
2 = Includes zero feet and suppresses zero inches
3 = Includes zero inches and suppresses zero feet
Controls the display of silhouette curves of body objects in
Wireframe mode:
$DISPSILH 70
0 = Off; 1 = On
Hard-pointer ID to visual style while creating 3D solid
primitives. The defualt value is NULL
$DRAGVS 349
HEADER Section Group Codes (DXF) | 21
DXF header variables
Variable Group code Description
Drawing code page; set to the system code page when a
new drawing is created, but not otherwise maintained by
AutoCAD
$DWGCODEPAGE 3
$ELEVATION 40 Current elevation set by ELEV command
$ENDCAPS 280 Lineweight endcaps setting for new objects:
0 = none; 1 = round; 2 = angle; 3 = square
$EXTMAX 10, 20, 30 X, Y, and Z drawing extents upper-right corner (in WCS)
$EXTMIN 10, 20, 30 X, Y, and Z drawing extents lower-left corner (in WCS)
$EXTNAMES 290 Controls symbol table naming:
0 = Release 14 compatibility. Limits names to 31 characters
in length. Names can include the letters A to Z, the numerals
0 to 9, and the special characters dollar sign ($), underscore
(_), and hyphen (-).
1 = AutoCAD 2000. Names can be up to 255 characters in
length, and can include the letters A to Z, the numerals 0
to 9, spaces, and any special characters not used for other
purposes by Microsoft Windows and AutoCAD
$FILLETRAD 40 Fillet radius
$FILLMODE 70 Fill mode on if nonzero
$FINGERPRINTGUID 2 Set at creation time, uniquely identifies a particular drawing
Specifies a gap to be displayed where an object is hidden
by another object; the value is specified as a percent of one
$HALOGAP 280
unit and is independent of the zoom level. A haloed line is
shortened at the point where it is hidden when HIDE or the
Hidden option of SHADEMODE is used
22 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$HANDSEED 5 Next available handle
$HIDETEXT 290 Specifies HIDETEXT system variable:
0 = HIDE ignores text objects when producing the hidden
view
1 = HIDE does not ignore text objects
Path for all relative hyperlinks in the drawing. If null, the
drawing path is used
$HYPERLINKBASE 1
Controls whether layer and spatial indexes are created and
saved in drawing files:
$INDEXCTL 280
0 = No indexes are created
1 = Layer index is created
2 = Spatial index is created
3 = Layer and spatial indexes are created
$INSBASE 10, 20, 30 Insertion base set by BASE command (in WCS)
$INSUNITS 70 Default drawing units for AutoCAD DesignCenter blocks:
0 = Unitless; 1 = Inches; 2 = Feet; 3 = Miles; 4 = Millimeters;
5 = Centimeters; 6 = Meters; 7 = Kilometers; 8 = Microinches;
9 = Mils; 10 = Yards; 11 = Angstroms; 12 = Nanometers;
13 = Microns; 14 = Decimeters; 15 = Decameters;
16 = Hectometers; 17 = Gigameters; 18 = Astronomical
units;
19 = Light years; 20 = Parsecs
Represents the ACI color index of the interference objects
created during the interfere command.Default value is 1
$INTERFERECOLOR 62
Hard-pointer ID to the visual style for interference objects.
Default visual style is Conceptual.
$INTERFEREOBJVS 345
HEADER Section Group Codes (DXF) | 23
DXF header variables
Variable Group code Description
Hard-pointer ID to the visual style for the viewport during
interference checking.Default visual style is 3d Wireframe.
$INTERFEREVPVS 346
$INTERSECTIONCOLOR 70 Specifies the entity color of intersection polylines:
Values 1-255 designate an AutoCAD color index(ACI)
0 = Color BYBLOCK
256 = Color BYLAYER
257 = Color BYENTITY
$INTERSECTIONDISPLAY 290 Specifies the display of intersection polylines:
0 = Turns off the display of intersection polylines
1 = Turns on the display of intersection polylines
$JOINSTYLE 280 Lineweight joint setting for new objects:
0=none; 1= round; 2 = angle; 3 = flat
$LIMCHECK 70 Nonzero if limits checking is on
$LIMMAX 10, 20 XY drawing limits upper-right corner(in WCS)
$LIMMIN 10, 20 XY drawing limits lower-left corner(in WCS)
$LTSCALE 40 Global linetype scale
$LUNITS 70 Units format for coordinates and distances
$LUPREC 70 Units precision for coordinates and distances
Controls the display of lineweights on the Model or Layout
tab:
$LWDISPLAY 290
0 = Lineweight is not displayed
1 = Lineweight is displayed
$MAXACTVP 70 Sets maximum number of viewports to be regenerated
24 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$MEASUREMENT 70 Sets drawing units: 0 = English; 1 = Metric
$MENU 1 Name of menu file
$MIRRTEXT 70 Mirror text if nonzero
Specifies the color of obscured lines.An obscured line is a
hidden line made visible by changing its color and linetype
$OBSCOLOR 70
and is visible only when the HIDE or SHADEMODE command
is used.The OBSCUREDCOLOR setting is visible only
if the OBSCUREDLTYPE is turned ON by setting it to a value
other than 0.
0 and 256 = Entity color
1-255 = An AutoCAD color index(ACI)
Specifies the linetype of obscured lines.Obscured linetypes
are independent of zoom level, unlike regular AutoCAD
$OBSLTYPE 280
linetypes.Value 0 turns off display of obscured lines and is
the default. Linetype values are defined as follows:
0 = Off
1 = Solid
2 = Dashed
3 = Dotted
4 = Short Dash
5 = Medium Dash
6 = Long Dash
7 = Double Short Dash
8 = Double Medium Dash
9 = Double Long Dash
10 = Medium Long Dash
11 = Sparse Dot
$ORTHOMODE 70 Ortho mode on if nonzero
$PDMODE 70 Point display mode
HEADER Section Group Codes (DXF) | 25
DXF header variables
Variable Group code Description
$PDSIZE 40 Point display size
$PELEVATION 40 Current paper space elevation
$PEXTMAX 10, 20, 30 Maximum X, Y, and Z extents for paper space
$PEXTMIN 10, 20, 30 Minimum X, Y, and Z extents for paper space
$PINSBASE 10, 20, 30 Paper space insertion base point
$PLIMCHECK 70 Limits checking in paper space when nonzero
$PLIMMAX 10, 20 Maximum X and Y limits in paper space
$PLIMMIN 10, 20 Minimum X and Y limits in paper space
Governs the generation of linetype patterns around the
vertices of a 2D polyline:
$PLINEGEN 70
1 = Linetype is generated in a continuous pattern around
vertices of the polyline
0 = Each segment of the polyline starts and ends with a
dash
$PLINEWID 40 Default polyline width
Assigns a project name to the current drawing.Used when
an external reference or image is not found on its original
$PROJECTNAME 1
path.The project name points to a section in the registry
that can contain one or more search paths for each project
name defined.Project names and their search directories
are created from the Files tab of the Options dialog box
$PROXYGRAPHICS 70 Controls the saving of proxy object images
26 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$PSLTSCALE 70 Controls paper space linetype scaling:
1 = No special linetype scaling
0 = Viewport scaling governs linetype scaling
Indicates whether the current drawing is in a Color-Dependent
or Named Plot Style mode:
$PSTYLEMODE 290
0 = Uses named plot style tables in the current drawing
1 = Uses color-dependent plot style tables in the current
drawing
$PSVPSCALE 40 View scale factor for new viewports:
0 = Scaled to fit
>0 = Scale factor(a positive real value)
Name of the UCS that defines the origin and orientation of
orthographic UCS settings(paper space only)
$PUCSBASE 2
$PUCSNAME 2 Current paper space UCS name
$PUCSORG 10, 20, 30 Current paper space UCS origin
Point which becomes the new UCS origin after changing
paper space UCS to BACK when PUCSBASE is set to WORLD
$PUCSORGBACK 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to BOTTOM when PUCSBASE is set to
WORLD
$PUCSORGBOTTOM 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to FRONT when PUCSBASE is set to
WORLD
$PUCSORGFRONT 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to LEFT when PUCSBASE is set to WORLD
$PUCSORGLEFT 10, 20, 30
HEADER Section Group Codes(DXF) | 27
DXF header variables
Variable Group code Description
Point which becomes the new UCS origin after changing
paper space UCS to RIGHT when PUCSBASE is set to WORLD
$PUCSORGRIGHT 10, 20, 30
Point which becomes the new UCS origin after changing
paper space UCS to TOP when PUCSBASE is set to WORLD
$PUCSORGTOP 10, 20, 30
If paper space UCS is orthographic(PUCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to.If blank, UCS is relative to WORLD
$PUCSORTHOREF 2
$PUCSORTHOVIEW 70 Orthographic view type of paper space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
$PUCSXDIR 10, 20, 30 Current paper space UCS X axis
$PUCSYDIR 10, 20, 30 Current paper space UCS Y axis
$QTEXTMODE 70 Quick Text mode on if nonzero
$REGENMODE 70 REGENAUTO mode on if nonzero
$SHADEDGE 70 0 = Faces shaded, edges not highlighted
1 = Faces shaded, edges highlighted in black
2 = Faces not filled, edges in entity color
3 = Faces in entity color, edges in black
$SHADEDIF 70 Percent ambient/diffuse light; range 1-100; default 70
Location of the ground shadow plane.This is a Z axis ordinate.
$SHADOWPLANELOCA- 40
TION
$SKETCHINC 40 Sketch record increment
28 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
$SKPOLY 70 0 = Sketch lines; 1 = Sketch polylines
Controls the object sorting methods; accessible from the
Options dialog box User Preferences tab.SORTENTS uses
the following bitcodes:
$SORTENTS 280
0 = Disables SORTENTS
1 = Sorts for object selection
2 = Sorts for object snap
4 = Sorts for redraws
8 = Sorts for MSLIDE command slide creation
16 = Sorts for REGEN commands
32 = Sorts for plotting
64 = Sorts for PostScript output
$SPLINESEGS 70 Number of line segments per spline patch
$SPLINETYPE 70 Spline curve type for PEDIT Spline
$SURFTAB1 70 Number of mesh tabulations in first direction
$SURFTAB2 70 Number of mesh tabulations in second direction
$SURFTYPE 70 Surface type for PEDIT Smooth
$SURFU 70 Surface density(for PEDIT Smooth) in M direction
$SURFV 70 Surface density(for PEDIT Smooth) in N direction
Local date/time of drawing creation(see Special Handling
of Date/Time Variables)
$TDCREATE 40
Cumulative editing time for this drawing(see Special
Handling of Date/Time Variables)
$TDINDWG 40
HEADER Section Group Codes(DXF) | 29
DXF header variables
Variable Group code Description
Universal date/time the drawing was created(see Special
Handling of Date/Time Variables)
$TDUCREATE 40
Local date/time of last drawing update(see Special Handling
of Date/Time Variables)
$TDUPDATE 40
$TDUSRTIMER 40 User-elapsed timer
Universal date/time of the last update/save(see Special
Handling of Date/Time Variables)
$TDUUPDATE 40
$TEXTSIZE 40 Default text height
$TEXTSTYLE 7 Current text style name
$THICKNESS 40 Current thickness set by ELEV command
$TILEMODE 70 1 for previous release compatibility mode; 0 otherwise
$TRACEWID 40 Default trace width
$TREEDEPTH 70 Specifies the maximum depth of the spatial index
Name of the UCS that defines the origin and orientation of
orthographic UCS settings
$UCSBASE 2
$UCSNAME 2 Name of current UCS
$UCSORG 10, 20, 30 Origin of current UCS(in WCS)
Point which becomes the new UCS origin after changing
model space UCS to BACK when UCSBASE is set to WORLD
$UCSORGBACK 10, 20, 30
30 | Chapter 2 HEADER Section
DXF header variables
Variable Group code Description
Point which becomes the new UCS origin after changing
model space UCS to BOTTOM when UCSBASE is set to
WORLD
$UCSORGBOTTOM 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to FRONT when UCSBASE is set to WORLD
$UCSORGFRONT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to LEFT when UCSBASE is set to WORLD
$UCSORGLEFT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to RIGHT when UCSBASE is set to WORLD
$UCSORGRIGHT 10, 20, 30
Point which becomes the new UCS origin after changing
model space UCS to TOP when UCSBASE is set to WORLD
$UCSORGTOP 10, 20, 30
If model space UCS is orthographic(UCSORTHOVIEW not
equal to 0), this is the name of the UCS that the orthographic
UCS is relative to.If blank, UCS is relative to WORLD
$UCSORTHOREF 2
$UCSORTHOVIEW 70 Orthographic view type of model space UCS:
0 = UCS is not orthographic;
1 = Top; 2 = Bottom;
3 = Front; 4 = Back;
5 = Left; 6 = Right
$UCSXDIR 10, 20, 30 Direction of the current UCS X axis(in WCS)
$UCSYDIR 10, 20, 30 Direction of the current UCS Y axis(in WCS)
Low bit set = Display fractions, feet-and-inches, and surveyor's
angles in input format
$UNITMODE 70
Five integer variables intended for use by third-party developers
$USERI1 - 5 70
HEADER Section Group Codes(DXF) | 31
DXF header variables
Variable Group code Description
$USERR1 - 5 40 Five real variables intended for use by third-party developers
$USRTIMER 70 0 = Timer off
1 = Timer on
Uniquely identifies a particular version of a drawing.Updated
when the drawing is modified
$VERSIONGUID 2
$VISRETAIN 70 0 = Don't retain xref-dependent visibility settings
1 = Retain xref-dependent visibility settings
$WORLDVIEW 70 0 = Don't change UCS
1 = Set UCS to WCS during DVIEW/VPOINT
$XCLIPFRAME 290 Controls the visibility of xref clipping boundaries:
0 = Clipping boundary is not visible
1 = Clipping boundary is visible
Controls whether the current drawing can be edited in-place
when being referenced by another drawing.
$XEDIT 290
0 = Can't use in-place reference editing
1 = Can use in-place reference editing";
    }
}
