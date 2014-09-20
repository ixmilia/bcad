using System.IO;
using IxMilia.Stl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.StlTests
{
    [TestClass]
    public class StlWriterTests
    {
        [TestMethod]
        public void AsciiWriterTest()
        {
            var file = new StlFile();
            file.SolidName = "foo";
            file.Triangles.Add(new StlTriangle(new StlNormal(1, 2, 3), new StlVertex(4, 5, 6), new StlVertex(7, 8, 9), new StlVertex(10, 11, 12)));
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamReader(stream).ReadToEnd();
            Assert.AreEqual(@"solid foo
  facet normal 1.000000e+000 2.000000e+000 3.000000e+000
    outer loop
      vertex 4.000000e+000 5.000000e+000 6.000000e+000
      vertex 7.000000e+000 8.000000e+000 9.000000e+000
      vertex 1.000000e+001 1.100000e+001 1.200000e+001
    endloop
  endfacet
endsolid foo
", content);
        }

        [TestMethod]
        public void BinaryWriterTest()
        {
            var file = new StlFile();
            file.SolidName = "foo";
            file.Triangles.Add(new StlTriangle(new StlNormal(1, 2, 3), new StlVertex(4, 5, 6), new StlVertex(7, 8, 9), new StlVertex(10, 11, 12)));
            var stream = new MemoryStream();
            file.Save(stream, asAscii: false);
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[256];
            var read = stream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(134, read);
            var expected = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // 80 byte header
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00,                                     // 1 facet
                0x00, 0x00, 0x80, 0x3F,                                     // normal.X = 1
                0x00, 0x00, 0x00, 0x40,                                     // normal.Y = 1
                0x00, 0x00, 0x40, 0x40,                                     // normal.Z = 1
                0x00, 0x00, 0x80, 0x40,                                     // vertex1.X = 1
                0x00, 0x00, 0xA0, 0x40,                                     // vertex1.Y = 1
                0x00, 0x00, 0xC0, 0x40,                                     // vertex1.Z = 1
                0x00, 0x00, 0xE0, 0x40,                                     // vertex2.X = 1
                0x00, 0x00, 0x00, 0x41,                                     // vertex2.Y = 1
                0x00, 0x00, 0x10, 0x41,                                     // vertex2.Z = 1
                0x00, 0x00, 0x20, 0x41,                                     // vertex3.X = 1
                0x00, 0x00, 0x30, 0x41,                                     // vertex3.Y = 1
                0x00, 0x00, 0x40, 0x41,                                     // vertex3.Z = 1
                0x00, 0x00                                                  // attribute byte count
            };
            for (int i = 0; i < read; i++)
            {
                Assert.AreEqual(expected[i], buffer[i]);
            }
        }
    }
}
