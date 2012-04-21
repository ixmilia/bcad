using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BCad.Test
{
    public class VectorTests
    {
        [Fact]
        public void NormalizeTest()
        {
            var v = new Vector(3, 4, 0);
            Assert.Equal(1.0, v.Normalize().Length);
        }
    }
}
