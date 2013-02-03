using System.IO;
using BCad.Stl;
using Xunit;

namespace BCad.Test.StlTests
{
    public class StlWriterTests
    {
        [Fact]
        public void AsciiWriterTest()
        {
            var file = new StlFile();
            file.SolidName = "foo";
            file.Triangles.Add(new StlTriangle(new StlVertex(1, 2, 3), new StlVertex(4, 5, 6), new StlVertex(7, 8, 9), new StlNormal(10, 11, 12)));
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamReader(stream).ReadToEnd();
            Assert.Equal(@"solid foo
  facet normal 1.000000e+001 1.100000e+001 1.200000e+001
    outer loop
      vertex 1.000000e+000 2.000000e+000 3.000000e+000
      vertex 4.000000e+000 5.000000e+000 6.000000e+000
      vertex 7.000000e+000 8.000000e+000 9.000000e+000
    endloop
  endfacet
endsolid foo
", content);
        }
    }
}
