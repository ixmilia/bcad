using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCad.Iges.Directory;
using BCad.Iges.Entities;

namespace BCad.Iges
{
    internal class IgesFileReader
    {
        public static IgesFile Load(Stream stream)
        {
            var file = new IgesFile();
            var allLines = new StreamReader(stream).ReadToEnd().Split("\n".ToCharArray()).Select(s => s.TrimEnd());
            string terminateLine = null;
            var startLines = new List<string>();
            var globalLines = new List<string>();
            var directoryLines = new List<string>();
            var parameterLines = new List<string>();
            var sectionLines = new Dictionary<IgesSectionType, List<string>>()
                {
                    { IgesSectionType.Start, startLines },
                    { IgesSectionType.Global, globalLines },
                    { IgesSectionType.Directory, directoryLines },
                    { IgesSectionType.Parameter, parameterLines }
                };

            foreach (var line in allLines)
            {
                if (line.Length != 80)
                    throw new IgesException("Expected line length of 80 characters.");
                var data = line.Substring(0, IgesFile.MaxDataLength);
                var sectionType = SectionTypeFromCharacter(line[IgesFile.MaxDataLength]);
                var lineNumber = int.Parse(line.Substring(IgesFile.MaxDataLength + 1).TrimStart());

                if (sectionType == IgesSectionType.Terminate)
                {
                    if (terminateLine != null)
                        throw new IgesException("Unexpected duplicate terminate line");
                    terminateLine = data;

                    // verify terminate data and quit
                    var startCount = int.Parse(terminateLine.Substring(1, 7));
                    var globalCount = int.Parse(terminateLine.Substring(9, 7));
                    var directoryCount = int.Parse(terminateLine.Substring(17, 7));
                    var parameterCount = int.Parse(terminateLine.Substring(25, 7));
                    if (startLines.Count != startCount)
                        throw new IgesException("Incorrect number of start lines reported");
                    if (globalLines.Count != globalCount)
                        throw new IgesException("Incorrect number of global lines reported");
                    if (directoryLines.Count != directoryCount)
                        throw new IgesException("Incorrect number of directory lines reported");
                    if (parameterLines.Count != parameterCount)
                        throw new IgesException("Incorrect number of parameter lines reported");
                    break;
                }
                else
                {
                    if (sectionType == IgesSectionType.Parameter)
                        data = data.Substring(0, data.Length - 8); // parameter data doesn't need its last 8 bytes
                    sectionLines[sectionType].Add(data);
                    if (sectionLines[sectionType].Count != lineNumber)
                        throw new IgesException("Unordered line number");
                }
            }

            // don't worry if terminate line isn't present

            ParseGlobalLines(file, globalLines);
            var parameterMap = PrepareParameterLines(parameterLines, file.FieldDelimiter, file.RecordDelimiter);
            PopulateEntities(file, directoryLines, parameterMap);

            return file;
        }

        private static Dictionary<int, List<string>> PrepareParameterLines(List<string> parameterLines, char fieldDelimiter, char recordDelimiter)
        {
            var map = new Dictionary<int, List<string>>();
            var sb = new StringBuilder();
            int parameterStart = 1;
            for (int i = 0; i < parameterLines.Count; i++)
            {
                // strip off entity number
                var startIndex = parameterLines[i].IndexOf(',') + 1;
                var line = parameterLines[i].Substring(startIndex).TrimEnd(); // TODO: could trim off whitespace in a string
                Debug.Assert(line.Length > 0);
                sb.Append(line);
                if (line[line.Length - 1] == recordDelimiter)
                {
                    var fullLine = sb.ToString();
                    var fields = SplitFields(fullLine.Substring(0, fullLine.Length - 1), fieldDelimiter);
                    map[parameterStart] = fields;
                    parameterStart = i + 2;
                    sb.Clear();
                }
            }

            return map;
        }

        private static List<string> SplitFields(string text, char fieldDelimiter)
        {
            var fields = new List<string>();
            int startIndex = 0;
            for (int i = 0; i < text.Length; i++)
            {
                // TODO: allow for string fields that might contain the delimiter
                if (text[i] == fieldDelimiter)
                {
                    var field = text.Substring(startIndex, i - startIndex);
                    fields.Add(field);
                    startIndex = i + 1;
                }
            }

            fields.Add(text.Substring(startIndex)); // don't forget the last field
            return fields;
        }

        private static void PopulateEntities(IgesFile file, List<string> directoryLines, Dictionary<int, List<string>> parameterMap)
        {
            var transformationMatrices = new Dictionary<int, IgesTransformationMatrix>();
            for (int i = 0; i < directoryLines.Count; i += 2)
            {
                var dir = IgesDirectoryData.FromRawLines(directoryLines[i], directoryLines[i + 1]);
                var entity = IgesEntity.FromData(dir, parameterMap[dir.ParameterPointer]);
                if (entity != null)
                {
                    if (entity.EntityType == IgesEntityType.TransformationMatrix)
                    {
                        transformationMatrices[i + 1] = (IgesTransformationMatrix)entity;
                    }

                    if (dir.TransformationMatrixPointer > 0)
                        entity.TransformationMatrix = transformationMatrices[dir.TransformationMatrixPointer];
                    else
                        entity.TransformationMatrix = IgesTransformationMatrix.Identity;
                    file.Entities.Add(entity);
                }
            }
        }

