using Xunit;

namespace BCad.Test
{
    public class ViewPortTests
    {
        [Fact]
        public void TransformationMatrixDirect3DTest()
        {
            var viewPort = new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 100.0);
            var transform = viewPort.GetTransformationMatrixDirect3DStyle(200, 100);
            var inverse = transform.Inverse();
            // view port
            // 0,100     200,100
            //
            // 0,0         200,0
            //
            // screen
            // -1,1          1,1
            //
            // -1,-1        1,-1

            // center
            Assert.Equal(new Point(0, 0, 0), transform.Transform(new Point(100, 50, 0)));
            Assert.Equal(new Point(100, 50, 0), inverse.Transform(new Point(0, 0, 0)));

            // bottom left
            Assert.Equal(new Point(-1, -1, 0), transform.Transform(new Point(0, 0, 0)));
            Assert.Equal(new Point(0, 0, 0), inverse.Transform(new Point(-1, -1, 0)));

            // bottom right
            Assert.Equal(new Point(1, -1, 0), transform.Transform(new Point(200, 0, 0)));
            Assert.Equal(new Point(200, 0, 0), inverse.Transform(new Point(1, -1, 0)));

            // top left
            Assert.Equal(new Point(-1, 1, 0), transform.Transform(new Point(0, 100, 0)));
            Assert.Equal(new Point(0, 100, 0), inverse.Transform(new Point(-1, 1, 0)));

            // top right
            Assert.Equal(new Point(1, 1, 0), transform.Transform(new Point(200, 100, 0)));
            Assert.Equal(new Point(200, 100, 0), inverse.Transform(new Point(1, 1, 0)));
        }

        [Fact]
        public void TransformationMatrixWindowsTest()
        {
            var viewPort = new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 100.0);
            var transform = viewPort.GetTransformationMatrixWindowsStyle(100, 50);
            var inverse = transform.Inverse();
            // view port
            // 0,100     200,100
            //
            // 0,0         200,0
            //
            // screen
            // 0,0         100,0
            //
            // 0,50       100,50

            // center
            Assert.Equal(new Point(50, 25, 0), transform.Transform(new Point(100, 50, 0)));
            Assert.Equal(new Point(100, 50, 0), inverse.Transform(new Point(50, 25, 0)));

            // bottom left
            Assert.Equal(new Point(0, 50, 0), transform.Transform(new Point(0, 0, 0)));
            Assert.Equal(new Point(0, 0, 0), inverse.Transform(new Point(0, 50, 0)));

            // bottom right
            Assert.Equal(new Point(100, 50, 0), transform.Transform(new Point(200, 0, 0)));
            Assert.Equal(new Point(200, 0, 0), inverse.Transform(new Point(100, 50, 0)));

            // top left
            Assert.Equal(new Point(0, 0, 0), transform.Transform(new Point(0, 100, 0)));
            Assert.Equal(new Point(0, 100, 0), inverse.Transform(new Point(0, 0, 0)));

            // top right
            Assert.Equal(new Point(100, 0, 0), transform.Transform(new Point(200, 100, 0)));
            Assert.Equal(new Point(200, 100, 0), inverse.Transform(new Point(100, 0, 0)));
        }

        [Fact]
        public void TransformationMatrixWindowsNonCenteredTest()
        {
            var viewPort = new ViewPort(new Point(100, 100, 0), Vector.ZAxis, Vector.YAxis, 100.0);
            var transform = viewPort.GetTransformationMatrixWindowsStyle(100, 100);
            var inverse = transform.Inverse();
            // view port
            // 100,200   200,200
            //      150,150
            // 100,100   200,100
            //
            // screen
            // 0,0         100,0
            //      50,50
            // 0,100     100,100

            // center
            Assert.Equal(new Point(50, 50, 0), transform.Transform(new Point(150, 150, 0)));
            Assert.Equal(new Point(150, 150, 0), inverse.Transform(new Point(50, 50, 0)));

            // bottom left
            Assert.Equal(new Point(0, 100, 0), transform.Transform(new Point(100, 100, 0)));
            Assert.Equal(new Point(100, 100, 0), inverse.Transform(new Point(0, 100, 0)));

            // bottom right
            Assert.Equal(new Point(100, 100, 0), transform.Transform(new Point(200, 100, 0)));
            Assert.Equal(new Point(200, 100, 0), inverse.Transform(new Point(100, 100, 0)));

            // top left
            Assert.Equal(new Point(0, 0, 0), transform.Transform(new Point(100, 200, 0)));
            Assert.Equal(new Point(100, 200, 0), inverse.Transform(new Point(0, 0, 0)));

            // top right
            Assert.Equal(new Point(100, 0, 0), transform.Transform(new Point(200, 200, 0)));
            Assert.Equal(new Point(200, 200, 0), inverse.Transform(new Point(100, 0, 0)));
        }
    }
}
