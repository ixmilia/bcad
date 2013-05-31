using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BCad.Iges.Directory;
using BCad.Iges.Entities;

namespace BCad.Iges.Parameter
{
    internal abstract class IgesParameterData
    {
        public int DirectoryPointer { get; set; }

        public abstract IgesEntity ToEntity(IgesDirectoryData dir);

        protected abstract object[] GetFields();

        internal void ToString(List<string> parameterLines, IgesFile file, IgesEntityType type)
        {
            var fields = GetFields();
            var parameters = new object[fields.Length + 1];
            parameters[0] = (int)type;
            Array.Copy(fields, 0, parameters, 1, fields.Length);

            IgesFileWriter.AddParametersToStringList(parameters, parameterLines, file.FieldDelimiter, file.RecordDelimiter,
                maxLength: IgesFile.MaxParameterLength,
                lineSuffix: string.Format(" {0,7}", DirectoryPointer));
        }

        public static IgesParameterData ParseFields(IgesEntityType type, List<string> fields)
        {
            switch (type)
            {
                case IgesEntityType.Circle:
                    return ParseCircle(fields);
                case IgesEntityType.Line:
                    return ParseLine(fields);
                case IgesEntityType.TransformationMatrix:
                    return ParseTransformationMatrix(fields);
                default:
                    return null;
            }
        }

        internal static IgesParameterData FromEntity(IgesEntity entity, int directoryPointer)
        {
            IgesParameterData data;
            switch (entity.Type)
            {
                case IgesEntityType.Circle:
                    var circle = (IgesCircle)entity;
                    data = new IgesCircleParameterData()
                    {
                        ZT = circle.PlaneDisplacement,
                        X1 = circle.Center.X,
                        Y1 = circle.Center.Y,
                        X2 = circle.StartPoint.X,
                        Y2 = circle.StartPoint.Y,
                        X3 = circle.EndPoint.X,
                        Y3 = circle.EndPoint.Y
                    };
                    break;
                case IgesEntityType.Line:
                    var line = (IgesLine)entity;
                    data = new IgesLineParameterData()
                    {
                        X1 = line.P1.X,
                        Y1 = line.P1.Y,
                        Z1 = line.P1.Z,
                        X2 = line.P2.X,
                        Y2 = line.P2.Y,
                        Z2 = line.P2.Z
                    };
                    break;
                case IgesEntityType.TransformationMatrix:
                    var matrix = (IgesTransformationMatrix)entity;
                    data = new IgesTransformationMatrixParameterData()
                    {
                        R11 = matrix.R11,
                        R12 = matrix.R12,
                        R13 = matrix.R13,
                        T1 = matrix.T1,
                        R21 = matrix.R21,
                        R22 = matrix.R22,
                        R23 = matrix.R23,
                        T2 = matrix.T2,
                        R31 = matrix.R31,
                        R32 = matrix.R32,
                        R33 = matrix.R33,
                        T3 = matrix.T3
                    };
                    break;
                default:
                    throw new IgesException("Unsupported entity type: " + entity.Type.ToString());
            }

            data.DirectoryPointer = directoryPointer;

            return data;
        }

        private static IgesCircleParameterData ParseCircle(List<string> fields)
        {
            EnsureFieldCount(7, fields);
            return new IgesCircleParameterData()
            {
                ZT = ParseDouble(fields[0]),
                X1 = ParseDouble(fields[1]),
                Y1 = ParseDouble(fields[2]),
                X2 = ParseDouble(fields[3]),
                Y2 = ParseDouble(fields[4]),
                X3 = ParseDouble(fields[5]),
                Y3 = ParseDouble(fields[6])
            };
        }

        private static IgesLineParameterData ParseLine(List<string> fields)
        {
            EnsureFieldCount(6, fields);
            return new IgesLineParameterData()
            {
                X1 = ParseDouble(fields[0]),
                Y1 = ParseDouble(fields[1]),
                Z1 = ParseDouble(fields[2]),
                X2 = ParseDouble(fields[3]),
                Y2 = ParseDouble(fields[4]),
                Z2 = ParseDouble(fields[5])
            };
        }

        private static IgesTransformationMatrixParameterData ParseTransformationMatrix(List<string> fields)
        {
            EnsureFieldCount(12, fields);
            return new IgesTransformationMatrixParameterData()
            {
                R11 = ParseDouble(fields[0]),
                R12 = ParseDouble(fields[1]),
                R13 = ParseDouble(fields[2]),
                T1 = ParseDouble(fields[3]),
                R21 = ParseDouble(fields[4]),
                R22 = ParseDouble(fields[5]),
                R23 = ParseDouble(fields[6]),
                T2 = ParseDouble(fields[7]),
                R31 = ParseDouble(fields[8]),
                R32 = ParseDouble(fields[9]),
                R33 = ParseDouble(fields[10]),
                T3 = ParseDouble(fields[11])
            };
        }

        private static double ParseDouble(string value)
        {
            return double.Parse(value);
        }

        private static void EnsureFieldCount(int expectedFields, List<string> fields)
        {
            if (fields.Count != expectedFields)
                throw new IgesException("Incorrect number of fields");
        }
    }
}
