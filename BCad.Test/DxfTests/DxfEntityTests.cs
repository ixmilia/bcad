using System;
using System.IO;
using System.Linq;
using BCad.Dxf;
using BCad.Dxf.Entities;
using Xunit;

namespace BCad.Test.DxfTests
{
    public class DxfEntityTests : AbstractDxfTests
    {
        #region Private helpers

        private static DxfEntity Entity(string entityType, string data)
        {
            var file = Section("ENTITIES", string.Format(@"
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
            var entity = file.EntitiesSection.Entities.Single();
            Assert.Equal("<handle>", entity.Handle);
            Assert.Equal("<linetype-name>", entity.LinetypeName);
            Assert.Equal("<layer>", entity.Layer);
            Assert.Equal(3.14159, entity.LinetypeScale);
            Assert.False(entity.IsVisible);
            Assert.True(entity.IsInPaperSpace);
            Assert.Equal(DxfColor.FromIndex(1), entity.Color);
            return entity;
        }

        private static DxfEntity EmptyEntity(string entityType)
        {
            var file = Section("ENTITIES", string.Format(@"
  0
{0}", entityType));
            var entity = file.EntitiesSection.Entities.Single();
            Assert.Null(entity.Handle);
            Assert.Null(entity.Layer);
            Assert.Null(entity.LinetypeName);
            Assert.Equal(1.0, entity.LinetypeScale);
            Assert.True(entity.IsVisible);
            Assert.False(entity.IsInPaperSpace);
            Assert.Equal(DxfColor.ByBlock, entity.Color);
            return entity;
        }

        private static void EnsureFileContainsEntity(DxfEntity entity, string text)
        {
            var file = new DxfFile();
            file.EntitiesSection.Entities.Add(entity);
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var actual = new StreamReader(stream).ReadToEnd();
            Assert.Contains(text.Trim(), actual);
        }

        #endregion

        #region Read default value tests

        [Fact]
        public void ReadDefaultLineTest()
        {
            var line = (DxfLine)EmptyEntity("LINE");
            Assert.Equal(0.0, line.P1.X);
            Assert.Equal(0.0, line.P1.Y);
            Assert.Equal(0.0, line.P1.Z);
            Assert.Equal(0.0, line.P2.X);
            Assert.Equal(0.0, line.P2.Y);
            Assert.Equal(0.0, line.P2.Z);
        }

        [Fact]
        public void ReadDefaultCircleTest()
        {
            var circle = (DxfCircle)EmptyEntity("CIRCLE");
            Assert.Equal(0.0, circle.Center.X);
            Assert.Equal(0.0, circle.Center.Y);
            Assert.Equal(0.0, circle.Center.Z);
            Assert.Equal(0.0, circle.Radius);
            Assert.Equal(0.0, circle.Normal.X);
            Assert.Equal(0.0, circle.Normal.Y);
            Assert.Equal(1.0, circle.Normal.Z);
            Assert.Equal(0.0, circle.Thickness);
        }

        [Fact]
        public void ReadDefaultArcTest()
        {
            var arc = (DxfArc)EmptyEntity("ARC");
            Assert.Equal(0.0, arc.Center.X);
            Assert.Equal(0.0, arc.Center.Y);
            Assert.Equal(0.0, arc.Center.Z);
            Assert.Equal(0.0, arc.Radius);
            Assert.Equal(0.0, arc.Normal.X);
            Assert.Equal(0.0, arc.Normal.Y);
            Assert.Equal(1.0, arc.Normal.Z);
            Assert.Equal(0.0, arc.StartAngle);
            Assert.Equal(360.0, arc.EndAngle);
            Assert.Equal(0.0, arc.Thickness);
        }

        [Fact]
        public void ReadDefaultEllipseTest()
        {
            var el = (DxfEllipse)EmptyEntity("ELLIPSE");
            Assert.Equal(0.0, el.Center.X);
            Assert.Equal(0.0, el.Center.Y);
            Assert.Equal(0.0, el.Center.Z);
            Assert.Equal(0.0, el.MajorAxis.X);
            Assert.Equal(0.0, el.MajorAxis.Y);
            Assert.Equal(0.0, el.MajorAxis.Z);
            Assert.Equal(0.0, el.Normal.X);
            Assert.Equal(0.0, el.Normal.Y);
            Assert.Equal(1.0, el.Normal.Z);
            Assert.Equal(1.0, el.MinorAxisRatio);
            Assert.Equal(0.0, el.StartParameter);
            Assert.Equal(360.0, el.EndParameter);
        }

        [Fact]
        public void ReadDefaultTextTest()
        {
            var text = (DxfText)EmptyEntity("TEXT");
            Assert.Equal(0.0, text.Location.X);
            Assert.Equal(0.0, text.Location.Y);
            Assert.Equal(0.0, text.Location.Z);
            Assert.Equal(0.0, text.Normal.X);
            Assert.Equal(0.0, text.Normal.Y);
            Assert.Equal(1.0, text.Normal.Z);
            Assert.Equal(0.0, text.Rotation);
            Assert.Equal(1.0, text.TextHeight);
            Assert.Equal(null, text.Value);
        }

        [Fact]
        public void ReadDefaultVertexTest()
        {
            var vertex = (DxfVertex)EmptyEntity("VERTEX");
            Assert.Equal(0.0, vertex.Location.X);
            Assert.Equal(0.0, vertex.Location.Y);
            Assert.Equal(0.0, vertex.Location.Z);
        }

        [Fact]
        public void ReadDefaultSeqendTest()
        {
            var seqend = (DxfSeqend)EmptyEntity("SEQEND");
            // nothing to verify
        }

        [Fact]
        public void ReadDefaultPolylineTest()
        {
            var poly = (DxfPolyline)EmptyEntity("POLYLINE");
            Assert.Equal(0.0, poly.Elevation);
            Assert.Equal(0.0, poly.Normal.X);
            Assert.Equal(0.0, poly.Normal.Y);
            Assert.Equal(1.0, poly.Normal.Z);
        }

        #endregion

        #region Read specific value tests

        [Fact]
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
");
            Assert.Equal(11.0, line.P1.X);
            Assert.Equal(22.0, line.P1.Y);
            Assert.Equal(33.0, line.P1.Z);
            Assert.Equal(44.0, line.P2.X);
            Assert.Equal(55.0, line.P2.Y);
            Assert.Equal(66.0, line.P2.Z);
        }

        [Fact]
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
            Assert.Equal(11.0, circle.Center.X);
            Assert.Equal(22.0, circle.Center.Y);
            Assert.Equal(33.0, circle.Center.Z);
            Assert.Equal(44.0, circle.Radius);
            Assert.Equal(55.0, circle.Normal.X);
            Assert.Equal(66.0, circle.Normal.Y);
            Assert.Equal(77.0, circle.Normal.Z);
            Assert.Equal(35.0, circle.Thickness);
        }

        [Fact]
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
            Assert.Equal(11.0, arc.Center.X);
            Assert.Equal(22.0, arc.Center.Y);
            Assert.Equal(33.0, arc.Center.Z);
            Assert.Equal(44.0, arc.Radius);
            Assert.Equal(55.0, arc.Normal.X);
            Assert.Equal(66.0, arc.Normal.Y);
            Assert.Equal(77.0, arc.Normal.Z);
            Assert.Equal(88.0, arc.StartAngle);
            Assert.Equal(99.0, arc.EndAngle);
            Assert.Equal(35.0, arc.Thickness);
        }

        [Fact]
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
            Assert.Equal(11.0, el.Center.X);
            Assert.Equal(22.0, el.Center.Y);
            Assert.Equal(33.0, el.Center.Z);
            Assert.Equal(44.0, el.MajorAxis.X);
            Assert.Equal(55.0, el.MajorAxis.Y);
            Assert.Equal(66.0, el.MajorAxis.Z);
            Assert.Equal(77.0, el.Normal.X);
            Assert.Equal(88.0, el.Normal.Y);
            Assert.Equal(99.0, el.Normal.Z);
            Assert.Equal(12.0, el.MinorAxisRatio);
            Assert.Equal(0.1, el.StartParameter);
            Assert.Equal(0.4, el.EndParameter);
        }

