using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected override Task<Entity> RoundTripEntity(Entity entity)
        {
            // shortcut to avoid file writing/reading
            return entity.ToDxfEntity(new Layer("layer")).ToEntity(Workspace.FileSystemService.ReadAllBytesAsync);
        }

        private async Task<DxfFile> WriteToFile(Drawing drawing)
        {
            using (var stream = await WriteToStream(drawing))
            {
                return DxfFile.Load(stream);
            }
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
        public async Task ReadImageTest()
        {
            var pngPath = "test-image.png";
            var pngData = File.ReadAllBytes(pngPath);
            var dxfContents = string.Join("\r\n",
                new[]
                {
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
                    ("  0", "ENDSEC"),
                }.Select(pair => $"{pair.Item1}\r\n{pair.Item2}"));
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, new UTF8Encoding(false)))
            {
                writer.WriteLine(dxfContents);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var dxfFile = DxfFile.Load(ms);
                var dxfImage = (DxfImage)dxfFile.Entities.Single();
                var image = (Image)(await dxfImage.ToEntity(Workspace.FileSystemService.ReadAllBytesAsync));
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
        public async Task RoundTripLayerTest()
        {
            // don't try to round-trip a null layer color; DXF always resets it to white
            await VerifyRoundTrip(new Layer("name", color: CadColor.White));
            await VerifyRoundTrip(new Layer("name", color: CadColor.Red));
            await VerifyRoundTrip(new Layer("name", color: CadColor.White, isVisible: false));
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
            var poly = (Polyline)(await lwpoly.ToEntity(Workspace.FileSystemService.ReadAllBytesAsync));
            Assert.Equal(2, poly.Vertices.Count());
            Assert.Equal(new Point(1.0, 2.0, 12.0), poly.Vertices.First().Location);
            Assert.Equal(new Point(3.0, 4.0, 12.0), poly.Vertices.Last().Location);
        }
    }
}
