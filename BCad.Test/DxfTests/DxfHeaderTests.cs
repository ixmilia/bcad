using System;
using System.IO;
using BCad.Dxf;
using BCad.Dxf.Sections;
using BCad.Dxf.Tables;
using Xunit;

namespace BCad.Test.DxfTests
{
    public class DxfHeaderTests : AbstractDxfTests
    {
        #region Read tests

        [Fact]
        public void DefaultHeaderValuesTest()
        {
            var file = Section("HEADER", "");
            Assert.Null(file.HeaderSection.CurrentLayer);
            Assert.Equal(0, file.HeaderSection.MaintenanceVersion);
            Assert.Equal(DxfAcadVersion.R14, file.HeaderSection.Version);
            Assert.Equal(DxfUnitFormat.None, file.HeaderSection.UnitFormat);
            Assert.Equal(0, file.HeaderSection.UnitPrecision);
        }

        [Fact]
        public void SpecificHeaderValuesTest()
        {
            var file = Section("HEADER", @"
  9
$ACADMAINTVER
 70
16
  9
$ACADVER
  1
AC1012
  9
$CLAYER
  8
<current layer>
  9
$LUNITS
 70
6
  9
$LUPREC
 70
7
");
            Assert.Equal("<current layer>", file.HeaderSection.CurrentLayer);
            Assert.Equal(16, file.HeaderSection.MaintenanceVersion);
            Assert.Equal(DxfAcadVersion.R13, file.HeaderSection.Version);
            Assert.Equal(DxfUnitFormat.Architectural, file.HeaderSection.UnitFormat);
            Assert.Equal(7, file.HeaderSection.UnitPrecision);
        }

        [Fact]
        public void LayerTableTest()
        {
            var file = Section("TABLES", @"
  0
TABLE
  2
LAYER
  0
LAYER
  2
a
 62
12
  0
LAYER
  2
b
 62
13
  0
ENDTAB
");
            var layers = file.TablesSection.LayerTable.Layers;
            Assert.Equal(2, layers.Count);
            Assert.Equal("a", layers[0].Name);
            Assert.Equal(12, layers[0].Color.RawValue);
            Assert.Equal("b", layers[1].Name);
            Assert.Equal(13, layers[1].Color.RawValue);
        }

        [Fact]
        public void ViewPortTableTest()
        {
            var file = Section("TABLES", @"
  0
TABLE
  2
VPORT
  0
VPORT
  0
VPORT
  2
vport-2
 10
1.100000E+001
 20
2.200000E+001
 11
3.300000E+001
 21
4.400000E+001
 12
5.500000E+001
 22
6.600000E+001
 13
7.700000E+001
 23
8.800000E+001
 14
9.900000E+001
 24
1.200000E+001
 15
1.300000E+001
 25
1.400000E+001
 16
1.500000E+001
 26
1.600000E+001
 36
1.700000E+001
 17
1.800000E+001
 27
1.900000E+001
 37
2.000000E+001
 40
2.100000E+001
 41
2.200000E+001
 42
2.300000E+001
 43
2.400000E+001
 44
2.500000E+001
 50
2.600000E+001
 51
2.700000E+001
  0
ENDTAB
");
            var viewPorts = file.TablesSection.ViewPortTable.ViewPorts;
            Assert.Equal(2, viewPorts.Count);

            // defaults
            Assert.Equal(null, viewPorts[0].Name);
            Assert.Equal(0.0, viewPorts[0].LowerLeft.X);
            Assert.Equal(0.0, viewPorts[0].LowerLeft.Y);
            Assert.Equal(0.0, viewPorts[0].UpperRight.X);
            Assert.Equal(0.0, viewPorts[0].UpperRight.Y);
            Assert.Equal(0.0, viewPorts[0].ViewCenter.X);
            Assert.Equal(0.0, viewPorts[0].ViewCenter.Y);
            Assert.Equal(0.0, viewPorts[0].SnapBasePoint.X);
            Assert.Equal(0.0, viewPorts[0].SnapBasePoint.Y);
            Assert.Equal(0.0, viewPorts[0].SnapSpacing.X);
            Assert.Equal(0.0, viewPorts[0].SnapSpacing.Y);
            Assert.Equal(0.0, viewPorts[0].GridSpacing.X);
            Assert.Equal(0.0, viewPorts[0].GridSpacing.Y);
            Assert.Equal(0.0, viewPorts[0].ViewDirection.X);
            Assert.Equal(0.0, viewPorts[0].ViewDirection.Y);
            Assert.Equal(1.0, viewPorts[0].ViewDirection.Z);
            Assert.Equal(0.0, viewPorts[0].TargetViewPoint.X);
            Assert.Equal(0.0, viewPorts[0].TargetViewPoint.Y);
            Assert.Equal(0.0, viewPorts[0].TargetViewPoint.Z);
            Assert.Equal(0.0, viewPorts[0].ViewHeight);
            Assert.Equal(0.0, viewPorts[0].ViewPortAspectRatio);
            Assert.Equal(0.0, viewPorts[0].LensLength);
            Assert.Equal(0.0, viewPorts[0].FrontClippingPlane);
            Assert.Equal(0.0, viewPorts[0].BackClippingPlane);
            Assert.Equal(0.0, viewPorts[0].SnapRotationAngle);
            Assert.Equal(0.0, viewPorts[0].ViewTwistAngle);

            // specifics
            Assert.Equal("vport-2", viewPorts[1].Name);
            Assert.Equal(11.0, viewPorts[1].LowerLeft.X);
            Assert.Equal(22.0, viewPorts[1].LowerLeft.Y);
            Assert.Equal(33.0, viewPorts[1].UpperRight.X);
            Assert.Equal(44.0, viewPorts[1].UpperRight.Y);
            Assert.Equal(55.0, viewPorts[1].ViewCenter.X);
            Assert.Equal(66.0, viewPorts[1].ViewCenter.Y);
            Assert.Equal(77.0, viewPorts[1].SnapBasePoint.X);
            Assert.Equal(88.0, viewPorts[1].SnapBasePoint.Y);
            Assert.Equal(99.0, viewPorts[1].SnapSpacing.X);
            Assert.Equal(12.0, viewPorts[1].SnapSpacing.Y);
            Assert.Equal(13.0, viewPorts[1].GridSpacing.X);
            Assert.Equal(14.0, viewPorts[1].GridSpacing.Y);
            Assert.Equal(15.0, viewPorts[1].ViewDirection.X);
            Assert.Equal(16.0, viewPorts[1].ViewDirection.Y);
            Assert.Equal(17.0, viewPorts[1].ViewDirection.Z);
            Assert.Equal(18.0, viewPorts[1].TargetViewPoint.X);
            Assert.Equal(19.0, viewPorts[1].TargetViewPoint.Y);
            Assert.Equal(20.0, viewPorts[1].TargetViewPoint.Z);
            Assert.Equal(21.0, viewPorts[1].ViewHeight);
            Assert.Equal(22.0, viewPorts[1].ViewPortAspectRatio);
            Assert.Equal(23.0, viewPorts[1].LensLength);
            Assert.Equal(24.0, viewPorts[1].FrontClippingPlane);
            Assert.Equal(25.0, viewPorts[1].BackClippingPlane);
            Assert.Equal(26.0, viewPorts[1].SnapRotationAngle);
            Assert.Equal(27.0, viewPorts[1].ViewTwistAngle);
        }

        #endregion

        #region Write tests

        private static void VerifyFileContents(DxfFile file, string expected, Action<string, string> predicate)
        {
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var actual = new StreamReader(stream).ReadToEnd();
            predicate(expected, actual);
        }

        private static void VerifyFileContains(DxfFile file, string expected)
        {
            VerifyFileContents(file, expected, (ex, ac) => Assert.Contains(ex.Trim(), ac));
        }

        private static void VerifyFileIsExactly(DxfFile file, string expected)
        {
            VerifyFileContents(file, expected, (ex, ac) => Assert.Equal(ex.Trim(), ac.Trim()));
        }

        [Fact]
        public void WriteDefaultHeaderValuesTest()
        {
            VerifyFileIsExactly(new DxfFile(), "0\r\nEOF");
        }

        [Fact]
        public void WriteSpecificHeaderValuesTest()
        {
            var file = new DxfFile();
            file.HeaderSection.MaintenanceVersion = 16;
            file.HeaderSection.CurrentLayer = "<current layer>";
            file.HeaderSection.Version = DxfAcadVersion.R13;
            file.HeaderSection.UnitFormat = DxfUnitFormat.Engineering;
            file.HeaderSection.UnitPrecision = 4;
            VerifyFileContains(file, @"
  0
SECTION
  2
HEADER
  9
$ACADMAINTVER
 70
16
  9
$ACADVER
  1
AC1012
  9
$CLAYER
  8
<current layer>
  9
$LUNITS
 70
3
  9
$LUPREC
 70
4
  0
ENDSEC
");
        }

        [Fact]
        public void WriteLayersTest()
        {
            var file = new DxfFile();
            file.TablesSection.LayerTable.Layers.Add(new DxfLayer("default"));
            VerifyFileContains(file, @"
  0
SECTION
  2
TABLES
  0
TABLE
  2
LAYER
  0
LAYER
  2
default
 62
0
  0
ENDTAB
  0
ENDSEC
");
        }

        [Fact]
        public void WriteViewportTest()
        {
            var file = new DxfFile();
            file.TablesSection.ViewPortTable.ViewPorts.Add(new DxfViewPort());
            VerifyFileContains(file, @"
  0
SECTION
  2
TABLES
  0
TABLE
  2
VPORT
  0
VPORT
  2
*ACTIVE
  0
ENDTAB
  0
ENDSEC
");
        }

        #endregion

    }
}
