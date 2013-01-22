﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCad.Dxf.Sections;

namespace BCad.Dxf
{
    internal class DxfWriter
    {
        private StreamWriter textWriter = null;
        private BinaryWriter binWriter = null;
        private ASCIIEncoding ascii = new ASCIIEncoding();
        private Stream fileStream = null;

        private bool asText = true;

        public DxfWriter(string filename, bool asText)
        {
            fileStream = new FileStream(filename, FileMode.OpenOrCreate);
            this.asText = asText;
        }

        public DxfWriter(Stream stream, bool asText)
        {
            fileStream = stream;
            this.asText = asText;
        }

        public void Open()
        {
            if (asText)
            {
                textWriter = new StreamWriter(fileStream);
            }
            else
            {
                binWriter = new BinaryWriter(fileStream);
                binWriter.Write(ascii.GetBytes(DxfFile.BinarySentinel));
                binWriter.Write("\r\n");
                binWriter.Write((byte)26);
                binWriter.Write((byte)0);
            }
        }

        public void Close()
        {
            WriteCodeValuePair(new DxfCodePair(0, DxfFile.EofText));
            if (textWriter != null)
            {
                textWriter.Close();
                textWriter.Dispose();
                textWriter = null;
            }
            if (binWriter != null)
            {
                binWriter.Close();
                binWriter.Dispose();
                binWriter = null;
            }
        }

        public void WriteCodeValuePair(DxfCodePair pair)
        {
            WriteCode(pair.Code);
            WriteValue(pair.Code, pair.Value);
        }

        public void WriteCodeValuePairs(IEnumerable<DxfCodePair> pairs)
        {
            foreach (var pair in pairs)
                WriteCodeValuePair(pair);
        }

        private void WriteCode(int code)
        {
            if (textWriter != null)
            {
                textWriter.WriteLine(code.ToString().PadLeft(3));
            }
            else if (binWriter != null)
            {
                if (code >= 255)
                {
                    binWriter.Write((byte)255);
                    binWriter.Write((short)code);
                }
                else
                {
                    binWriter.Write((byte)code);
                }
            }
            else
            {
                throw new DxfReadException("No writer available");
            }
        }

        private void WriteValue(int code, object value)
        {
            var type = DxfCodePair.ExpectedTypeForCode(code);
            if (type == typeof(string))
                WriteString((string)value);
            else if (type == typeof(double))
                WriteDouble((double)value);
            else if (type == typeof(short))
                WriteShort((short)value);
            else if (type == typeof(int))
                WriteInt((int)value);
            else if (type == typeof(long))
                WriteLong((long)value);
            else
                throw new DxfReadException("No writer available");
        }

        private void WriteString(string value)
        {
            if (textWriter != null)
                textWriter.WriteLine(value);
            else if (binWriter != null)
            {
                binWriter.Write(ascii.GetBytes((string)value));
                binWriter.Write((byte)0);
            }
        }

        private void WriteDouble(double value)
        {
            if (textWriter != null)
                textWriter.WriteLine(value.ToString("E16"));
            else if (binWriter != null)
                binWriter.Write(value);
        }

        private void WriteShort(short value)
        {
            if (textWriter != null)
                textWriter.WriteLine(value);
            else if (binWriter != null)
                binWriter.Write(value);
        }

        private void WriteInt(int value)
        {
            if (textWriter != null)
                textWriter.WriteLine(value);
            else if (binWriter != null)
                binWriter.Write(value);
        }

        private void WriteLong(long value)
        {
            if (textWriter != null)
                textWriter.WriteLine(value);
            else if (binWriter != null)
                binWriter.Write(value);
        }
    }
}
