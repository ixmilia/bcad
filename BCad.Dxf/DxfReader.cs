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
        public Stream BaseStream { get; private set; }

        private bool started = false;
        private bool readText = false;
        private string firstLine = null;
        private StreamReader textReader = null;
        private BinaryReader binReader = null;

        public DxfReader(Stream input)
        {
            this.BaseStream = input;
            binReader = new BinaryReader(input);
        }

        private string ReadLine()
        {
            if (firstLine != null)
            {
                string result = firstLine;
                firstLine = null;
                return result;
            }

            return textReader.ReadLine();
        }

        private void Initialize()
        {
            // read first line char-by-char
            var sb = new StringBuilder();
            char c = binReader.ReadChar();
            while (c != '\n')
            {
                sb.Append(c);
                c = binReader.ReadChar();
            }

            // if sentinel, continue with binary reader
            var line = sb.ToString();
            if (line.StartsWith(DxfFile.BinarySentinel))
            {
                readText = false;
                firstLine = null;
            }
            else
            {
                // otherwise, first line is data
                readText = true;
                firstLine = line;
                textReader = new StreamReader(this.BaseStream);
            }

            started = true;
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
                for (var sec = NextSection(); sec != null; sec = NextSection())
                {
                    var sectionType = sec.SectionName.ToDxfSection();
                    switch (sectionType)
                    {
                        case DxfSectionType.Header:
                            yield return new DxfHeaderSection(sec.ValuePairs);
                            break;
                        case DxfSectionType.Entities:
                            yield return new DxfEntitiesSection(sec.ValuePairs);
                            break;
                        case DxfSectionType.Tables:
                            yield return new DxfTablesSection(sec.ValuePairs);
                            break;
                    }
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

        private int ReadCode()
        {
            int code = 0;
            if (!started)
                Initialize();

            if (readText)
            {
                code = int.Parse(ReadLine().Trim());
            }
            else
            {
                code = binReader.ReadByte();
                if (code == 255)
                    code = binReader.ReadInt16();
            }

            return code;
        }

        private object ReadValue(Type expectedType)
        {
            object value = null;
            if (readText)
            {
                string line = ReadLine().Trim(); ;
                if (expectedType == typeof(short))
                    value = short.Parse(line);
                else if (expectedType == typeof(double))
                    value = double.Parse(line);
                else if (expectedType == typeof(string))
                    value = line;
                else if (expectedType == typeof(int))
                    value = int.Parse(line);
                else if (expectedType == typeof(long))
                    value = long.Parse(line);
                else
                    throw new DxfReadException("Reading type not supported " + expectedType);
            }
            else
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
