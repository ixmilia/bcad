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

        [Fact]
        public void LayerTableTest()
        {
            var file = Section("TABLES", @"
  0
TABLE
  2
LAYER
  0
LAYER
  2
a
 62
12
  0
LAYER
  2
b
 62
13
  0
ENDTAB
");
            var layers = file.TablesSection.LayerTable.Layers;
            Assert.Equal(2, layers.Count);
            Assert.Equal("a", layers[0].Name);
            Assert.Equal(12, layers[0].Color.RawValue);
            Assert.Equal("b", layers[1].Name);
            Assert.Equal(13, layers[1].Color.RawValue);
        }
    }
}
