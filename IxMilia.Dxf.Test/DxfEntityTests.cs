using System;
using System.IO;
using System.Linq;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.DxfTests
{
    [TestClass]
    public class DxfEntityTests : AbstractDxfTests
    {
        #region Private helpers

        private static DxfEntity Entity(string entityType, string data)
        {
            var file = Section("ENTITIES", string.Format(@"
999
ill-placed comment
  0
{0}
  5
<handle>
  6
<linetype-name>
  8
<layer>
 48
3.14159
 60
1
 62
1
 67
1
{1}
", entityType, data.Trim()));
            var entity = file.Entities.Single();
            Assert.AreEqual("<handle>", entity.Handle);
            Assert.AreEqual("<linetype-name>", entity.LinetypeName);
            Assert.AreEqual("<layer>", entity.Layer);
            Assert.AreEqual(3.14159, entity.LinetypeScale);
            Assert.IsFalse(entity.IsVisible);
            Assert.IsTrue(entity.IsInPaperSpace);
            Assert.AreEqual(DxfColor.FromIndex(1), entity.Color);
            return entity;
        }

        private static DxfEntity EmptyEntity(string entityType)
        {
            var file = Section("ENTITIES", string.Format(@"
  0
{0}", entityType));
            var entity = file.Entities.Single();
            Assert.IsNull(entity.Handle);
            Assert.AreEqual("0", entity.Layer);
            Assert.AreEqual("BYLAYER", entity.LinetypeName);
            Assert.AreEqual(1.0, entity.LinetypeScale);
            Assert.IsTrue(entity.IsVisible);
            Assert.IsFalse(entity.IsInPaperSpace);
            Assert.AreEqual(DxfColor.ByLayer, entity.Color);
            return entity;
        }

        private static void EnsureFileContainsEntity(DxfEntity entity, string text)
        {
            var file = new DxfFile();
            file.Entities.Add(entity);
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var actual = new StreamReader(stream).ReadToEnd();
            Assert.IsTrue(actual.Contains(text.Trim()));
        }

        #endregion

        #region Read default value tests

        [TestMethod]
        public void ReadDefaultLineTest()
        {
            var line = (DxfLine)EmptyEntity("LINE");
            Assert.AreEqual(0.0, line.P1.X);
            Assert.AreEqual(0.0, line.P1.Y);
            Assert.AreEqual(0.0, line.P1.Z);
            Assert.AreEqual(0.0, line.P2.X);
            Assert.AreEqual(0.0, line.P2.Y);
            Assert.AreEqual(0.0, line.P2.Z);
            Assert.AreEqual(0.0, line.Thickness);
            Assert.AreEqual(0.0, line.ExtrusionDirection.X);
            Assert.AreEqual(0.0, line.ExtrusionDirection.Y);
            Assert.AreEqual(1.0, line.ExtrusionDirection.Z);
        }

        [TestMethod]
        public void ReadDefaultCircleTest()
        {
            var circle = (DxfCircle)EmptyEntity("CIRCLE");
            Assert.AreEqual(0.0, circle.Center.X);
            Assert.AreEqual(0.0, circle.Center.Y);
            Assert.AreEqual(0.0, circle.Center.Z);
            Assert.AreEqual(0.0, circle.Radius);
            Assert.AreEqual(0.0, circle.Normal.X);
            Assert.AreEqual(0.0, circle.Normal.Y);
            Assert.AreEqual(1.0, circle.Normal.Z);
            Assert.AreEqual(0.0, circle.Thickness);
        }

        [TestMethod]
        public void ReadDefaultArcTest()
        {
            var arc = (DxfArc)EmptyEntity("ARC");
            Assert.AreEqual(0.0, arc.Center.X);
            Assert.AreEqual(0.0, arc.Center.Y);
            Assert.AreEqual(0.0, arc.Center.Z);
            Assert.AreEqual(0.0, arc.Radius);
            Assert.AreEqual(0.0, arc.Normal.X);
            Assert.AreEqual(0.0, arc.Normal.Y);
            Assert.AreEqual(1.0, arc.Normal.Z);
            Assert.AreEqual(0.0, arc.StartAngle);
            Assert.AreEqual(360.0, arc.EndAngle);
            Assert.AreEqual(0.0, arc.Thickness);
        }

        [TestMethod]
        public void ReadDefaultEllipseTest()
        {
            var el = (DxfEllipse)EmptyEntity("ELLIPSE");
            Assert.AreEqual(0.0, el.Center.X);
            Assert.AreEqual(0.0, el.Center.Y);
            Assert.AreEqual(0.0, el.Center.Z);
            Assert.AreEqual(1.0, el.MajorAxis.X);
            Assert.AreEqual(0.0, el.MajorAxis.Y);
            Assert.AreEqual(0.0, el.MajorAxis.Z);
            Assert.AreEqual(0.0, el.Normal.X);
            Assert.AreEqual(0.0, el.Normal.Y);
            Assert.AreEqual(1.0, el.Normal.Z);
            Assert.AreEqual(1.0, el.MinorAxisRatio);
            Assert.AreEqual(0.0, el.StartParameter);
            Assert.AreEqual(Math.PI * 2, el.EndParameter);
        }

        [TestMethod]
        public void ReadDefaultTextTest()
        {
            var text = (DxfText)EmptyEntity("TEXT");
            Assert.AreEqual(0.0, text.Location.X);
            Assert.AreEqual(0.0, text.Location.Y);
            Assert.AreEqual(0.0, text.Location.Z);
            Assert.AreEqual(0.0, text.Normal.X);
            Assert.AreEqual(0.0, text.Normal.Y);
            Assert.AreEqual(1.0, text.Normal.Z);
            Assert.AreEqual(0.0, text.Rotation);
            Assert.AreEqual(1.0, text.TextHeight);
            Assert.IsNull(text.Value);
            Assert.AreEqual("STANDARD", text.TextStyleName);
            Assert.AreEqual(0.0, text.Thickness);
            Assert.AreEqual(1.0, text.RelativeXScaleFactor);
            Assert.AreEqual(0.0, text.ObliqueAngle);
            Assert.IsFalse(text.IsTextBackward);
            Assert.IsFalse(text.IsTextUpsideDown);
            Assert.AreEqual(0.0, text.SecondAlignmentPoint.X);
            Assert.AreEqual(0.0, text.SecondAlignmentPoint.Y);
            Assert.AreEqual(0.0, text.SecondAlignmentPoint.Z);
            Assert.AreEqual(DxfHorizontalTextJustification.Left, text.HorizontalTextJustification);
            Assert.AreEqual(DxfVerticalTextJustification.Baseline, text.VerticalTextJustification);
        }

        [TestMethod]
        public void ReadDefaultVertexTest()
        {
            var vertex = (DxfVertex)EmptyEntity("VERTEX");
            Assert.AreEqual(0.0, vertex.Location.X);
            Assert.AreEqual(0.0, vertex.Location.Y);
            Assert.AreEqual(0.0, vertex.Location.Z);
            Assert.AreEqual(0.0, vertex.StartingWidth);
            Assert.AreEqual(0.0, vertex.EndingWidth);
            Assert.AreEqual(0.0, vertex.Bulge);
            Assert.IsFalse(vertex.IsExtraCreatedByCurveFit);
            Assert.IsFalse(vertex.IsCurveFitTangentDefined);
            Assert.IsFalse(vertex.IsSplineVertexCreatedBySplineFitting);
            Assert.IsFalse(vertex.IsSplineFrameControlPoint);
            Assert.IsFalse(vertex.Is3DPolylineVertex);
            Assert.IsFalse(vertex.Is3DPolygonMesh);
            Assert.IsFalse(vertex.IsPolyfaceMeshVertex);
            Assert.AreEqual(0.0, vertex.CurveFitTangentDirection);
            Assert.AreEqual(0, vertex.PolyfaceMeshVertexIndex1);
            Assert.AreEqual(0, vertex.PolyfaceMeshVertexIndex2);
            Assert.AreEqual(0, vertex.PolyfaceMeshVertexIndex3);
            Assert.AreEqual(0, vertex.PolyfaceMeshVertexIndex4);
        }

        [TestMethod]
        public void ReadDefaultSeqendTest()
        {
            var seqend = (DxfSeqend)EmptyEntity("SEQEND");
            // nothing to verify
        }

        [TestMethod]
        public void ReadDefaultPolylineTest()
        {
            var poly = (DxfPolyline)EmptyEntity("POLYLINE");
            Assert.AreEqual(0.0, poly.Elevation);
            Assert.AreEqual(0.0, poly.Normal.X);
            Assert.AreEqual(0.0, poly.Normal.Y);
            Assert.AreEqual(1.0, poly.Normal.Z);
            Assert.AreEqual(0.0, poly.Thickness);
            Assert.AreEqual(0.0, poly.DefaultStartingWidth);
            Assert.AreEqual(0.0, poly.DefaultEndingWidth);
            Assert.AreEqual(0, poly.PolygonMeshMVertexCount);
            Assert.AreEqual(0, poly.PolygonMeshNVertexCount);
            Assert.AreEqual(0, poly.SmoothSurfaceMDensity);
            Assert.AreEqual(0, poly.SmoothSurfaceNDensity);
            Assert.AreEqual(DxfPolylineCurvedAndSmoothSurfaceType.None, poly.SurfaceType);
            Assert.IsFalse(poly.IsClosed);
            Assert.IsFalse(poly.CurveFitVerticiesAdded);
            Assert.IsFalse(poly.SplineFitVerticiesAdded);
            Assert.IsFalse(poly.Is3DPolyline);
            Assert.IsFalse(poly.Is3DPolygonMesh);
            Assert.IsFalse(poly.IsPolygonMeshClosedInNDirection);
            Assert.IsFalse(poly.IsPolyfaceMesh);
            Assert.IsFalse(poly.IsLinetypePatternGeneratedContinuously);
        }

        [TestMethod]
        public void ReadDefaultSolidTest()
        {
            var solid = (DxfSolid)EmptyEntity("SOLID");
            Assert.AreEqual(DxfPoint.Origin, solid.FirstCorner);
            Assert.AreEqual(DxfPoint.Origin, solid.SecondCorner);
            Assert.AreEqual(DxfPoint.Origin, solid.ThirdCorner);
            Assert.AreEqual(DxfPoint.Origin, solid.FourthCorner);
            Assert.AreEqual(0.0, solid.Thickness);
            Assert.AreEqual(DxfVector.ZAxis, solid.ExtrusionDirection);
        }

        #endregion

        #region Read specific value tests

        [TestMethod]
        public void ReadDimensionTest()
        {
            var dimension = (DxfAlignedDimension)Entity("DIMENSION", @"
  1
text
 10
330.250000
 20
1310.000000
 13
330.250000
 23
1282.000000
 14
319.750000
 24
1282.000000
 70
1
");
            Assert.AreEqual(new DxfPoint(330.25, 1310.0, 0.0), dimension.DefinitionPoint1);
            Assert.AreEqual(new DxfPoint(330.25, 1282, 0.0), dimension.DefinitionPoint2);
            Assert.AreEqual(new DxfPoint(319.75, 1282, 0.0), dimension.DefinitionPoint3);
            Assert.AreEqual("text", dimension.Text);
        }

        [TestMethod]
        public void ReadLineTest()
        {
            var line = (DxfLine)Entity("LINE", @"
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
 11
4.400000E+001
 21
5.500000E+001
 31
6.600000E+001
 39
7.700000E+001
210
8.800000E+001
220
9.900000E+001
230
1.500000E+002
");
            Assert.AreEqual(11.0, line.P1.X);
            Assert.AreEqual(22.0, line.P1.Y);
            Assert.AreEqual(33.0, line.P1.Z);
            Assert.AreEqual(44.0, line.P2.X);
            Assert.AreEqual(55.0, line.P2.Y);
            Assert.AreEqual(66.0, line.P2.Z);
            Assert.AreEqual(77.0, line.Thickness);
            Assert.AreEqual(88.0, line.ExtrusionDirection.X);
            Assert.AreEqual(99.0, line.ExtrusionDirection.Y);
            Assert.AreEqual(150.0, line.ExtrusionDirection.Z);
        }

        [TestMethod]
        public void ReadCircleTest()
        {
            var circle = (DxfCircle)Entity("CIRCLE", @"
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
 40
4.400000E+001
 39
3.500000E+001
210
5.500000E+001
220
6.600000E+001
230
7.700000E+001
");
            Assert.AreEqual(11.0, circle.Center.X);
            Assert.AreEqual(22.0, circle.Center.Y);
            Assert.AreEqual(33.0, circle.Center.Z);
            Assert.AreEqual(44.0, circle.Radius);
            Assert.AreEqual(55.0, circle.Normal.X);
            Assert.AreEqual(66.0, circle.Normal.Y);
            Assert.AreEqual(77.0, circle.Normal.Z);
            Assert.AreEqual(35.0, circle.Thickness);
        }

        [TestMethod]
        public void ReadArcTest()
        {
            var arc = (DxfArc)Entity("ARC", @"
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
 40
4.400000E+001
210
5.500000E+001
220
6.600000E+001
230
7.700000E+001
 50
8.800000E+001
 51
9.900000E+001
 39
3.500000E+001
");
            Assert.AreEqual(11.0, arc.Center.X);
            Assert.AreEqual(22.0, arc.Center.Y);
            Assert.AreEqual(33.0, arc.Center.Z);
            Assert.AreEqual(44.0, arc.Radius);
            Assert.AreEqual(55.0, arc.Normal.X);
            Assert.AreEqual(66.0, arc.Normal.Y);
            Assert.AreEqual(77.0, arc.Normal.Z);
            Assert.AreEqual(88.0, arc.StartAngle);
            Assert.AreEqual(99.0, arc.EndAngle);
            Assert.AreEqual(35.0, arc.Thickness);
        }

        [TestMethod]
        public void ReadEllipseTest()
        {
            var el = (DxfEllipse)Entity("ELLIPSE", @"
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
 11
4.400000E+001
 21
5.500000E+001
 31
6.600000E+001
210
7.700000E+001
220
8.800000E+001
230
9.900000E+001
 40
1.200000E+001
 41
0.100000E+000
 42
0.400000E+000
");
            Assert.AreEqual(11.0, el.Center.X);
            Assert.AreEqual(22.0, el.Center.Y);
            Assert.AreEqual(33.0, el.Center.Z);
            Assert.AreEqual(44.0, el.MajorAxis.X);
            Assert.AreEqual(55.0, el.MajorAxis.Y);
            Assert.AreEqual(66.0, el.MajorAxis.Z);
            Assert.AreEqual(77.0, el.Normal.X);
            Assert.AreEqual(88.0, el.Normal.Y);
            Assert.AreEqual(99.0, el.Normal.Z);
            Assert.AreEqual(12.0, el.MinorAxisRatio);
            Assert.AreEqual(0.1, el.StartParameter);
            Assert.AreEqual(0.4, el.EndParameter);
        }

        [TestMethod]
        public void ReadTextTest()
        {
            var text = (DxfText)Entity("TEXT", @"
  1
foo bar
  7
text style name
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
 39
3.900000E+001
 40
4.400000E+001
 41
4.100000E+001
 50
5.500000E+001
 51
5.100000E+001
 71
255
 72
3
 73
1
 11
9.100000E+001
 21
9.200000E+001
 31
9.300000E+001
 210
6.600000E+001
 220
7.700000E+001
 230
8.800000E+001
");
            Assert.AreEqual("foo bar", text.Value);
            Assert.AreEqual("text style name", text.TextStyleName);
            Assert.AreEqual(11.0, text.Location.X);
            Assert.AreEqual(22.0, text.Location.Y);
            Assert.AreEqual(33.0, text.Location.Z);
            Assert.AreEqual(39.0, text.Thickness);
            Assert.AreEqual(41.0, text.RelativeXScaleFactor);
            Assert.AreEqual(44.0, text.TextHeight);
            Assert.AreEqual(51.0, text.ObliqueAngle);
            Assert.IsTrue(text.IsTextBackward);
            Assert.IsTrue(text.IsTextUpsideDown);
            Assert.AreEqual(DxfHorizontalTextJustification.Aligned, text.HorizontalTextJustification);
            Assert.AreEqual(DxfVerticalTextJustification.Bottom, text.VerticalTextJustification);
            Assert.AreEqual(91.0, text.SecondAlignmentPoint.X);
            Assert.AreEqual(92.0, text.SecondAlignmentPoint.Y);
            Assert.AreEqual(93.0, text.SecondAlignmentPoint.Z);
            Assert.AreEqual(55.0, text.Rotation);
            Assert.AreEqual(66.0, text.Normal.X);
            Assert.AreEqual(77.0, text.Normal.Y);
            Assert.AreEqual(88.0, text.Normal.Z);
        }

        [TestMethod]
        public void ReadVertexTest()
        {
            var vertex = (DxfVertex)Entity("VERTEX", @"
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
 40
4.000000E+001
 41
4.100000E+001
 42
4.200000E+001
 50
5.000000E+001
 70
255
 71
71
 72
72
 73
73
 74
74
");
            Assert.AreEqual(11.0, vertex.Location.X);
            Assert.AreEqual(22.0, vertex.Location.Y);
            Assert.AreEqual(33.0, vertex.Location.Z);
            Assert.AreEqual(40.0, vertex.StartingWidth);
            Assert.AreEqual(41.0, vertex.EndingWidth);
            Assert.AreEqual(42.0, vertex.Bulge);
            Assert.IsTrue(vertex.IsExtraCreatedByCurveFit);
            Assert.IsTrue(vertex.IsCurveFitTangentDefined);
            Assert.IsTrue(vertex.IsSplineVertexCreatedBySplineFitting);
            Assert.IsTrue(vertex.IsSplineFrameControlPoint);
            Assert.IsTrue(vertex.Is3DPolylineVertex);
            Assert.IsTrue(vertex.Is3DPolygonMesh);
            Assert.IsTrue(vertex.IsPolyfaceMeshVertex);
            Assert.AreEqual(50.0, vertex.CurveFitTangentDirection);
            Assert.AreEqual(71, vertex.PolyfaceMeshVertexIndex1);
            Assert.AreEqual(72, vertex.PolyfaceMeshVertexIndex2);
            Assert.AreEqual(73, vertex.PolyfaceMeshVertexIndex3);
            Assert.AreEqual(74, vertex.PolyfaceMeshVertexIndex4);
        }

        [TestMethod]
        public void ReadSeqendTest()
        {
            var seqend = (DxfSeqend)Entity("SEQEND", "");
            // nothing to verify
        }

        [TestMethod]
        public void ReadPolylineTest()
        {
            var poly = (DxfPolyline)Entity("POLYLINE", @"
 30
1.100000E+001
 39
1.800000E+001
 40
4.000000E+001
 41
4.100000E+001
 70
255
 71
71
 72
72
 73
73
 74
74
 75
6
210
2.200000E+001
220
3.300000E+001
230
4.400000E+001
  0
VERTEX
 10
1.200000E+001
 20
2.300000E+001
 30
3.400000E+001
  0
VERTEX
 10
4.500000E+001
 20
5.600000E+001
 30
6.700000E+001
  0
SEQEND
");
            Assert.AreEqual(11.0, poly.Elevation);
            Assert.AreEqual(18.0, poly.Thickness);
            Assert.AreEqual(40.0, poly.DefaultStartingWidth);
            Assert.AreEqual(41.0, poly.DefaultEndingWidth);
            Assert.AreEqual(71, poly.PolygonMeshMVertexCount);
            Assert.AreEqual(72, poly.PolygonMeshNVertexCount);
            Assert.AreEqual(73, poly.SmoothSurfaceMDensity);
            Assert.AreEqual(74, poly.SmoothSurfaceNDensity);
            Assert.AreEqual(DxfPolylineCurvedAndSmoothSurfaceType.CubicBSpline, poly.SurfaceType);
            Assert.IsTrue(poly.IsClosed);
            Assert.IsTrue(poly.CurveFitVerticiesAdded);
            Assert.IsTrue(poly.SplineFitVerticiesAdded);
            Assert.IsTrue(poly.Is3DPolyline);
            Assert.IsTrue(poly.Is3DPolygonMesh);
            Assert.IsTrue(poly.IsPolygonMeshClosedInNDirection);
            Assert.IsTrue(poly.IsPolyfaceMesh);
            Assert.IsTrue(poly.IsLinetypePatternGeneratedContinuously);
            Assert.AreEqual(22.0, poly.Normal.X);
            Assert.AreEqual(33.0, poly.Normal.Y);
            Assert.AreEqual(44.0, poly.Normal.Z);
            Assert.AreEqual(2, poly.Vertices.Count);
            Assert.AreEqual(12.0, poly.Vertices[0].Location.X);
            Assert.AreEqual(23.0, poly.Vertices[0].Location.Y);
            Assert.AreEqual(34.0, poly.Vertices[0].Location.Z);
            Assert.AreEqual(45.0, poly.Vertices[1].Location.X);
            Assert.AreEqual(56.0, poly.Vertices[1].Location.Y);
            Assert.AreEqual(67.0, poly.Vertices[1].Location.Z);
        }

        public void ReadSolidTest()
        {
            var solid = (DxfSolid)Entity("SOLID", @"
 10
1
 20
2
 30
3
 11
4
 21
5
 31
6
 12
7
 22
8
 32
9
 13
10
 23
11
 33
12
 39
13
210
14
220
15
230
16
");
            Assert.AreEqual(new DxfPoint(1, 2, 3), solid.FirstCorner);
            Assert.AreEqual(new DxfPoint(4, 5, 6), solid.SecondCorner);
            Assert.AreEqual(new DxfPoint(7, 8, 9), solid.ThirdCorner);
            Assert.AreEqual(new DxfPoint(10, 11, 12), solid.FourthCorner);
            Assert.AreEqual(13.0, solid.Thickness);
            Assert.AreEqual(new DxfVector(14, 15, 16), solid.ExtrusionDirection);
        }

        #endregion

        #region Write default value tests

        [TestMethod]
        public void WriteDefaultLineTest()
        {
            EnsureFileContainsEntity(new DxfLine(), @"
  0
LINE
  5

  8
0
100
AcDbLine
 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
 11
0.0000000000000000E+000
 21
0.0000000000000000E+000
 31
0.0000000000000000E+000
  0
");
        }

        [TestMethod]
        public void WriteDefaultCircleTest()
        {
            EnsureFileContainsEntity(new DxfCircle(), @"
  0
CIRCLE
  5

  8
0
100
AcDbCircle
 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
 40
0.0000000000000000E+000
  0
");
        }

        [TestMethod]
        public void WriteDefaultArcTest()
        {
            EnsureFileContainsEntity(new DxfArc(), @"
  0
ARC
  5

  8
0
100
AcDbCircle
 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
 40
0.0000000000000000E+000
100
AcDbArc
 50
0.0000000000000000E+000
 51
3.6000000000000000E+002
  0
");
        }

        [TestMethod]
        public void WriteDefaultEllipseTest()
        {
            EnsureFileContainsEntity(new DxfEllipse(), @"
  0
ELLIPSE
  5

  8
0
100
AcDbEllipse
 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
 11
1.0000000000000000E+000
 21
0.0000000000000000E+000
 31
0.0000000000000000E+000
 40
1.0000000000000000E+000
 41
0.0000000000000000E+000
 42
6.2831853071795862E+000
  0
");
        }

        [TestMethod]
        public void WriteDefaultTextTest()
        {
            EnsureFileContainsEntity(new DxfText(), @"
  0
TEXT
  5

  8
0
100
AcDbText
 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
 40
1.0000000000000000E+000
  1

 11
0.0000000000000000E+000
 21
0.0000000000000000E+000
 31
0.0000000000000000E+000
  0
");
        }

        [TestMethod]
        public void WriteDefaultPolylineTest()
        {
            EnsureFileContainsEntity(new DxfPolyline(), @"
  0
POLYLINE
  5

  8
0
100
AcDb2dPolyline
 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
  0
SEQEND
  5

  8
0
  0
");
        }

        public void WriteDefaultSolidTest()
        {
            EnsureFileContainsEntity(new DxfSolid(), @"
  0
SOLID
 62
0
100
AcDbTrace
 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
 11
0.0000000000000000E+000
 21
0.0000000000000000E+000
 31
0.0000000000000000E+000
 12
0.0000000000000000E+000
 22
0.0000000000000000E+000
 32
0.0000000000000000E+000
 13
0.0000000000000000E+000
 23
0.0000000000000000E+000
 33
0.0000000000000000E+000
");
        }

        #endregion

        #region Write specific value tests TODO

        [TestMethod]
        public void WriteLineTest()
        {
            EnsureFileContainsEntity(new DxfLine(new DxfPoint(1, 2, 3), new DxfPoint(4, 5, 6))
                {
                    Color = DxfColor.FromIndex(7),
                    Handle = "foo",
                    Layer = "bar",
                    Thickness = 7,
                    ExtrusionDirection = new DxfVector(8, 9, 10)
                }, @"
  0
LINE
  5
foo
  8
bar
 62
7
100
AcDbLine
 39
7.0000000000000000E+000
 10
1.0000000000000000E+000
 20
2.0000000000000000E+000
 30
3.0000000000000000E+000
 11
4.0000000000000000E+000
 21
5.0000000000000000E+000
 31
6.0000000000000000E+000
210
8.0000000000000000E+000
220
9.0000000000000000E+000
230
1.0000000000000000E+001
  0
");
        }

        [TestMethod]
        public void WriteDimensionTest()
        {
            EnsureFileContainsEntity(new DxfAlignedDimension()
            {
                Color = DxfColor.FromIndex(7),
                DefinitionPoint1 = new DxfPoint(330.25, 1310.0, 330.25),
                DefinitionPoint2 = new DxfPoint(330.25, 1282.0, 0.0),
                DefinitionPoint3 = new DxfPoint(319.75, 1282.0, 0.0),
                Handle = "foo",
                Layer = "bar",
                Text = "text"
            }, @"
  0
DIMENSION
  5
foo
  8
bar
 62
7
100
AcDbDimension
  2

 10
3.3025000000000000E+002
 20
1.3100000000000000E+003
 30
3.3025000000000000E+002
 11
0.0000000000000000E+000
 21
0.0000000000000000E+000
 31
0.0000000000000000E+000
 70
0
  1
text
 71
1
100
AcDbAlignedDimension
 12
0.0000000000000000E+000
 22
0.0000000000000000E+000
 32
0.0000000000000000E+000
 13
3.3025000000000000E+002
 23
1.2820000000000000E+003
 33
0.0000000000000000E+000
 14
3.1975000000000000E+002
 24
1.2820000000000000E+003
 34
0.0000000000000000E+000
");
        }

        #endregion

        #region Block tests

        public void ReadBlockTest()
        {
            var file = Parse(@"
  0
SECTION
  2
BLOCKS
  0
BLOCK
  2
block #1
 10
1
 20
2
 30
3
  0
LINE
 10
10
 20
20
 30
30
 11
11
 21
21
 31
31
  0
ENDBLK
  0
BLOCK
  2
block #2
  0
CIRCLE
 40
40
  0
ARC
 40
41
  0
ENDBLK
  0
ENDSEC
  0
EOF");

            // 2 blocks
            Assert.AreEqual(2, file.Blocks.Count);

            // first block
            var first = file.Blocks[0];
            Assert.AreEqual("block #1", first.Name);
            Assert.AreEqual(new DxfPoint(1, 2, 3), first.BasePoint);
            Assert.AreEqual(1, first.Entities.Count);
            var entity = first.Entities.First();
            Assert.AreEqual(DxfEntityType.Line, entity.EntityType);
            var line = (DxfLine)entity;
            Assert.AreEqual(new DxfPoint(10, 20, 30), line.P1);
            Assert.AreEqual(new DxfPoint(11, 21, 31), line.P2);

            // second block
            var second = file.Blocks[1];
            Assert.AreEqual("block #2", second.Name);
            Assert.AreEqual(2, second.Entities.Count);
            Assert.AreEqual(DxfEntityType.Circle, second.Entities[0].EntityType);
            Assert.AreEqual(40.0, ((DxfCircle)second.Entities[0]).Radius);
            Assert.AreEqual(DxfEntityType.Arc, second.Entities[1].EntityType);
            Assert.AreEqual(41.0, ((DxfArc)second.Entities[1]).Radius);
        }

        #endregion
    }
}
