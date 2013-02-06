using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BCad.Igs
{
    internal static class IgsFileReader
    {
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
                var data = line.Substring(0, IgsFile.MaxDataLength);
                var sectionType = SectionTypeFromCharacter(line[IgsFile.MaxDataLength]);
                var lineNumber = int.Parse(line.Substring(IgsFile.MaxDataLength + 1).TrimStart());

                if (sectionType == IgsSectionType.Terminate)
                {
                    if (terminateLine != null)
                        throw new IgsException("Unexpected duplicate terminate line");
                    terminateLine = data;
                }
                else
                {
                    sectionLines[sectionType].Add(data);
                    if (sectionLines[sectionType].Count != lineNumber)
                        throw new IgsException("Unordered line number");
                }
            }

            ParseGlobalLines(file, sectionLines[IgsSectionType.Global]);

            return file;
        }

        private static Regex delimiterReg = new Regex("1H.", RegexOptions.Compiled);

        private static void ParseGlobalLines(IgsFile file, List<string> lines)
        {
            var fullString = string.Join(string.Empty, lines).TrimEnd();

            string temp;
            int index = 0;
            for (int field = 1; field <= 26; field++)
            {
                switch (field)
                {
                    case 1:
                        temp = ParseString(file, fullString, ref index, file.FieldDelimiter.ToString());
                        if (temp == null || temp.Length != 1)
                            throw new IgsException("Expected delimiter of length 1");
                        file.FieldDelimiter = temp[0];
                        break;
                    case 2:
                        temp = ParseString(file, fullString, ref index, file.RecordDelimiter.ToString());
                        if (temp == null || temp.Length != 1)
                            throw new IgsException("Expected delimiter of length 1");
                        file.RecordDelimiter = temp[0];
                        break;
                    case 3:
                        file.Identification = ParseString(file, fullString, ref index);
                        break;
                    case 4:
                        file.FullFileName = ParseString(file, fullString, ref index);
                        break;
                    case 5:
                        file.SystemIdentifier = ParseString(file, fullString, ref index);
                        break;
                    case 6:
                        file.SystemVersion = ParseString(file, fullString, ref index);
                        break;
                    case 7:
                        file.IntegerSize = ParseInt(file, fullString, ref index);
                        break;
                    case 8:
                        file.SingleSize = ParseInt(file, fullString, ref index);
                        break;
                    case 9:
                        file.DecimalDigits = ParseInt(file, fullString, ref index);
                        break;
                    case 10:
                        file.DoubleMagnitude = ParseInt(file, fullString, ref index);
                        break;
                    case 11:
                        file.DoublePrecision = ParseInt(file, fullString, ref index);
                        break;
                    case 12:
                        file.Identifier = ParseString(file, fullString, ref index);
                        break;
                    case 13:
                        file.ModelSpaceScale = ParseDouble(file, fullString, ref index);
                        break;
                    case 14:
                        file.ModelUnits = (IgsUnits)ParseInt(file, fullString, ref index, (int)file.ModelUnits);
                        break;
                    case 15:
                        file.CustomModelUnits = ParseString(file, fullString, ref index);
                        break;
                    case 16:
                        file.MaxLineWeightGraduations = ParseInt(file, fullString, ref index);
                        break;
                    case 17:
                        file.MaxLineWeight = ParseDouble(file, fullString, ref index);
                        break;
                    case 18:
                        file.TimeStamp = ParseDateTime(ParseString(file, fullString, ref index), file.TimeStamp);
                        break;
                    case 19:
                        file.MinimumResolution = ParseDouble(file, fullString, ref index);
                        break;
                    case 20:
                        file.MaxCoordinateValue = ParseDouble(file, fullString, ref index);
                        break;
                    case 21:
                        file.Author = ParseString(file, fullString, ref index);
                        break;
                    case 22:
                        file.Organization = ParseString(file, fullString, ref index);
                        break;
                    case 23:
                        file.IegsVersion = (IegsVersion)ParseInt(file, fullString, ref index);
                        break;
                    case 24:
                        file.DraftingStandard = (IgsDraftingStandard)ParseInt(file, fullString, ref index);
                        break;
                    case 25:
                        file.ModifiedTime = ParseDateTime(ParseString(file, fullString, ref index), file.ModifiedTime);
                        break;
                    case 26:
                        file.ApplicationProtocol = ParseString(file, fullString, ref index);
                        break;
                }
            }
        }

        private static void ParseDirectoryLines(IgsFile file, List<string> lines)
        {
            if (lines.Count % 2 != 0)
                throw new IgsException("Expected an even number of lines");
        }

        private static string ParseString(IgsFile file, string str, ref int index, string defaultValue = null)
        {
            if (index < str.Length && (str[index] == file.FieldDelimiter || str[index] == file.RecordDelimiter))
            {
                // swallow the delimiter and return the default
                index++;
                return defaultValue;
            }

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
                SwallowDelimiter(str, file.RecordDelimiter, ref index);
            else
                SwallowDelimiter(str, file.FieldDelimiter, ref index);

            return value;
        }

        private static int ParseInt(IgsFile file, string str, ref int index, int defaultValue = 0)
        {
            if (index < str.Length && (str[index] == file.FieldDelimiter || str[index] == file.RecordDelimiter))
            {
                // swallow the delimiter and return the default
                index++;
                return defaultValue;
            }

            var sb = new StringBuilder();
            for (; index < str.Length; index++)
            {
                var c = str[index];
                if (c == file.FieldDelimiter || c == file.RecordDelimiter)
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

        private static double ParseDouble(IgsFile file, string str, ref int index, double defaultValue = 0.0)
        {
            if (index < str.Length && (str[index] == file.FieldDelimiter || str[index] == file.RecordDelimiter))
            {
                // swallow the delimiter and return the default
                index++;
                return defaultValue;
            }

            var sb = new StringBuilder();
            for (; index < str.Length; index++)
            {
                var c = str[index];
                if (c == file.FieldDelimiter || c == file.RecordDelimiter)
                {
                    index++; // swallow it
                    break;
                }
                sb.Append(c);
            }

            return double.Parse(sb.ToString());
        }

        private static DateTime ParseDateTime(string value, DateTime defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DateTime.Now;
            }

            var match = dateTimeReg.Match(value);
            if (!match.Success)
                throw new IgsException("Invalid date/time format");
            Debug.Assert(match.Groups.Count == 9);
            int year = int.Parse(match.Groups[1].Value);
            int month = int.Parse(match.Groups[4].Value);
            int day = int.Parse(match.Groups[5].Value);
            int hour = int.Parse(match.Groups[6].Value);
            int minute = int.Parse(match.Groups[7].Value);
            int second = int.Parse(match.Groups[8].Value);
            if (match.Groups[1].Value.Length == 2)
                year += 1900;
            return new DateTime(year, month, day, hour, minute, second);
        }

        private static Regex dateTimeReg = new Regex(@"((\d{2})|(\d{4}))(\d{2})(\d{2})\.(\d{2})(\d{2})(\d{2})", RegexOptions.Compiled);
        //                                             12       3       4      5        6      7      8

        private static void SwallowDelimiter(string str, char delim, ref int index)
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
