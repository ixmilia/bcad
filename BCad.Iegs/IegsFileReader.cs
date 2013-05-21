using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCad.Iegs.Directory;
using BCad.Iegs.Entities;
using BCad.Iegs.Parameter;

namespace BCad.Iegs
{
    internal class IegsFileReader
    {
        public IegsFile Load(Stream stream)
        {
            var file = new IegsFile();
            var allLines = new StreamReader(stream).ReadToEnd().Split("\n".ToCharArray()).Select(s => s.TrimEnd());
            string terminateLine = null;
            var startLines = new List<string>();
            var globalLines = new List<string>();
            var directoryLines = new List<string>();
            var parameterLines = new List<string>();
            var parameterData = new Dictionary<int, IegsParameterData>();
            var sectionLines = new Dictionary<IegsSectionType, List<string>>()
                {
                    { IegsSectionType.Start, startLines },
                    { IegsSectionType.Global, globalLines },
                    { IegsSectionType.Directory, directoryLines },
                    { IegsSectionType.Parameter, parameterLines }
                };

            foreach (var line in allLines)
            {
                if (line.Length != 80)
                    throw new IegsException("Expected line length of 80 characters.");
                var data = line.Substring(0, IegsFile.MaxDataLength);
                var sectionType = SectionTypeFromCharacter(line[IegsFile.MaxDataLength]);
                var lineNumber = int.Parse(line.Substring(IegsFile.MaxDataLength + 1).TrimStart());

                if (sectionType == IegsSectionType.Terminate)
                {
                    if (terminateLine != null)
                        throw new IegsException("Unexpected duplicate terminate line");
                    terminateLine = data;

                    // verify terminate data and quit
                    var startCount = int.Parse(terminateLine.Substring(1, 7));
                    var globalCount = int.Parse(terminateLine.Substring(9, 7));
                    var directoryCount = int.Parse(terminateLine.Substring(17, 7));
                    var parameterCount = int.Parse(terminateLine.Substring(25, 7));
                    if (startLines.Count != startCount)
                        throw new IegsException("Incorrect number of start lines reported");
                    if (globalLines.Count != globalCount)
                        throw new IegsException("Incorrect number of global lines reported");
                    if (directoryLines.Count != directoryCount)
                        throw new IegsException("Incorrect number of directory lines reported");
                    if (parameterLines.Count != parameterCount)
                        throw new IegsException("Incorrect number of parameter lines reported");
                    break;
                }
                else
                {
                    if (sectionType == IegsSectionType.Parameter)
                        data = data.Substring(0, data.Length - 8); // parameter data doesn't need its last 8 bytes
                    sectionLines[sectionType].Add(data);
                    if (sectionLines[sectionType].Count != lineNumber)
                        throw new IegsException("Unordered line number");
                }
            }

            // don't worry if terminate line isn't present

            ParseGlobalLines(file, globalLines);
            ParseParameterLines(file, parameterLines, parameterData);
            ParseDirectoryLines(file, directoryLines, parameterData);

            return file;
        }

