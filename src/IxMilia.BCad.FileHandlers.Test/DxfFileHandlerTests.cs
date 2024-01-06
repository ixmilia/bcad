using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.FileHandlers.Extensions;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    public class DxfFileHandlerTests : FileHandlerTestsBase
    {
        public override IFileHandler FileHandler => new DxfFileHandler();

        private DrawingSettings DrawingSettings { get; } = new DrawingSettings();

        protected override Task<Entity> RoundTripEntity(Entity entity, DrawingSettings drawingSettings = null)
        {
            // shortcut to avoid file writing/reading
            drawingSettings ??= new DrawingSettings();
            return entity.ToDxfEntity(drawingSettings).ToEntity(Workspace.FileSystemService.ReadAllBytesAsync, new Dictionary<string, IEnumerable<DxfEntity>>());
        }

        private Task VerifyRoundTrip(Entity entity) => VerifyRoundTrip(entity, DrawingSettings);

        private async Task<DxfFile> WriteToFile(Drawing drawing)
        {
            using (var stream = await WriteToStream(drawing))
            {
                return DxfFile.Load(stream);
            }
        }

        private static string CreateDxfContent(params (string, string)[] pairs)
        {
            var dxfContent = string.Join("\r\n", pairs.Select(pair => $"{pair.Item1}\r\n{pair.Item2}"));
            return dxfContent;
        }

        private async Task<Drawing> ReadFromDxfContent(params (string, string)[] pairs)
        {
            var dxfContent = CreateDxfContent(pairs);
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, new UTF8Encoding(false)))
            {
                writer.WriteLine(dxfContent);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var drawing = await ReadFromStream(ms);
                return drawing;
            }
        }

        private async Task<(string, string)[]> GetDxfCodePairs(Drawing drawing)
        {
            var pairs = new List<(string, string)>();
            var stream = await WriteToStream(drawing);
            using (var reader = new StreamReader(stream))
            {
                var code = await reader.ReadLineAsync();
                while (code != null)
                {
                    var value = await reader.ReadLineAsync();
                    pairs.Add((code, value));
                    code = await reader.ReadLineAsync();
                }
            }

            return pairs.ToArray();
        }

        [Fact]
        public async Task RoundTripColorTest()
        {
            await VerifyRoundTrip(new Line(Point.Origin, Point.Origin, color: null));
            await VerifyRoundTrip(new Line(Point.Origin, Point.Origin, CadColor.Red));
            await VerifyRoundTrip(new Line(Point.Origin, Point.Origin, CadColor.FromArgb(255, 1, 2, 5)));
        }

        [Fact]
        public async Task RoundTripArcTest()
        {
            await VerifyRoundTrip(new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, Vector.YAxis, CadColor.Yellow, thickness: 1.2345));
        }

        [Fact]
        public async Task RoundTripCircleTest()
        {
            await VerifyRoundTrip(new Circle(new Point(1.0, 2.0, 3.0), 4.0, Vector.XAxis, CadColor.Red, thickness: 1.2345));
        }

        [Fact]
        public async Task RoundTripLineTest()
        {
            await VerifyRoundTrip(new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), CadColor.Green, thickness: 1.2345));
        }

        [Fact]
        public async Task RoundTripPolylineTest()
        {
            // 90 degree arc from (0,0) to (1,1) representing the fourth quadrant
            await VerifyRoundTrip(new Polyline(new[]
            {
                new Vertex(Point.Origin),
                new Vertex(new Point(1.0, 1.0, 0.0), 90.0, VertexDirection.CounterClockwise)
            }));

            // 90 degree arc from (0,0) to (1,1) representing the second quadrant
            await VerifyRoundTrip(new Polyline(new[]
            {
                new Vertex(Point.Origin),
                new Vertex(new Point(1.0, 1.0, 0.0), 90.0, VertexDirection.Clockwise)
            }));
        }

        [Fact]
        public async Task RoundTripSplineTest()
        {
            await VerifyRoundTrip(new Spline(3,
                new Point[]
                {
                    new Point(0.0, 0.0, 0.0),
                    new Point(1.0, 1.0, 0.0),
                    new Point(2.0, 2.0, 0.0),
                    new Point(3.0, 3.0, 0.0),
                },
                new double[]
                {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                }));
        }

        [Fact]
        public async Task ReadEllipseAndCorrectMinorAxisRatioTest_DefaultStartAndEndParameters()
        {
            var drawing = await ReadFromDxfContent(
                ("  0", "SECTION"),
                ("  2", "ENTITIES"),
                ("  0", "ELLIPSE"),
                (" 10", "0"), // center
                (" 20", "0"),
                (" 30", "0"),
                (" 11", "1"), // major axis
                (" 21", "0"),
                (" 31", "0"),
                (" 40", "2") // minor axis ratio
            );
            var ellipse = (Ellipse)drawing.GetEntities().Single();
            Assert.Equal(new Point(0.0, 0.0, 0.0), ellipse.Center);
            Assert.Equal(new Vector(0.0, 2.0, 0.0), ellipse.MajorAxis);
            Assert.Equal(0.5, ellipse.MinorAxisRatio);
            AssertClose(0.0, ellipse.StartAngle);
            AssertClose(360.0, ellipse.EndAngle);
        }

        [Fact]
        public async Task ReadEllipseAndCorrectMinorAxisRatioTest_CustomStartAndEndParameters()
        {
            var drawing = await ReadFromDxfContent(
                ("  0", "SECTION"),
                ("  2", "ENTITIES"),
                ("  0", "ELLIPSE"),
                (" 10", "0"), // center
                (" 20", "0"),
                (" 30", "0"),
                (" 11", "1"), // major axis
                (" 21", "0"),
                (" 31", "0"),
                (" 40", "2"), // minor axis ratio
                (" 41", "0"), // start angle in radians (0 degrees)
                (" 42", "1.5707963267948966") // end angle in radians (90 degrees)
            );
            var ellipse = (Ellipse)drawing.GetEntities().Single();
            Assert.Equal(new Point(0.0, 0.0, 0.0), ellipse.Center);
            Assert.Equal(new Vector(0.0, 2.0, 0.0), ellipse.MajorAxis);
            Assert.Equal(0.5, ellipse.MinorAxisRatio);
            AssertClose(270.0, ellipse.StartAngle);
            AssertClose(360.0, ellipse.EndAngle);
        }

        [Fact]
        public async Task ReadAlignedDimensionTest()
        {
            var drawing = await ReadFromDxfContent(
                ("  0", "SECTION"),
                ("  2", "ENTITIES"),
                ("  0", "DIMENSION"),
                (" 10", "3.7567067079130281"), // dimension line location
                (" 20", "3.4324699690652278"),
                (" 30", "0.0"),
                (" 11", "2.2567067079130281"), // text midpoint
                (" 21", "1.4324699690652281"),
                (" 31", "0.0"),
                (" 70", "     1"), // aligned = true
                (" 13", "0.0"), // definition point 1
                (" 23", "0.0"),
                (" 33", "0.0"),
                (" 14", "3.0"), // definition point 2
                (" 24", "4.0"),
                (" 34", "0.0"),
                ("  0", "ENDSEC"),
                ("  0", "EOF")
            );
            var dim = (LinearDimension)drawing.GetEntities().Single();
            Assert.True(dim.IsAligned);
            AssertClose(new Point(0.0, 0.0, 0.0), dim.DefinitionPoint1);
            AssertClose(new Point(3.0, 4.0, 0.0), dim.DefinitionPoint2);
            AssertClose(new Point(3.756706707913028, 3.4324699690652278, 0.0), dim.DimensionLineLocation);
            AssertClose(new Point(2.2567067079130281, 1.4324699690652281, 0.0), dim.TextMidPoint);
        }

        [Fact]
        public async Task WriteAlignedDimensionTest()
        {
            var drawing = new Drawing();
            drawing = drawing.Update(settings: drawing.Settings.Update(dimStyles: drawing.Settings.DimensionStyles.Add(new DimensionStyle("my-dimension-style"))));
            drawing = drawing
                .AddToCurrentLayer(new LinearDimension(
                    new Point(0.0, 0.0, 0.0),
                    new Point(3.0, 4.0, 0.0),
                    new Point(0.7567067079130286, -0.5675300309347714, 0.0),
                    true,
                    new Point(1.0, 2.0, 0.0),
                    "my-dimension-style"));
            var actual = await GetDxfCodePairs(drawing);
            var expected = new[]
            {
                (" 10", "0.756706707913029"),
                (" 20", "-0.567530030934771"),
                (" 30", "0.0"),
                (" 11", "1.0"),
                (" 21", "2.0"),
                (" 31", "0.0"),
                (" 70", "     1"), // aligned = true
                ("  1", "<>"), // auto-text
                ("  3", "my-dimension-style"),
                (" 13", "0.0"),
                (" 23", "0.0"),
                (" 33", "0.0"),
                (" 14", "3.0"),
                (" 24", "4.0"),
                (" 34", "0.0"),
            };
            AssertContains(expected, actual);
        }

        [Fact]
        public async Task ReadLinearDimensionTest()
        {
            var drawing = await ReadFromDxfContent(
                ("  0", "SECTION"),
                ("  2", "ENTITIES"),
                ("  0", "DIMENSION"),
                (" 10", "3.0"), // dimension line location
                (" 20", "5.0636363959996498"),
                (" 30", "0.0"),
                (" 11", "1.5"), // text midpoint
                (" 21", "5.0636363959996498"),
                (" 31", "0.0"),
                (" 70", "     0"), // aligned = false
                (" 13", "0.0"), // definition point 1
                (" 23", "0.0"),
                (" 33", "0.0"),
                (" 14", "3.0"), // definition point 2
                (" 24", "4.0"),
                (" 34", "0.0"),
                ("  0", "ENDSEC"),
                ("  0", "EOF")
            );
            var dim = (LinearDimension)drawing.GetEntities().Single();
            Assert.False(dim.IsAligned);
            AssertClose(new Point(0.0, 0.0, 0.0), dim.DefinitionPoint1);
            AssertClose(new Point(3.0, 4.0, 0.0), dim.DefinitionPoint2);
            AssertClose(new Point(3.0, 5.06363639599965, 0.0), dim.DimensionLineLocation);
            AssertClose(new Point(1.5, 5.06363639599965, 0.0), dim.TextMidPoint);
        }

        [Fact]
        public async Task WriteLinearDimensionTest()
        {
            var drawing = new Drawing();
            drawing = drawing.Update(settings: drawing.Settings.Update(dimStyles: drawing.Settings.DimensionStyles.Add(new DimensionStyle("my-dimension-style"))));
            drawing = drawing
                .AddToCurrentLayer(new LinearDimension(
                    new Point(0.0, 0.0, 0.0),
                    new Point(3.0, 4.0, 0.0),
                    new Point(0.0, 5.06363639599965, 0.0),
                    false,
                    new Point(1.0, 2.0, 0.0),
                    "my-dimension-style"));
            var actual = await GetDxfCodePairs(drawing);
            var expected = new[]
            {
                (" 10", "0.0"),
                (" 20", "5.06363639599965"),
                (" 30", "0.0"),
                (" 11", "1.0"),
                (" 21", "2.0"),
                (" 31", "0.0"),
                (" 70", "     0"), // aligned = false
                ("  1", "<>"), // auto-text
                ("  3", "my-dimension-style"),
                (" 13", "0.0"),
                (" 23", "0.0"),
                (" 33", "0.0"),
                (" 14", "3.0"),
                (" 24", "4.0"),
                (" 34", "0.0"),
                (" 50", "0.0"), // rotation angle 0
            };
            AssertContains(expected, actual);
        }

        [Theory]
        [InlineData(null, null)] // auto-set
        [InlineData("<>", null)]
        [InlineData(" ", "")] // suppressed
        [InlineData("123", "123")] // set specific
        public async Task ReadDimensionTextTests(string dxfTextSpecification, string expectedTextOverride)
        {
            var pairs = new List<(string, string)>()
            {
                ("  0", "SECTION"),
                ("  2", "ENTITIES"),
                ("  0", "DIMENSION"),
                (" 70", "     1"), // aligned = true
            };
            if (dxfTextSpecification != null)
            {
                pairs.Add(("  1", dxfTextSpecification));
            }

            pairs.Add(("  0", "ENDSEC"));
            pairs.Add(("  0", "EOF"));
            var drawing = await ReadFromDxfContent(pairs.ToArray());
            var dim = (LinearDimension)drawing.GetEntities().Single();
            Assert.Equal(expectedTextOverride, dim.TextOverride);
        }

        [Theory]
        [InlineData(null, "<>")] // auto-set
        [InlineData("", " ")] // suppressed
        [InlineData("123", "123")] // set specific
        public async Task WriteDimensionTextTests(string entityTextOverride, string expectedDxfText)
        {
            var drawing = new Drawing()
                .AddToCurrentLayer(new LinearDimension(
                    new Point(),
                    new Point(),
                    new Point(),
                    true,
                    new Point(),
                    "STANDARD",
                    entityTextOverride));
            var actual = await GetDxfCodePairs(drawing);
            var entitiesStartIndex = -1;
            var entitiesEndIndex = -1;
            for (int i = 0; i < actual.Length; i++)
            {
                var pair = actual[i];
                if (entitiesStartIndex >= 0)
                {
                    // looking for end
                    if (pair.Item1 == "  0" && pair.Item2 == "ENDSEC")
                    {
                        entitiesEndIndex = i;
                        break;
                    }
                }
                else
                {
                    // looking for start
                    if (pair.Item1 == "  2" && pair.Item2 == "ENTITIES")
                    {
                        entitiesStartIndex = i;
                    }
                }
            }
            var actualTrimmed = actual.Skip(entitiesStartIndex).Take(entitiesEndIndex - entitiesStartIndex).ToArray();

            // look for it specifically
            var found = actualTrimmed.Single(pair => pair.Item1 == "  1" && pair.Item2 == expectedDxfText);
        }

        [Fact]
        public async Task ReadImageTest()
        {
            var pngPath = "test-image.png";
            var pngData = File.ReadAllBytes(pngPath);
            var dxfContents = CreateDxfContent(
                ("  0", "SECTION"),
                ("  2", "ENTITIES"),
                ("  0", "IMAGE"),
                (" 10", "1.0"), // location
                (" 20", "2.0"),
                (" 30", "3.0"),
                (" 11", "1.0"), // u vector
                (" 21", "0.0"),
                (" 31", "0.0"),
                (" 12", "0.0"), // v vector
                (" 22", "1.0"),
                (" 32", "0.0"),
                (" 13", "2.0"), // image size in pixels
                (" 23", "2.0"),
                ("340", "AAAA"), // imagedef handle
                ("  0", "ENDSEC"),
                ("  0", "SECTION"),
                ("  2", "OBJECTS"),
                ("  0", "IMAGEDEF"),
                ("  5", "AAAA"), // handle (corresponds to code 340 above)
                ("  1", "test-image.png"),
                (" 10", "2.0"), // size in pixels
                (" 20", "2.0"),
                ("  0", "ENDSEC"));
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, new UTF8Encoding(false)))
            {
                writer.WriteLine(dxfContents);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var dxfFile = DxfFile.Load(ms);
                var dxfImage = (DxfImage)dxfFile.Entities.Single();
                var image = (Image)await dxfImage.ToEntity(Workspace.FileSystemService.ReadAllBytesAsync, new Dictionary<string, IEnumerable<DxfEntity>>());
                Assert.Equal(new Point(1.0, 2.0, 3.0), image.Location);
                Assert.Equal(pngPath, image.Path);
                Assert.Equal(pngData, image.ImageData);
                Assert.Equal(2.0, image.Width);
                Assert.Equal(2.0, image.Height);
                Assert.Equal(0.0, image.Rotation);
            }
        }

        [Fact]
        public async Task RoundTripImageTest()
        {
            // `test-image.png` is a solid red 4x4 image
            var imagePath = "test-image.png";
            var imageData = File.ReadAllBytes(imagePath);
            await VerifyRoundTrip(new Image(
                new Point(1.0, 2.0, 3.0),
                imagePath,
                imageData,
                8.0,
                8.0,
                0.0));
        }

        [Fact]
        public async Task ReadSolidTest()
        {
            var drawing = await ReadFromDxfContent(
                ("  0", "SECTION"),
                ("  2", "ENTITIES"),
                ("  0", "SOLID"),
                (" 10", "1.0"),
                (" 20", "2.0"),
                (" 30", "0.0"),
                (" 11", "3.0"),
                (" 21", "4.0"),
                (" 31", "0.0"),
                (" 12", "7.0"), // n.b., the dxf representation of a solid swaps the last two vertices
                (" 22", "8.0"),
                (" 32", "0.0"),
                (" 13", "5.0"),
                (" 23", "6.0"),
                (" 33", "0.0"),
                ("  0", "ENDSEC"),
                ("  0", "EOF"));
            var solid = Assert.IsType<Solid>(drawing.GetEntities().Single());
            Assert.Equal(new Point(1.0, 2.0, 0.0), solid.P1);
            Assert.Equal(new Point(3.0, 4.0, 0.0), solid.P2);
            Assert.Equal(new Point(5.0, 6.0, 0.0), solid.P3);
            Assert.Equal(new Point(7.0, 8.0, 0.0), solid.P4);
        }

        [Fact]
        public async Task WriteSolidTest()
        {
            var drawing = new Drawing()
                .AddToCurrentLayer(new Solid(
                    new Point(1.0, 2.0, 0.0),
                    new Point(3.0, 4.0, 0.0),
                    new Point(5.0, 6.0, 0.0),
                    new Point(7.0, 8.0, 0.0)));
            var actual = await GetDxfCodePairs(drawing);
            var expected = new[]
            {
                (" 10", "1.0"),
                (" 20", "2.0"),
                (" 30", "0.0"),
                (" 11", "3.0"),
                (" 21", "4.0"),
                (" 31", "0.0"),
                (" 12", "7.0"), // n.b., the dxf representation of a solid swaps the last two vertices
                (" 22", "8.0"),
                (" 32", "0.0"),
                (" 13", "5.0"),
                (" 23", "6.0"),
                (" 33", "0.0"),
            };
            AssertContains(expected, actual);
        }

        [Fact]
        public async Task RoundTripLinearDimensionTest()
        {
            await VerifyRoundTrip(new LinearDimension(
                new Point(1.0, 2.0, 0.0),
                new Point(3.0, 4.0, 0.0),
                new Point(5.0, 6.0, 0.0),
                true,
                new Point(7.0, 8.0, 0.0),
                "STANDARD",
                "text-override",
                CadColor.Green,
                CadColor.Red));
        }

        [Fact]
        public async Task RoundTripLayerTest()
        {
            // don't try to round-trip a null layer color; DXF always resets it to white
            await VerifyRoundTrip(new Layer("name", color: CadColor.White));
            await VerifyRoundTrip(new Layer("name", color: CadColor.Red));
            await VerifyRoundTrip(new Layer("name", color: CadColor.White, isVisible: false));
        }

        [Fact]
        public async Task RoundTripLineTypeTest()
        {
            var lineType = new LineType("custom-line-type", new[] { 0.5, 0.5 }, "some description");
            var drawing = new Drawing()
                .Update(lineTypes: new ReadOnlyTree<string, LineType>().Insert(lineType.Name, lineType));
            var roundTrippedDrawing = await RoundTripDrawing(drawing);
            var roundTrippedLineType = roundTrippedDrawing.LineTypes.GetValue(lineType.Name);
            Assert.Equal("custom-line-type", roundTrippedLineType.Name);
            Assert.Equal(new[] { 0.5, 0.5 }, roundTrippedLineType.Pattern);
            Assert.Equal("some description", roundTrippedLineType.Description);
        }

        [Fact]
        public async Task LayersAreNotDuplicatedOnSave()
        {
            var drawing = new Drawing();
            drawing = drawing.Update(layers: drawing.Layers.Insert("0", new Layer("0")));

            var dxf = await WriteToFile(drawing);
            Assert.Equal("0", dxf.Layers.Single().Name);
        }

        [Fact]
        public async Task VerifyDefaultDxfVersionInDirectWriteTest()
        {
            var file = await WriteToFile(new Drawing());
            Assert.Equal(DxfAcadVersion.R12, file.Header.Version);
        }

        [Fact]
        public async Task VerifyNonDefaultVersionIsPreservedInDirectWriteTest()
        {
            var file = new DxfFile();
            file.Header.Version = DxfAcadVersion.R2007;
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var drawing = await ReadFromStream(ms);
                var result = await WriteToFile(drawing);

                // file read from disk has preserved version
                Assert.Equal(DxfAcadVersion.R2007, result.Header.Version);
            }
        }

        [Fact]
        public async void VerifyNonDefaultVersionIsPreservedInWriteTest()
        {
            using (var ms1 = new MemoryStream())
            {
                var drawing = new Drawing();

                // write file as R13
                using (new DxfFileSettingsProvider(new DxfFileSettings() { FileVersion = DxfFileVersion.R13 }))
                {
                    Assert.True(await Workspace.ReaderWriterService.TryWriteDrawing("filename.dxf", drawing, ViewPort.CreateDefaultViewPort(), ms1, preserveSettings: false));
                }

                // verify that it was written as such
                ms1.Seek(0, SeekOrigin.Begin);
                var file = DxfFile.Load(ms1);
                Assert.Equal(DxfAcadVersion.R13, file.Header.Version);

                using (var ms2 = new MemoryStream())
                {
                    // write it again
                    Assert.True(await Workspace.ReaderWriterService.TryWriteDrawing("filename.dxf", drawing, ViewPort.CreateDefaultViewPort(), ms2, preserveSettings: true));

                    // verify again that it was written correctly without specifying drawing settings
                    ms2.Seek(0, SeekOrigin.Begin);
                    file = DxfFile.Load(ms2);
                    Assert.Equal(DxfAcadVersion.R13, file.Header.Version);
                }
            }
        }

        [Fact]
        public async Task ReadLwPolylineTest()
        {
            var lwpoly = new DxfLwPolyline(new[]
            {
                new DxfLwPolylineVertex() { X = 1.0, Y = 2.0 },
                new DxfLwPolylineVertex() { X = 3.0, Y = 4.0 }
            })
            {
                Elevation = 12.0
            };
            var poly = (Polyline)await lwpoly.ToEntity(Workspace.FileSystemService.ReadAllBytesAsync, new Dictionary<string, IEnumerable<DxfEntity>>());
            Assert.Equal(2, poly.Vertices.Count());
            Assert.Equal(new Point(1.0, 2.0, 12.0), poly.Vertices.First().Location);
            Assert.Equal(new Point(3.0, 4.0, 12.0), poly.Vertices.Last().Location);
        }

        [Fact]
        public async Task MissingLayersAreAddedOnRead()
        {
            var dxfContents = string.Join("\r\n",
                new[]
                {
                    ("  0", "SECTION"),
                    ("  2", "ENTITIES"),
                    ("  0", "LINE"),
                    ("  8", "not-a-layer"), // entity is on a layer that wasn't defined
                    ("  0", "ENDSEC"),
                }.Select(pair => $"{pair.Item1}\r\n{pair.Item2}"));
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, new UTF8Encoding(false));
            writer.WriteLine(dxfContents);
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            var result = await FileHandler.ReadDrawing(null, ms, null);
            Assert.True(result.Success);
            Assert.NotNull(result.Drawing.Layers.GetValue("not-a-layer"));
        }

        [Fact]
        public async Task MissingLineTypesAreAddedOnRead()
        {
            var dxfContents = string.Join("\r\n",
                new[]
                {
                    ("  0", "SECTION"),
                    ("  2", "ENTITIES"),
                    ("  0", "LINE"),
                    ("  6", "not-a-line-type"), // entity uses a line type that's not in the file
                    ("  0", "ENDSEC"),
                }.Select(pair => $"{pair.Item1}\r\n{pair.Item2}"));
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, new UTF8Encoding(false));
            writer.WriteLine(dxfContents);
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            var result = await FileHandler.ReadDrawing(null, ms, null);
            Assert.True(result.Success);
            Assert.NotNull(result.Drawing.LineTypes.GetValue("not-a-line-type"));
        }

        [Fact]
        public async Task RoundTripFilletRadius()
        {
            var drawing = new Drawing();
            Assert.NotEqual(6.0, drawing.Settings.FilletRadius);
            drawing = drawing.Update(settings: drawing.Settings.Update(filletRadius: 6.0));
            var roundTripped = await RoundTripDrawing(drawing);
            Assert.Equal(6.0, roundTripped.Settings.FilletRadius);
        }
    }
}
