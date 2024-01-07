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

        [Fact]
        public void GenerateTangentLineBetweenTwoCircles_SameRadius()
        {
            var c1 = new PrimitiveEllipse(new Point(0.0, 0.0, 0.0), 1.0, Vector.ZAxis);
            var c2 = new PrimitiveEllipse(new Point(3.0, 0.0, 0.0), 1.0, Vector.ZAxis);
            var lines = EditUtilities.TangentLine(c1, c2);
            var line1 = lines.Value.Item1;
            var line2 = lines.Value.Item2;
            Assert.Equal(new Point(0.0, -1.0, 0.0), line1.P1);
            Assert.Equal(new Point(3.0, -1.0, 0.0), line1.P2);
            Assert.Equal(new Point(0.0, 1.0, 0.0), line2.P1);
            Assert.Equal(new Point(3.0, 1.0, 0.0), line2.P2);
        }

        [Fact]
        public void GenerateTangentLineBetweenTwoCircles_DifferentRadius()
        {
            var c1 = new PrimitiveEllipse(new Point(0.0, 0.0, 0.0), 1.0, Vector.ZAxis);
            var c2 = new PrimitiveEllipse(new Point(3.0, 0.0, 0.0), 1.5, Vector.ZAxis);
            var lines = EditUtilities.TangentLine(c1, c2);
            var line1 = lines.Value.Item1;
            var line2 = lines.Value.Item2;
            Assert.Equal(new Point(-0.1666666666674352, 0.9860132971831395, 0.0), line1.P1);
            Assert.Equal(new Point(2.7499999999985647, 1.4790199457769997, 0.0), line1.P2);
            Assert.Equal(new Point(-0.1666666666674352, -0.9860132971831395, 0.0), line2.P1);
            Assert.Equal(new Point(2.7499999999985647, -1.4790199457769997, 0.0), line2.P2);
        }
    }
}
