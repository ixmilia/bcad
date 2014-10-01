using System;
using System.IO;
using IxMilia.Dxf;
using IxMilia.Dxf.Sections;
using IxMilia.Dxf.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.DxfTests
{
    [TestClass]
    public class DxfHeaderTests : AbstractDxfTests
    {
        #region Read tests

        [TestMethod]
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
$ANGBASE
 50
5.5E1
  9
$ANGDIR
 70
1
  9
$ATTMODE
 70
1
  9
$AUNITS
 70
3
  9
$AUPREC
 70
7
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
            Assert.AreEqual(16, file.Header.MaintenenceVersion);
            Assert.AreEqual(DxfAcadVersion.R13, file.Header.Version);
            Assert.AreEqual(55.0, file.Header.AngleZeroDirection);
            Assert.AreEqual(DxfAngleDirection.Clockwise, file.Header.AngleDirection);
            Assert.AreEqual(DxfAttributeVisibility.Normal, file.Header.AttributeVisibility);
            Assert.AreEqual(DxfAngleFormat.Radians, file.Header.AngleUnitFormat);
            Assert.AreEqual(7, file.Header.AngleUnitPrecision);
            Assert.AreEqual("<current layer>", file.Header.CurrentLayer);
            Assert.AreEqual(DxfUnitFormat.Architectural, file.Header.UnitFormat);
            Assert.AreEqual(7, file.Header.UnitPrecision);
        }

        [TestMethod]
        public void DateConversionTest()
        {
            // from Autodesk spec: 2451544.91568287 = December 31, 1999, 9:58:35 pm.

            // verify reading
            var file = Section("HEADER", @"
  9
$TDCREATE
 40
2451544.91568287
");
            Assert.AreEqual(new DateTime(1999, 12, 31, 21, 58, 35), file.Header.CreationDate);

            // verify writing.  appending "04" to double value for precision issues.
            VerifyFileContains(file, @"
  9
$TDCREATE
 40
2.4515449156828704E+006
");
        }

        [TestMethod]
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
            var layers = file.Layers;
            Assert.AreEqual(2, layers.Count);
            Assert.AreEqual("a", layers[0].Name);
            Assert.AreEqual(12, layers[0].Color.RawValue);
            Assert.AreEqual("b", layers[1].Name);
            Assert.AreEqual(13, layers[1].Color.RawValue);
        }

        [TestMethod]
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
            var viewPorts = file.ViewPorts;
            Assert.AreEqual(2, viewPorts.Count);

            // defaults
            Assert.AreEqual(null, viewPorts[0].Name);
            Assert.AreEqual(0.0, viewPorts[0].LowerLeft.X);
            Assert.AreEqual(0.0, viewPorts[0].LowerLeft.Y);
            Assert.AreEqual(0.0, viewPorts[0].UpperRight.X);
            Assert.AreEqual(0.0, viewPorts[0].UpperRight.Y);
            Assert.AreEqual(0.0, viewPorts[0].ViewCenter.X);
            Assert.AreEqual(0.0, viewPorts[0].ViewCenter.Y);
            Assert.AreEqual(0.0, viewPorts[0].SnapBasePoint.X);
            Assert.AreEqual(0.0, viewPorts[0].SnapBasePoint.Y);
            Assert.AreEqual(0.0, viewPorts[0].SnapSpacing.X);
            Assert.AreEqual(0.0, viewPorts[0].SnapSpacing.Y);
            Assert.AreEqual(0.0, viewPorts[0].GridSpacing.X);
            Assert.AreEqual(0.0, viewPorts[0].GridSpacing.Y);
            Assert.AreEqual(0.0, viewPorts[0].ViewDirection.X);
            Assert.AreEqual(0.0, viewPorts[0].ViewDirection.Y);
            Assert.AreEqual(1.0, viewPorts[0].ViewDirection.Z);
            Assert.AreEqual(0.0, viewPorts[0].TargetViewPoint.X);
            Assert.AreEqual(0.0, viewPorts[0].TargetViewPoint.Y);
            Assert.AreEqual(0.0, viewPorts[0].TargetViewPoint.Z);
            Assert.AreEqual(0.0, viewPorts[0].ViewHeight);
            Assert.AreEqual(0.0, viewPorts[0].ViewPortAspectRatio);
            Assert.AreEqual(0.0, viewPorts[0].LensLength);
            Assert.AreEqual(0.0, viewPorts[0].FrontClippingPlane);
            Assert.AreEqual(0.0, viewPorts[0].BackClippingPlane);
            Assert.AreEqual(0.0, viewPorts[0].SnapRotationAngle);
            Assert.AreEqual(0.0, viewPorts[0].ViewTwistAngle);

            // specifics
            Assert.AreEqual("vport-2", viewPorts[1].Name);
            Assert.AreEqual(11.0, viewPorts[1].LowerLeft.X);
            Assert.AreEqual(22.0, viewPorts[1].LowerLeft.Y);
            Assert.AreEqual(33.0, viewPorts[1].UpperRight.X);
            Assert.AreEqual(44.0, viewPorts[1].UpperRight.Y);
            Assert.AreEqual(55.0, viewPorts[1].ViewCenter.X);
            Assert.AreEqual(66.0, viewPorts[1].ViewCenter.Y);
            Assert.AreEqual(77.0, viewPorts[1].SnapBasePoint.X);
            Assert.AreEqual(88.0, viewPorts[1].SnapBasePoint.Y);
            Assert.AreEqual(99.0, viewPorts[1].SnapSpacing.X);
            Assert.AreEqual(12.0, viewPorts[1].SnapSpacing.Y);
            Assert.AreEqual(13.0, viewPorts[1].GridSpacing.X);
            Assert.AreEqual(14.0, viewPorts[1].GridSpacing.Y);
            Assert.AreEqual(15.0, viewPorts[1].ViewDirection.X);
            Assert.AreEqual(16.0, viewPorts[1].ViewDirection.Y);
            Assert.AreEqual(17.0, viewPorts[1].ViewDirection.Z);
            Assert.AreEqual(18.0, viewPorts[1].TargetViewPoint.X);
            Assert.AreEqual(19.0, viewPorts[1].TargetViewPoint.Y);
            Assert.AreEqual(20.0, viewPorts[1].TargetViewPoint.Z);
            Assert.AreEqual(21.0, viewPorts[1].ViewHeight);
            Assert.AreEqual(22.0, viewPorts[1].ViewPortAspectRatio);
            Assert.AreEqual(23.0, viewPorts[1].LensLength);
            Assert.AreEqual(24.0, viewPorts[1].FrontClippingPlane);
            Assert.AreEqual(25.0, viewPorts[1].BackClippingPlane);
            Assert.AreEqual(26.0, viewPorts[1].SnapRotationAngle);
            Assert.AreEqual(27.0, viewPorts[1].ViewTwistAngle);
        }

        #endregion

        #region Write tests

        [TestMethod]
        public void WriteDefaultHeaderValuesTest()
        {
            VerifyFileContains(new DxfFile(), @"
  9
$DIMGAP
 40
0.0000000000000000E+000
");
        }

        [TestMethod]
        public void WriteSpecificHeaderValuesTest()
        {
            var file = new DxfFile();
            file.Header.DimensionLineGap = 11.0;
            VerifyFileContains(file, @"
  9
$DIMGAP
 40
1.1000000000000000E+001
");
        }

        [TestMethod]
        public void WriteLayersTest()
        {
            var file = new DxfFile();
            file.Layers.Add(new DxfLayer("default"));
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
 70
0
 62
0
  0
ENDTAB
  0
ENDSEC
");
        }

        [TestMethod]
        public void WriteViewportTest()
        {
            var file = new DxfFile();
            file.ViewPorts.Add(new DxfViewPort());
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