        private static void ParseGlobalLines(IegsFile file, List<string> globalLines)
        {
            var fullString = string.Join(string.Empty, globalLines).TrimEnd();
            if (string.IsNullOrEmpty(fullString))
                return;

            string temp;
            int index = 0;
            for (int field = 1; field <= 26; field++)
            {
                switch (field)
                {
                    case 1:
                        temp = ParseString(file, fullString, ref index, file.FieldDelimiter.ToString());
                        if (temp == null || temp.Length != 1)
                            throw new IegsException("Expected delimiter of length 1");
                        file.FieldDelimiter = temp[0];
                        break;
                    case 2:
                        temp = ParseString(file, fullString, ref index, file.RecordDelimiter.ToString());
                        if (temp == null || temp.Length != 1)
                            throw new IegsException("Expected delimiter of length 1");
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
                        file.ModelUnits = (IegsUnits)ParseInt(file, fullString, ref index, (int)file.ModelUnits);
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
                        file.DraftingStandard = (IegsDraftingStandard)ParseInt(file, fullString, ref index);
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

        private static void ParseParameterLines(IegsFile file, List<string> parameterLines, Dictionary<int, IegsParameterData> parameterData)
        {
            // group parameter lines together
            int index = 1;
            var sb = new StringBuilder();
            for (int i = 0; i < parameterLines.Count; i++)
            {
                var line = parameterLines[i].Substring(0, 64); // last 16 bytes aren't needed
                sb.Append(line);
                if (line.TrimEnd().EndsWith(file.RecordDelimiter.ToString())) // TODO: string may contain delimiter
                {
                    var fields = SplitFields(line, file.FieldDelimiter, file.RecordDelimiter);
                    if (fields.Count < 2)
                        throw new IegsException("At least two fields necessary");
                    var entityType = (IegsEntityType)int.Parse(fields[0]);
                    var data = IegsParameterData.ParseFields(entityType, fields.Skip(1).ToList());
                    if (data != null)
                        parameterData.Add(index, data);
                    index = i + 2; // +1 for zero offset, +1 to skip to the next line
                    sb.Clear();
                }
            }
        }

        private static void ParseDirectoryLines(IegsFile file, List<string> directoryLines, Dictionary<int, IegsParameterData> parameterData)
        {
            if (directoryLines.Count % 2 != 0)
                throw new IegsException("Expected an even number of lines");

            var transformationMatricies = new Dictionary<int, IegsTransformationMatrix>();

            for (int i = 0; i < directoryLines.Count; i += 2)
            {
                var lineNumber = i + 1;
                var line1 = directoryLines[i];
                var line2 = directoryLines[i + 1];
                var entityTypeNumber = int.Parse(GetField(line1, 1));
                if (entityTypeNumber != 0)
                {
                    var dir = new IegsDirectoryData();
                    dir.EntityType = (IegsEntityType)entityTypeNumber;
                    dir.ParameterPointer = int.Parse(GetField(line1, 2));
                    dir.Structure = int.Parse(GetField(line1, 3));
                    dir.LineFontPattern = int.Parse(GetField(line1, 4));
                    dir.Level = int.Parse(GetField(line1, 5));
                    dir.View = int.Parse(GetField(line1, 6));
                    dir.TransformationMatrixPointer = int.Parse(GetField(line1, 7));
                    dir.LableDisplay = int.Parse(GetField(line1, 8));
                    dir.StatusNumber = int.Parse(GetField(line1, 9));

                    dir.LineWeight = int.Parse(GetField(line2, 2));
                    dir.Color = (IegsColorNumber)int.Parse(GetField(line2, 3)); // TODO: could be a negative pointer
                    dir.LineCount = int.Parse(GetField(line2, 4));
                    dir.FormNumber = int.Parse(GetField(line2, 5));
                    dir.EntityLabel = GetField(line2, 8, null);
                    dir.EntitySubscript = int.Parse(GetField(line2, 9));

                    if (dir.TransformationMatrixPointer >= lineNumber)
                        throw new IegsException("Pointer must point back");

                    if (parameterData.ContainsKey(dir.ParameterPointer))
                    {
                        var data = parameterData[dir.ParameterPointer];
                        var entity = IegsEntity.CreateEntity(data, dir, transformationMatricies);
                        if (entity.Type == IegsEntityType.TransformationMatrix)
                        {
                            transformationMatricies.Add(lineNumber, (IegsTransformationMatrix)entity);
                        }
                        file.Entities.Add(entity);
                    }
                }
            }
        }

        private static string GetField(string str, int field, string defaultValue = "0")
        {
            var size = 8;
            var offset = (field - 1) * size;
            var value = str.Substring(offset, size).Trim();
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        private static List<string> SplitFields(string input, char fieldDelimiter, char recordDelimiter)
        {
            // TODO: watch for strings containing delimiters
            var fields = new List<string>();
            var sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == fieldDelimiter || c == recordDelimiter)
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                    if (c == recordDelimiter)
                    {
                        break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return fields;
        }

        private static string ParseString(IegsFile file, string str, ref int index, string defaultValue = null)
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
                    throw new IegsException("Expected digit");
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

        private static int ParseInt(IegsFile file, string str, ref int index, int defaultValue = 0)
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
                    throw new IegsException("Expected digit");
                sb.Append(c);
            }

            return int.Parse(sb.ToString());
        }

        private static double ParseDouble(IegsFile file, string str, ref int index, double defaultValue = 0.0)
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
                throw new IegsException("Invalid date/time format");
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
                throw new IegsException("Unexpected end of string");
            if (str[index++] != delim)
                throw new IegsException("Expected delimiter");
        }

        private static char SectionTypeChar(IegsSectionType type)
        {
            switch (type)
            {
                case IegsSectionType.Start: return 'S';
                case IegsSectionType.Global: return 'G';
                case IegsSectionType.Directory: return 'D';
                case IegsSectionType.Parameter: return 'P';
                case IegsSectionType.Terminate: return 'T';
                default:
                    throw new IegsException("Unexpected section type " + type);
            }
        }

        private static IegsSectionType SectionTypeFromCharacter(char c)
        {
            switch (c)
            {
                case 'S': return IegsSectionType.Start;
                case 'G': return IegsSectionType.Global;
                case 'D': return IegsSectionType.Directory;
                case 'P': return IegsSectionType.Parameter;
                case 'T': return IegsSectionType.Terminate;
                default:
                    throw new IegsException("Invalid section type " + c);
            }
        }
    }
}
