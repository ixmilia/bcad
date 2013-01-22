using BCad.Dxf;
using Xunit;

namespace BCad.Test.DxfTests
{
    public class DxfHeaderTests : AbstractDxfTests
    {
        [Fact]
        public void DefaultHeaderValuesTest()
        {
            var file = Section("HEADER", "");
            Assert.Null(file.HeaderSection.CurrentLayer);
            Assert.Equal(DxfAcadVersion.R14, file.HeaderSection.Version);
        }

        [Fact]
        public void SpecificHeaderValuesTest()
        {
            var file = Section("HEADER", @"
  9
$CLAYER
  8
<current layer>
  9
$ACADVER
  1
AC1012
");
            Assert.Equal("<current layer>", file.HeaderSection.CurrentLayer);
            Assert.Equal(DxfAcadVersion.R13, file.HeaderSection.Version);
        }
    }
}
