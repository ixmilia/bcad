using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.FileHandlers.Extensions;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    public class IgesFileHandlerTests : FileHandlerTestsBase
    {
        public override IFileHandler FileHandler => new IgesFileHandler();

        protected override Task<Entity> RoundTripEntity(Entity entity)
        {
            // shortcut to avoid file writing/reading
            var roundTripped = entity.ToIgesEntity().ToEntity();
            return Task.FromResult(roundTripped);
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
            await VerifyRoundTrip(new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, Vector.ZAxis));
        }

        [Fact]
        public async Task RoundTripCircleTest()
        {
            await VerifyRoundTrip(new Circle(new Point(1.0, 2.0, 3.0), 4.0, Vector.ZAxis));
        }

        [Fact]
        public async Task RoundTripLineTest()
        {
            await VerifyRoundTrip(new Line(new Point(1.0, 2.0, 3.0), new Point(1.0, 2.0, 3.0)));
        }
    }
}
