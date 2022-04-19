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

        [Fact]
        public async Task RoundTripLineTest()
        {
            await VerifyRoundTrip(new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), CadColor.Green, thickness: 1.2345));
        }
    }
}
