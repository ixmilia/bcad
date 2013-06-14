using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Dwg;
using Xunit;

namespace BCad.Test.DwgTests
{
    public class BitReaderTests
    {
        [Fact]
        public void ReadBitTest()
        {
            var reader = new BitReader(new MemoryStream(new byte[] { 0x5F, 0xF5 }));
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());

            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());

            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());

            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
        }

        [Fact]
        public void ReadWholeBytesTest()
        {
            var reader = new BitReader(new MemoryStream(new byte[] { 0x5F, 0xF5 }));
            Assert.Equal((byte)0x5F, reader.ReadByte());
            Assert.Equal((byte)0xF5, reader.ReadByte());
        }

        [Fact]
        public void ReadBytesWithOffsetTest()
        {
            var reader = new BitReader(new MemoryStream(new byte[] { 0x5F, 0xF5 }));
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal((byte)0xBF, reader.ReadByte());
        }

        [Fact]
        public void ReadBytesWithHalfOffsetTest()
        {
            var reader = new BitReader(new MemoryStream(new byte[] { 0x5F, 0xF5 }));
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());

            Assert.Equal((byte)0xFF, reader.ReadByte());

            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
            Assert.Equal(0, reader.ReadBit());
            Assert.Equal(1, reader.ReadBit());
        }
    }
}
