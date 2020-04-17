using Xunit;

namespace IxMilia.BCad.Core.Test
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
