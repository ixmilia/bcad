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
    }
}
