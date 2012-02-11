using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCad.Dxf.Sections;

namespace BCad.Dxf
{
    internal class DxfReader
    {
        private StreamReader textReader = null;
        private BinaryReader binReader = null;
        private bool isOpen = false;
        private Stream fileStream = null;

        public DxfReader(string filename)
        {
            fileStream = new FileStream(filename, FileMode.Open);
            Open();
        }

        public DxfReader(Stream stream)
        {
            fileStream = stream;
            Open();
        }

        private DxfSimpleSection NextSection()
        {
            DxfSimpleSection sec = null;
            var pair = ReadCodeValue();
            if (pair == null)
            {
                throw new DxfReadException("Unexpected end of file");
            }
            else if (pair.Code == 0 && pair.StringValue == DxfSection.SectionText)
            {
                pair = ReadCodeValue();
                if (pair.Code != 2)
                    throw new DxfReadException("Expected section type");
                var name = pair.StringValue;
                var values = CodeValuePairs.TakeWhile(p => !IsEndSec(p)).ToList();
                sec = new DxfSimpleSection(name, values);
            }
            else if (IsEof(pair))
            {
                sec = null;
            }
            return sec;
        }

        public IEnumerable<DxfSection> Sections
        {
            get
            {
                if (isOpen)
                {
                    for (var sec = NextSection(); sec != null; sec = NextSection())
                    {
                        var sectionType = sec.SectionName.ToDxfSection();
                        switch (sectionType)
                        {
                            case DxfSectionType.Entities:
                                yield return new DxfEntitiesSection(sec.ValuePairs);
                                break;
                            case DxfSectionType.Tables:
                                yield return new DxfTablesSection(sec.ValuePairs);
                                break;
                        }
                    }
                    Close();
                }
            }
        }

        private bool IsEof(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfFile.EofText;
        }

        private bool IsEndSec(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfSection.EndSectionText;
        }

        private void Open()
        {
            // check for sentinel value
            textReader = new StreamReader(fileStream);
            string sentinel = textReader.ReadLine();
            if (sentinel.StartsWith(DxfFile.BinarySentinel))
            {
                // use binary reader
                textReader = null;
                fileStream.Position = 0;
                binReader = new BinaryReader(fileStream);
                binReader.BaseStream.Seek(23, SeekOrigin.Begin);
            }
            else
            {
                // reset and use string reader
                fileStream.Position = 0;
                textReader = new StreamReader(fileStream);
            }
            isOpen = true;
        }

        private void Close()
        {
            if (textReader != null)
            {
                textReader.Close();
                textReader.Dispose();
                textReader = null;
            }
            if (binReader != null)
            {
                binReader.Close();
                binReader.Dispose();
                binReader = null;
            }
        }

        private bool EndOfStream
        {
            get
            {
                if (textReader != null)
                    return textReader.EndOfStream;
                else if (binReader != null)
                    return binReader.BaseStream.Position >= binReader.BaseStream.Length;
                else
                    throw new DxfReadException("No reader available");
            }
        }

        private void ThrowIfEof()
        {
            if (EndOfStream)
                throw new DxfReadException("Unexpected end of file");
        }

        private int ReadCode()
        {
            ThrowIfEof();
            int code = 0;
            if (textReader != null)
            {
                string line = textReader.ReadLine().Trim();
                code = int.Parse(line);
            }
            else if (binReader != null)
            {
                code = binReader.ReadByte();
                if (code == 255)
                    code = binReader.ReadInt16();
            }
            else
            {
                throw new DxfReadException("No reader available");
            }
            return code;
        }

        private object ReadValue(Type expectedType)
        {
            ThrowIfEof();
            object value = null;
            if (textReader != null)
            {
                string line = textReader.ReadLine();
                if (expectedType == typeof(short))
                    value = short.Parse(line.Trim());
                else if (expectedType == typeof(double))
                    value = double.Parse(line.Trim());
                else if (expectedType == typeof(string))
                    value = line.Trim();
                else if (expectedType == typeof(int))
                    value = int.Parse(line.Trim());
                else if (expectedType == typeof(long))
                    value = long.Parse(line.Trim());
                else
                    throw new DxfReadException("Reading type not supported " + expectedType);
            }
            else if (binReader != null)
            {
                if (expectedType == typeof(short))
                    value = binReader.ReadInt16();
                else if (expectedType == typeof(double))
                    value = binReader.ReadDouble();
                else if (expectedType == typeof(string))
                {
                    StringBuilder sb = new StringBuilder();
                    for (int b = binReader.Read(); b != 0; b = binReader.Read())
                        sb.Append((char)b);
                    value = sb.ToString();
                }
                else if (expectedType == typeof(int))
                    value = binReader.ReadInt32();
                else if (expectedType == typeof(long))
                    value = binReader.ReadInt64();
                else
                    throw new DxfReadException("Reading type not supported " + expectedType);
            }
            else
            {
                throw new DxfReadException("No reader available");
            }
            return value;
        }

        private IEnumerable<DxfCodePair> CodeValuePairs
        {
            get
            {
                for (var pair = ReadCodeValue(); pair != null; pair = ReadCodeValue())
                {
                    yield return pair;
                }
            }
        }

        private DxfCodePair ReadCodeValue()
        {
            var pair = NextFilePair();
            while (pair != null && pair.Code == 999)
            {
                pair = NextFilePair();
            }
            return pair;
        }

        private DxfCodePair NextFilePair()
        {
            int code = ReadCode();
            object value = ReadValue(DxfCodePair.ExpectedTypeForCode(code));
            return new DxfCodePair(code, value);
        }
    }
}