        private static void ParseGlobalLines(IgesFile file, List<string> globalLines)
        {
            var fullString = string.Join(string.Empty, globalLines).TrimEnd();
            if (string.IsNullOrEmpty(fullString))
                return;

            int index = 0;
            for (int field = 1; field <= 26; field++)
            {
                switch (field)
                {
                    case 1:
                        ParseDelimiterCharacter(file, fullString, ref index, true);
                        break;
                    case 2:
                        ParseDelimiterCharacter(file, fullString, ref index, false);
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
                        file.ModelUnits = (IgesUnits)ParseInt(file, fullString, ref index, (int)file.ModelUnits);
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
                        file.IgesVersion = (IgesVersion)ParseInt(file, fullString, ref index);
                        break;
                    case 24:
                        file.DraftingStandard = (IgesDraftingStandard)ParseInt(file, fullString, ref index);
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

        private static void ParseDelimiterCharacter(IgesFile file, string str, ref int index, bool readFieldSeparator)
        {
            // verify length
            if (index >= str.Length)
                throw new IgesException("Unexpected end of input");

            // could be empty
            if (str[index] == IgesFile.DefaultFieldDelimiter)
            {
                index++;
                return;
            }

            if (str[index] != '1')
                throw new IgesException("Expected delimiter of length 1");
            index++;

            // verify 'H' separator
            if (index >= str.Length)
                throw new IgesException("Unexpected end of input");
            if (str[index] != IgesFile.StringSentinelCharacter)
                throw new IgesException("Unexpected string sentinel character");
            index++;

            // get the separator character and set it
            if (index >= str.Length)
                throw new IgesException("Expected delimiter character");
            var separator = str[index];
            if (readFieldSeparator)
                file.FieldDelimiter = separator;
            else
                file.RecordDelimiter = separator;
            index++;

            // verify delimiter
            if (index >= str.Length)
                throw new IgesException("Unexpected end of input");
            separator = str[index];
            if (separator != file.FieldDelimiter && separator != file.RecordDelimiter)
                throw new IgesException("Expected field or record delimiter");
            index++; // swallow it
        }

        private static string ParseString(IgesFile file, string str, ref int index, string defaultValue = null)
        {
            if (index < str.Length && (str[index] == file.FieldDelimiter || str[index] == file.RecordDelimiter))
            {
                // swallow the delimiter and return the default
                index++;
                return defaultValue;
            }

            SwallowWhitespace(str, ref index);

            var sb = new StringBuilder();

            // parse length
            for (; index < str.Length; index++)
            {
                var c = str[index];
                if (c == IgesFile.StringSentinelCharacter)
                {
                    index++; // swallow H
                    break;
                }
                if (!char.IsDigit(c))
                    throw new IgesException("Expected digit");
                sb.Append(c);
            }

            var lengthString = sb.ToString();
            if (string.IsNullOrWhiteSpace(lengthString))
            {
                return defaultValue;
            }

            int length = int.Parse(lengthString);
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

        private static int ParseInt(IgesFile file, string str, ref int index, int defaultValue = 0)
        {
            if (index < str.Length && (str[index] == file.FieldDelimiter || str[index] == file.RecordDelimiter))
            {
                // swallow the delimiter and return the default
                index++;
                return defaultValue;
            }

            SwallowWhitespace(str, ref index);

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
                    throw new IgesException("Expected digit");
                sb.Append(c);
            }

            var intString = sb.ToString();
            if (string.IsNullOrWhiteSpace(intString))
                return defaultValue;
            else
                return int.Parse(sb.ToString());
        }

        private static double ParseDouble(IgesFile file, string str, ref int index, double defaultValue = 0.0)
        {
            if (index < str.Length && (str[index] == file.FieldDelimiter || str[index] == file.RecordDelimiter))
            {
                // swallow the delimiter and return the default
                index++;
                return defaultValue;
            }

            SwallowWhitespace(str, ref index);

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

            var doubleString = sb.ToString();
            if (string.IsNullOrWhiteSpace(doubleString))
                return defaultValue;
            else
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
                throw new IgesException("Invalid date/time format");
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

        private static void SwallowWhitespace(string str, ref int index)
        {
            for (; index < str.Length; index++)
            {
                var c = str[index];
                if (!char.IsWhiteSpace(c))
                    break;
            }
        }

        private static Regex dateTimeReg = new Regex(@"((\d{2})|(\d{4}))(\d{2})(\d{2})\.(\d{2})(\d{2})(\d{2})");
        //                                             12       3       4      5        6      7      8

        private static void SwallowDelimiter(string str, char delim, ref int index)
        {
            if (index >= str.Length)
                throw new IgesException("Unexpected end of string");
            if (str[index++] != delim)
                throw new IgesException("Expected delimiter");
        }

        private static IgesSectionType SectionTypeFromCharacter(char c)
        {
            switch (c)
            {
                case 'S': return IgesSectionType.Start;
                case 'G': return IgesSectionType.Global;
                case 'D': return IgesSectionType.Directory;
                case 'P': return IgesSectionType.Parameter;
                case 'T': return IgesSectionType.Terminate;
                default:
                    throw new IgesException("Invalid section type " + c);
            }
        }
    }
}
