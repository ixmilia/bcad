using IxMilia.BCad.Primitives;
using IxMilia.BCad.Utilities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class TangentTests : TestBase
    {
        [Fact]
        public void GenerateTangentLineBetweenPointAndCircle()
        {
            var point = new Point(0.0, 0.0, 0.0);
            var circle = new PrimitiveEllipse(new Point(1.0, 0.0, 0.0), 0.5, Vector.ZAxis);
            var lines = EditUtilities.TangentLine(point, circle);
            var line1 = lines.Value.Item1;
            var line2 = lines.Value.Item2;
            Assert.Equal(line1.P1, point);
            Assert.Equal(new Point(0.75, 0.4330127018922193, 0.0), line1.P2);
            Assert.Equal(line2.P1, point);
            Assert.Equal(new Point(0.75,-0.4330127018922193, 0.0), line2.P2);
        }

        [Fact]
        public void CannotGenerateTangentLineBetweenPointAndEllipse()
        {
            var point = new Point(0.0, 0.0, 0.0);
            var ellipse = new PrimitiveEllipse(new Point(1.0, 0.0, 0.0), new Vector(0.5, 0.0, 0.0), Vector.ZAxis, 0.5, 0.0, 360.0);
            var tangentLines = EditUtilities.TangentLine(point, ellipse);
            Assert.Null(tangentLines);
        }
    }
}
