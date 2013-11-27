using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            // bottom left
            Assert.Equal(new Point(-1, -1, 0), transform.Transform(new Point(0, 0, 0)));

            // bottom right
            Assert.Equal(new Point(1, -1, 0), transform.Transform(new Point(200, 0, 0)));

            // top left
            Assert.Equal(new Point(-1, 1, 0), transform.Transform(new Point(0, 100, 0)));

            // top right
            Assert.Equal(new Point(1, 1, 0), transform.Transform(new Point(200, 100, 0)));
        }

        [Fact]
        public void TransformationMatrixWindowsTest()
        {
            var viewPort = new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 100.0);
            var transform = viewPort.GetTransformationMatrixWindowsStyle(100, 50);
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

            // bottom left
            Assert.Equal(new Point(0, 50, 0), transform.Transform(new Point(0, 0, 0)));

            // bottom right
            Assert.Equal(new Point(100, 50, 0), transform.Transform(new Point(200, 0, 0)));

            // top left
            Assert.Equal(new Point(0, 0, 0), transform.Transform(new Point(0, 100, 0)));

            // top right
            Assert.Equal(new Point(100, 0, 0), transform.Transform(new Point(200, 100, 0)));
        }
    }
}
