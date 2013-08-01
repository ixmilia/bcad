using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCad.Dxf;
using Xunit;

namespace BCad.Test.DxfTests
{
    public class DxfReaderWriterTests
    {
        [Fact]
        public void BinaryReaderTest()
        {
            // TODO:
        }

        [Fact]
        public void SkipBomTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write((char)0xFEFF); // BOM
            writer.Write("0\r\nEOF");
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var file = DxfFile.Load(stream);
            Assert.Equal(0, file.Layers.Count);
        }
    }
}
