using Xunit;

namespace BCad.Test.DxfTests
{
    public class DxfHeaderTests : AbstractDxfTests
    {
        [Fact]
        public void DefaultHeaderValuesTest()
        {
            var file = Section("HEADER", "");
            Assert.Null(file.CurrentLayer);
        }

        [Fact]
        public void SpecificHeaderValuesTest()
        {
            var file = Section("HEADER", @"
9
$CLAYER
8
<current layer>
");
            Assert.Equal("<current layer>", file.CurrentLayer);
        }
    }
}
