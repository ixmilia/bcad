using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    public class JsonFileHandlerTests : FileHandlerTestsBase
    {
        public override IFileHandler FileHandler => new JsonFileHandler();

        private DrawingSettings DrawingSettings { get; } = new DrawingSettings();

        private Task VerifyRoundTrip(Entity entity) => VerifyRoundTrip(entity, DrawingSettings);

        [Fact]
        public async Task RoundTripLayerTest()
        {
            await VerifyRoundTrip(new Layer("green-layer", color: CadColor.Green));
        }

        [Fact]
        public async Task RoundTripArcTest()
        {
            await VerifyRoundTrip(new Arc(new Point(1.0, 2.0, 0.0), 3.0, 4.0, 5.0, Vector.ZAxis));
        }

        [Fact]
        public async Task RoundTripLineTest()
        {
            await VerifyRoundTrip(new Line(new Point(1.0, 2.0, 0.0), new Point(3.0, 4.0, 0.0)));
        }

        [Fact]
        public async Task ReadDrawingTest()
        {
            var rawJson = """
                {
                    "floorplan": {
                        "bnds": {
                            "x": 0.0,
                            "y": 0.0,
                            "w": 0.0,
                            "h": 0.0
                        },
                        "lyrs": [
                            {
                                "n": "0",
                                "r": 0,
                                "g": 0,
                                "b": 0
                            }
                        ],
                        "blks": [
                            {
                                "n": "*Model_Space",
                                "h": null,
                                "ents": [
                                    {
                                        "t": "L",
                                        "h": null,
                                        "p": null,
                                        "l": "0",
                                        "d": "1,2,3|4,5,6"
                                    }
                                ]
                            }
                        ]
                    }
                }
                """;
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms);
            await writer.WriteAsync(rawJson);
            await writer.FlushAsync();
            ms.Seek(0, SeekOrigin.Begin);
            var drawingResult = await FileHandler.ReadDrawing("filename", ms, Workspace.FileSystemService.ReadAllBytesAsync);
            Assert.True(drawingResult.Success);
            Assert.Equal(1, drawingResult.Drawing.Layers.Count);
            var entity = Assert.Single(drawingResult.Drawing.Layers.GetValue("0").GetEntities());
            var line = Assert.IsType<Line>(entity);
            Assert.Equal(new Point(1.0, 2.0, 3.0), line.P1);
            Assert.Equal(new Point(4.0, 5.0, 6.0), line.P2);
        }
    }
}