        [Fact]
        public void ReadTextTest()
        {
            var text = (DxfText)Entity("TEXT", @"
  1
foo bar
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
 40
4.400000E+001
 50
5.500000E+001
 210
6.600000E+001
 220
7.700000E+001
 230
8.800000E+001
");
            Assert.Equal("foo bar", text.Value);
            Assert.Equal(11.0, text.Location.X);
            Assert.Equal(22.0, text.Location.Y);
            Assert.Equal(33.0, text.Location.Z);
            Assert.Equal(44.0, text.TextHeight);
            Assert.Equal(55.0, text.Rotation);
            Assert.Equal(66.0, text.Normal.X);
            Assert.Equal(77.0, text.Normal.Y);
            Assert.Equal(88.0, text.Normal.Z);
        }

        [Fact]
        public void ReadVertexTest()
        {
            var vertex = (DxfVertex)Entity("VERTEX", @"
 10
1.100000E+001
 20
2.200000E+001
 30
3.300000E+001
");
            Assert.Equal(11.0, vertex.Location.X);
            Assert.Equal(22.0, vertex.Location.Y);
            Assert.Equal(33.0, vertex.Location.Z);
        }

        [Fact]
        public void ReadSeqendTest()
        {
            var seqend = (DxfSeqend)Entity("SEQEND", "");
            // nothing to verify
        }

        [Fact]
        public void ReadPolylineTest()
        {
            var poly = (DxfPolyline)Entity("POLYLINE", @"
  30
1.100000E+001
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
            Assert.Equal(11.0, poly.Elevation);
            Assert.Equal(22.0, poly.Normal.X);
            Assert.Equal(33.0, poly.Normal.Y);
            Assert.Equal(44.0, poly.Normal.Z);
            Assert.Equal(2, poly.Vertices.Count);
            Assert.Equal(12.0, poly.Vertices[0].Location.X);
            Assert.Equal(23.0, poly.Vertices[0].Location.Y);
            Assert.Equal(34.0, poly.Vertices[0].Location.Z);
            Assert.Equal(45.0, poly.Vertices[1].Location.X);
            Assert.Equal(56.0, poly.Vertices[1].Location.Y);
            Assert.Equal(67.0, poly.Vertices[1].Location.Z);
        }

        #endregion

        #region Write default value tests

        [Fact]
        public void WriteDefaultLineTest()
        {
            EnsureFileContainsEntity(new DxfLine(), @"
  0
LINE
 62
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

        [Fact]
        public void WriteDefaultCircleTest()
        {
            EnsureFileContainsEntity(new DxfCircle(), @"
  0
CIRCLE
 62
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

        [Fact]
        public void WriteDefaultArcTest()
        {
            EnsureFileContainsEntity(new DxfArc(), @"
  0
ARC
 62
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
 50
0.0000000000000000E+000
 51
3.6000000000000000E+002
  0
");
        }

        [Fact]
        public void WriteDefaultEllipseTest()
        {
            EnsureFileContainsEntity(new DxfEllipse(), @"
  0
ELLIPSE
 62
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
0.0000000000000000E+000
 21
0.0000000000000000E+000
 31
0.0000000000000000E+000
 40
1.0000000000000000E+000
 41
0.0000000000000000E+000
 42
3.6000000000000000E+002
  0
");
        }

        [Fact]
        public void WriteDefaultTextTest()
        {
            EnsureFileContainsEntity(new DxfText(), @"
  0
TEXT
 62
0
100
AcDbText
  1

 10
0.0000000000000000E+000
 20
0.0000000000000000E+000
 30
0.0000000000000000E+000
 40
1.0000000000000000E+000
  0
");
        }

        [Fact]
        public void WriteDefaultPolylineTest()
        {
            EnsureFileContainsEntity(new DxfPolyline(), @"
  0
POLYLINE
 62
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
  0
");
        }

        #endregion

        #region Write specific value tests TODO

        [Fact]
        public void WriteLineTest()
        {
            EnsureFileContainsEntity(new DxfLine(new DxfPoint(1, 2, 3), new DxfPoint(4, 5, 6))
                {
                    Color = DxfColor.FromIndex(7),
                    Handle = "foo",
                    Layer = "bar"
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
  0
");
        }

        #endregion
    }
}
