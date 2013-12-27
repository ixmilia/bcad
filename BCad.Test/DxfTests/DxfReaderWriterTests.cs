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
            // this file contains 12 lines
            var stream = new FileStream("diamond-bin.dxf", FileMode.Open);
            var file = DxfFile.Load(stream);
            Assert.Equal(12, file.Entities.Count);
            Assert.Equal(12, file.Entities.Where(e => e.EntityType == Dxf.Entities.DxfEntityType.Line).Count());
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
