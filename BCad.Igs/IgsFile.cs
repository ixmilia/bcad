using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BCad.Igs
{
    public class IgsFile
    {
        private const int MaxDataLength = 72;

        public char FieldDelimiter { get; set; }
        public char RecordDelimiter { get; set; }
        public string Identification { get; set; }
        public string FullFileName { get; set; }
        public string SystemIdentifier { get; set; }
        public string SystemVersion { get; set; }
        public int IntegerSize { get; set; }
        public int SingleSize { get; set; }
        public int DecimalDigits { get; set; }
        public int DoubleMagnitude { get; set; }
        public int DoublePrecision { get; set; }

        public IgsFile()
        {
            FieldDelimiter = ',';
            RecordDelimiter = ';';
            IntegerSize = 32;
            SingleSize = 8;
            DecimalDigits = 23;
            DoubleMagnitude = 11;
            DoublePrecision = 52;
        }

        public void Save(Stream stream)
        {
            var writer = new StreamWriter(stream);

            //// write start section
            //foreach (var section in new IgsSection[] { startSection, globalSection })
            //{
            //    int line = 1;
            //    foreach (var data in section.GetData())
            //    {
            //        writer.WriteLine(string.Format("{0,72}{1,1}{2,7}", data, SectionTypeChar(section.SectionType), line));
            //        line++;
            //    }
            //}

            writer.Flush();
        }

        public static IgsFile Load(Stream stream)
        {
            var file = new IgsFile();
            var allLines = new StreamReader(stream).ReadToEnd().Split("\n".ToCharArray()).Select(s => s.TrimEnd());
            var sectionLines = new Dictionary<IgsSectionType, List<string>>()
                {
                    { IgsSectionType.Start, new List<string>() },
                    { IgsSectionType.Global, new List<string>() },
                    { IgsSectionType.Directory, new List<string>() },
                    { IgsSectionType.Parameter, new List<string>() }
                };
            string terminateLine = null;

            foreach (var line in allLines)
            {
                if (line.Length != 80)
                    throw new IgsException("Expected line length of 80 characters.");
                var data = line.Substring(0, MaxDataLength);
                var sectionType = SectionTypeFromCharacter(line[MaxDataLength]);
                var lineNumber = int.Parse(line.Substring(MaxDataLength + 1).TrimStart());

                if (sectionType == IgsSectionType.Terminate)
                {
                    if (terminateLine != null)
                        throw new IgsException("Unexpected duplicate terminate line");
                    terminateLine = data;
                }
                else
                {
                    sectionLines[sectionType].Add(data);
                    //if (sectionLines[sectionType].Count != lineNumber)
                    //    throw new IgsException("Unordered line number");
                }
            }

            ParseGlobalLines(file, sectionLines[IgsSectionType.Global]);

            return file;
        }

        private static Regex delimiterReg = new Regex("1H.", RegexOptions.Compiled);

        private static void ParseGlobalLines(IgsFile file, List<string> lines)
        {
            var fullString = string.Join(string.Empty, lines).TrimEnd();
            Debug.Assert(fullString.Length >= 6);

            if (!delimiterReg.IsMatch(fullString.Substring(0, 3)))
                throw new IgsException("Expected field delimiter");
            file.FieldDelimiter = fullString[2];

            if (!delimiterReg.IsMatch(fullString.Substring(4, 3)))
                throw new IgsException("Expected record delimiter");
            file.RecordDelimiter = fullString[6];

            int index = 8;
            for (int field = 3; field <= 26; field++)
            {
                switch (field)
                {
                    case 3:
                        file.Identification = file.ParseString(fullString, ref index);
                        break;
                    case 4:
                        file.FullFileName = file.ParseString(fullString, ref index);
                        break;
                    case 5:
                        file.SystemIdentifier = file.ParseString(fullString, ref index);
                        break;
                    case 6:
                        file.SystemVersion = file.ParseString(fullString, ref index);
                        break;
                    case 7:
                        file.IntegerSize = file.ParseInt(fullString, ref index);
                        break;
                    case 8:
                        file.SingleSize = file.ParseInt(fullString, ref index);
                        break;
                    case 9:
                        file.DecimalDigits = file.ParseInt(fullString, ref index);
                        break;
                    case 10:
                        file.DoubleMagnitude = file.ParseInt(fullString, ref index);
                        break;
                    case 11:
                        file.DoublePrecision = file.ParseInt(fullString, ref index);
                        break;
                }
            }
        }

        private static void ParseDirectoryLines(IgsFile file, List<string> lines)
        {
            if (lines.Count % 2 != 0)
                throw new IgsException("Expected an even number of lines");
        }

        private string ParseString(string str, ref int index)
        {
            var sb = new StringBuilder();

            // parse length
            for (; index < str.Length; index++)
            {
                var c = str[index];
                if (c == 'H')
                {
                    index++; // swallow H
                    break;
                }
                if (!char.IsDigit(c))
                    throw new IgsException("Expected digit");
                sb.Append(c);
            }

            int length = int.Parse(sb.ToString());
            sb.Clear();

            // parse content
            var value = str.Substring(index, length);
            index += length;

            // verify delimiter and swallow
            if (index == str.Length - 1)
                SwallowDelimiter(str, RecordDelimiter, ref index);
            else
                SwallowDelimiter(str, FieldDelimiter, ref index);

            return value;
        }

        private int ParseInt(string str, ref int index)
        {
            var sb = new StringBuilder();
            for (; index < str.Length; index++)
            {
                var c = str[index];
                if (c == FieldDelimiter || c == RecordDelimiter)
                {
                    index++; // swallow it
                    break;
                }
                if (!char.IsDigit(c))
                    throw new IgsException("Expected digit");
                sb.Append(c);
            }

            return int.Parse(sb.ToString());
        }

        private void SwallowDelimiter(string str, char delim, ref int index)
        {
            if (index >= str.Length)
                throw new IgsException("Unexpected end of string");
            if (str[index++] != delim)
                throw new IgsException("Expected delimiter");
        }

        private static char SectionTypeChar(IgsSectionType type)
        {
            switch (type)
            {
                case IgsSectionType.Start: return 'S';
                case IgsSectionType.Global: return 'G';
                case IgsSectionType.Directory: return 'D';
                case IgsSectionType.Parameter: return 'P';
                case IgsSectionType.Terminate: return 'T';
                default:
                    throw new IgsException("Unexpected section type " + type);
            }
        }

        private static IgsSectionType SectionTypeFromCharacter(char c)
        {
            switch (c)
            {
                case 'S': return IgsSectionType.Start;
                case 'G': return IgsSectionType.Global;
                case 'D': return IgsSectionType.Directory;
                case 'P': return IgsSectionType.Parameter;
                case 'T': return IgsSectionType.Terminate;
                default:
                    throw new IgsException("Invalid section type " + c);
            }
        }
    }
}
