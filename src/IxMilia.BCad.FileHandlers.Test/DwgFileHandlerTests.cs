using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    // The DWG file handler is implemented by the DXF file handler then using the DXF->DWG converter, so we only need a
    // few quick checks here.
    public class DwgFileHandlerTests : FileHandlerTestsBase
    {
        public override IFileHandler FileHandler => new DwgFileHandler();

        private DrawingSettings DrawingSettings { get; } = new DrawingSettings();

        private Task VerifyRoundTrip(Entity entity) => VerifyRoundTrip(entity, DrawingSettings);

        [Fact]
        public async Task RoundTripLineTest()
        {
            await VerifyRoundTrip(new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), CadColor.Green, thickness: 1.2345));
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
