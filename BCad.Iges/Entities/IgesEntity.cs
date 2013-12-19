using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Iges.Directory;

namespace BCad.Iges.Entities
{
    public abstract partial class IgesEntity
    {
        public abstract IgesEntityType EntityType { get; }

        public virtual int Structure { get; set; }
        public virtual int LineFontPattern { get; set; }
        public virtual int Level { get; set; }
        public virtual int View { get; set; }
        public virtual int TransformationMatrixPointer { get; set; }
        public virtual int LableDisplay { get; set; }
        public virtual int StatusNumber { get; set; }
        public virtual int LineWeight { get; set; }
        public virtual IgesColorNumber Color { get; set; }
        public virtual int LineCount { get; set; }
        public virtual int FormNumber { get; set; }
        public virtual string EntityLabel { get; set; }
        public virtual int EntitySubscript { get; set; }
        public IgesTransformationMatrix TransformationMatrix { get; set; } // TODO: populate this

        protected abstract void ReadParameters(IList<string> parameters);

        protected abstract void WriteParameters(IList<object> parameters);

        private void PopulateDirectoryData(IgesDirectoryData directoryData)
        {
            this.Structure = directoryData.Structure;
            this.LineFontPattern = directoryData.LineFontPattern;
            this.Level = directoryData.Level;
            this.View = directoryData.View;
            this.TransformationMatrixPointer = directoryData.TransformationMatrixPointer;
            this.LableDisplay = directoryData.LableDisplay;
            this.StatusNumber = directoryData.StatusNumber;
            this.LineWeight = directoryData.LineWeight;
            this.Color = directoryData.Color;
            this.LineCount = directoryData.LineCount;
            this.FormNumber = directoryData.FormNumber;
            this.EntityLabel = directoryData.EntityLabel;
            this.EntitySubscript = directoryData.EntitySubscript;
        }

        private IgesDirectoryData GetDirectoryData()
        {
            var dir = new IgesDirectoryData();
            dir.EntityType = EntityType;
            dir.Structure = this.Structure;
            dir.LineFontPattern = this.LineFontPattern;
            dir.Level = this.Level;
            dir.View = this.View;
            dir.TransformationMatrixPointer = this.TransformationMatrixPointer;
            dir.LableDisplay = this.LableDisplay;
            dir.StatusNumber = this.StatusNumber;
            dir.LineWeight = this.LineWeight;
            dir.Color = this.Color;
            dir.LineCount = this.LineCount;
            dir.FormNumber = this.FormNumber;
            dir.EntityLabel = this.EntityLabel;
            dir.EntitySubscript = this.EntitySubscript;
            return dir;
        }

        internal void AddDirectoryAndParameterLines(List<string> directoryLines, List<string> parameterLines, char fieldDelimiter, char recordDelimiter)
        {
            var nextDirectoryIndex = directoryLines.Count + 1;
            var nextParameterIndex = parameterLines.Count + 1;
            var dir = GetDirectoryData();
            dir.ParameterPointer = nextParameterIndex;
            dir.ToString(directoryLines);
            var parameters = new List<object>();
            this.WriteParameters(parameters);

            var paramArray = new object[parameters.Count + 1];
            paramArray[0] = (int)EntityType;
            Array.Copy(parameters.ToArray(), 0, paramArray, 1, parameters.Count);
            IgesFileWriter.AddParametersToStringList(paramArray, parameterLines, fieldDelimiter, recordDelimiter,
                lineSuffix: string.Format("{0,7}", nextDirectoryIndex));
        }

        private static string ToString(object parameter)
        {
            var type = parameter.GetType();
            if (type == typeof(string))
            {
                var value = (string)parameter;
                return string.Concat(value.Length, IgesFile.StringSentinelCharacter, value);
            }
            return parameter.ToString();
        }
    }

    public partial class IgesTransformationMatrix
    {
        public IgesPoint Transform(IgesPoint point)
        {
            return new IgesPoint(
                (R11 * point.X + R12 * point.Y + R13 * point.Z) + T1,
                (R21 * point.X + R22 * point.Y + R23 * point.Z) + T2,
                (R31 * point.X + R32 * point.Y + R33 * point.Z) + T3);
        }

        public static IgesTransformationMatrix Identity
        {
            get
            {
                return new IgesTransformationMatrix()
                {
                    R11 = 1.0,
                    R12 = 0.0,
                    R13 = 0.0,
                    T1 = 0.0,
                    R21 = 0.0,
                    R22 = 1.0,
                    R23 = 0.0,
                    T2 = 0.0,
                    R31 = 0.0,
                    R32 = 0.0,
                    R33 = 1.0,
                    T3 = 0.0,
                };
            }
        }
    }
}
