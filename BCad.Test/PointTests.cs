using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BCad.Test
{
    public class PointTests
    {
        [Fact]
        public void EqualityTests()
        {
            Assert.Equal(new Point(0, 0, 0), new Point(0, 0, 0));
            Assert.Equal(new Point(1, 1, 1), new Point(1, 1, 1));
            Assert.NotEqual(new Point(0, 0, 0), new Point(1, 0, 0));
        }

        [Fact]
        public void PointParsingTests()
        {
            Assert.Equal(new Point(0, 0, 0), Point.Parse("0,0,0"));
        }
    }
}
