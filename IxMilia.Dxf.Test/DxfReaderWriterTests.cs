using System.IO;
using System.Linq;
using IxMilia.Dxf;
using IxMilia.Dxf.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.DxfTests
{
    [TestClass]
    public class DxfReaderWriterTests : AbstractDxfTests
    {
        [TestMethod]
        public void BinaryReaderTest()
        {
            // this file contains 12 lines
            var stream = new FileStream("diamond-bin.dxf", FileMode.Open);
            var file = DxfFile.Load(stream);
            Assert.AreEqual(12, file.Entities.Count);
            Assert.AreEqual(12, file.Entities.Where(e => e.EntityType == IxMilia.Dxf.Entities.DxfEntityType.Line).Count());
        }

        [TestMethod]
        public void SkipBomTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write((char)0xFEFF); // BOM
            writer.Write("0\r\nEOF");
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var file = DxfFile.Load(stream);
            Assert.AreEqual(0, file.Layers.Count);
        }

        [TestMethod]
        public void ReadThumbnailTest()
        {
            var file = Section("THUMBNAILIMAGE", @" 90
3
310
012345");
            AssertArrayEqual(file.RawThumbnail, new byte[] { 0x01, 0x23, 0x45 });
        }

        [TestMethod]
        public void WriteThumbnailTestR14()
        {
            var file = new DxfFile();
            file.Header.Version = DxfAcadVersion.R14;
            file.RawThumbnail = new byte[] { 0x01, 0x23, 0x45 };
            VerifyFileDoesNotContain(file, @"  0
SECTION
  2
THUMBNAILIMAGE");
        }

        [TestMethod]
        public void WriteThumbnailTestR2000()
        {
            var file = new DxfFile();
            file.Header.Version = DxfAcadVersion.R2000;
            file.RawThumbnail = new byte[] { 0x01, 0x23, 0x45 };
            VerifyFileContains(file, @"  0
SECTION
  2
THUMBNAILIMAGE
 90
3
310
012345
  0
ENDSEC");
        }

        [TestMethod]
        public void WriteThumbnailTest_SetThumbnailBitmap()
        {
            var file = new DxfFile();
            file.Header.Version = DxfAcadVersion.R2000;
            var header = DxfThumbnailImageSection.BITMAPFILEHEADER;
            var bitmap = header.Concat(new byte[] { 0x01, 0x23, 0x45 }).ToArray();
            file.SetThumbnailBitmap(bitmap);
            VerifyFileContains(file, @"  0
SECTION
  2
THUMBNAILIMAGE
 90
3
310
012345
  0
ENDSEC");
        }

        [TestMethod]
        public void ReadThumbnailTest_GetThumbnailBitmap()
        {
            var file = Section("THUMBNAILIMAGE", @" 90
3
310
012345");
            var expected = new byte[]
            {
                (byte)'B', (byte)'M', // magic number
                0x03, 0x00, 0x00, 0x00, // file length excluding header
                0x00, 0x00, // reserved
                0x00, 0x00, // reserved
                0x36, 0x04, 0x00, 0x00, // bit offset; always 1078
                0x01, 0x23, 0x45 // body
            };
            var bitmap = file.GetThumbnailBitmap();
            AssertArrayEqual(expected, bitmap);
        }
    }
}
