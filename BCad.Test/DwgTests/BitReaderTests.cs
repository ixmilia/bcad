using System.IO;
using IxMilia.Dwg;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.DwgTests
{
    [TestClass]
    public class BitReaderTests
    {
        [TestMethod]
        public void ReadBitTest()
        {
            var reader = new BitReader(new byte[] { 0x5F, 0xF5 });
            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());

            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());

            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());

            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
        }

        [TestMethod]
        public void ReadWholeBytesTest()
        {
            var reader = new BitReader(new byte[] { 0x5F, 0xF5 });
            Assert.AreEqual((byte)0x5F, reader.ReadByte());
            Assert.AreEqual((byte)0xF5, reader.ReadByte());
        }

        [TestMethod]
        public void ReadBytesWithOffsetTest()
        {
            var reader = new BitReader(new byte[] { 0x5F, 0xF5 });
            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual((byte)0xBF, reader.ReadByte());
        }

        [TestMethod]
        public void ReadBytesWithHalfOffsetTest()
        {
            var reader = new BitReader(new byte[] { 0x5F, 0xF5 });
            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());

            Assert.AreEqual((byte)0xFF, reader.ReadByte());

            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
            Assert.AreEqual(0, reader.ReadBit());
            Assert.AreEqual(1, reader.ReadBit());
        }
    }
}
