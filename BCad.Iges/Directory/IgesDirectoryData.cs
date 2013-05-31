using System;
using System.Collections.Generic;
using BCad.Iges.Entities;

namespace BCad.Iges.Directory
{
    internal class IgesDirectoryData
    {
        public IgesEntityType EntityType { get; set; }
        public int ParameterPointer { get; set; }
        public int Structure { get; set; }
        public int LineFontPattern { get; set; }
        public int Level { get; set; }
        public int View { get; set; }
        public int TransformationMatrixPointer { get; set; }
        public int LableDisplay { get; set; }
        public int StatusNumber { get; set; }

        public int LineWeight { get; set; }
        public IgesColorNumber Color { get; set; }
        public int LineCount { get; set; }
        public int FormNumber { get; set; }
        public string EntityLabel { get; set; }
        public int EntitySubscript { get; set; }

        public void ToString(List<string> directoryLines)
        {
            var line1 = string.Format(
                "{0,8}{1,8}{2,8}{3,8}{4,8}{5,8}{6,8}{7,8}{8,8}",
                (int)EntityType,
                ParameterPointer,
                Structure,
                LineFontPattern,
                Level,
                ToStringOrDefault(View),
                ToStringOrDefault(TransformationMatrixPointer),
                ToStringOrDefault(LableDisplay),
                StatusNumber);
            var line2 = string.Format(
                "{0,8}{1,8}{2,8}{3,8}{4,8}{5,8}{6,8}",
                (int)EntityType,
                LineWeight,
                (int)Color,
                LineCount,
                FormNumber,
                ToStringOrDefault(EntityLabel),
                ToStringOrDefault(EntitySubscript));
            directoryLines.Add(line1);
            directoryLines.Add(line2);
        }

        private static string ToStringOrDefault(int value)
        {
            return value == 0
                ? "        "
                : string.Format("{0,8}", value);
        }

        private static string ToStringOrDefault(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "        "
                : value.Substring(0, Math.Min(8, value.Length));
        }

        public static IgesDirectoryData FromEntity(IgesEntity entity, int parameterPointer)
        {
            var dir = new IgesDirectoryData();
            dir.EntityType = entity.Type;
            dir.ParameterPointer = parameterPointer;
            dir.Structure = 0; // TODO: set real values
            dir.LineFontPattern = 0;
            dir.Level = 0;
            dir.View = 0;
            dir.TransformationMatrixPointer = 0; // TODO: proper pointer
            dir.LableDisplay = 0;
            dir.StatusNumber = 0;
            dir.LineWeight = 0;
            dir.Color = entity.Color;
            dir.LineCount = entity.LineCount;
            dir.FormNumber = entity.Form;
            dir.EntityLabel = string.Empty;
            dir.EntitySubscript = 0;
            return dir;
        }
    }
}
