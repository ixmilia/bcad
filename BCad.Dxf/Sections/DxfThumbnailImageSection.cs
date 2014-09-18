using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BCad.Dxf.Sections
{
    internal class DxfThumbnailImageSection : DxfSection
    {
        public override DxfSectionType Type
        {
            get { return DxfSectionType.Thumbnail; }
        }

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs(DxfAcadVersion version)
        {
            var list = new List<DxfCodePair>();
            list.Add(new DxfCodePair(90, Data.Length));

            // write lines in 128-byte chunks (expands to 256 hex bytes)
            var sb = new StringBuilder();
            var chunkCount = (int)Math.Ceiling((double)Data.Length / ChunkSize);
            for (int i = 0; i < chunkCount; i++)
            {
                sb.Clear();
                for (int offset = i * ChunkSize; offset < ChunkSize && offset < Data.Length; offset++)
                {
                    sb.Append(Data[offset].ToString("X2"));
                }

                list.Add(new DxfCodePair(310, sb.ToString()));
            }

            return list;
        }

        private const int ChunkSize = 128;

        public byte[] Data { get; set; }

        internal static DxfThumbnailImageSection ThumbnailImageSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            if (buffer.ItemsRemain)
            {
                var lengthPair = buffer.Peek();
                buffer.Advance();

                if (lengthPair.Code != 90)
                {
                    return null;
                }

                var length = lengthPair.IntegerValue;
                var data = new byte[length];
                var position = 0;
                while (buffer.ItemsRemain)
                {
                    var pair = buffer.Peek();
                    buffer.Advance();

                    if (DxfCodePair.IsSectionEnd(pair))
                    {
                        break;
                    }

                    Debug.Assert(pair.Code == 310);
                    var written = CopyHexToBuffer(pair.StringValue, data, position);
                    position += written;
                }

                var section = new DxfThumbnailImageSection();
                section.Data = data;
                return section;
            }

            return null;
        }

        private static int CopyHexToBuffer(string data, byte[] buffer, int offset)
        {
            for (int i = 0; i < data.Length; i += 2)
            {
                buffer[offset] = HexToByte(data[i], data[i + 1]);
                offset++;
            }

            return data.Length / 2;
        }

        private static byte HexToByte(char c1, char c2)
        {
            return (byte)((HexToByte(c1) << 4) + HexToByte(c2));
        }

        private static byte HexToByte(char c)
        {
            switch (c)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'a':
                case 'A': return 10;
                case 'b':
                case 'B': return 11;
                case 'c':
                case 'C': return 12;
                case 'd':
                case 'D': return 13;
                case 'e':
                case 'E': return 14;
                case 'f':
                case 'F': return 15;
                default:
                    return 0;
            }
        }
    }
}
