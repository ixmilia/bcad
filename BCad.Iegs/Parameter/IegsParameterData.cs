using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BCad.Iegs.Directory;
using BCad.Iegs.Entities;

namespace BCad.Iegs.Parameter
{
    internal abstract class IegsParameterData
    {
        public abstract IegsEntity ToEntity(IegsDirectoryData dir);

        protected abstract object[] GetFields();

        private const string Separator = ","; // TODO: parameterize this

        private const string Terminator = ";";

        public string ToString(IegsEntityType type, int lineNumber)
        {
            var sb = new StringBuilder();
            sb.Append((int)type);
            var fields = GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                // TODO: break, don't wrap, long lines
                sb.Append(Separator);
                sb.Append(ParameterToString(fields[i]));
            }

            sb.Append(Terminator);
            return sb.ToString();
        }

        public static IegsParameterData ParseFields(IegsEntityType type, List<string> fields)
        {
            switch (type)
            {
                case IegsEntityType.Circle:
                    return ParseCircle(fields);
                case IegsEntityType.Line:
                    return ParseLine(fields);
                case IegsEntityType.TransformationMatrix:
                    return ParseTransformationMatrix(fields);
                default:
                    return null;
            }
        }

        public static IegsParameterData FromEntity(IegsEntity entity)
        {
            IegsParameterData data;
            switch (entity.Type)
            {
                case IegsEntityType.Circle:
                    var circle = (IegsCircle)entity;
                    data = new IegsCircleParameterData()
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
                case IegsEntityType.Line:
                    var line = (IegsLine)entity;
                    data = new IegsLineParameterData()
                    {
                        X1 = line.P1.X,
                        Y1 = line.P1.Y,
                        Z1 = line.P1.Z,
                        X2 = line.P2.X,
                        Y2 = line.P2.Y,
                        Z2 = line.P2.Z
                    };
                    break;
                case IegsEntityType.TransformationMatrix:
                    var matrix = (IegsTransformationMatrix)entity;
                    data = new IegsTransformationMatrixParameterData()
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
                    throw new IegsException("Unsupported entity type: " + entity.Type.ToString());
            }

            return data;
        }

        private static string ParameterToString(object parameter)
        {
            var type = parameter.GetType();
            if (type == typeof(double))
                return ParameterToString((double)parameter);
            else if (type == typeof(string))
                return ParameterToString((string)parameter);
            else
            {
                Debug.Fail("Unsupported parameter type: " + type.ToString());
                return string.Empty;
            }
        }

        private static string ParameterToString(double parameter)
        {
            return parameter.ToString();
        }

        private static string ParameterToString(string parameter)
        {
            parameter = parameter ?? string.Empty;
            return string.Format("{0}H{1}", parameter.Length, parameter);
        }

        private static IegsCircleParameterData ParseCircle(List<string> fields)
        {
            EnsureFieldCount(7, fields);
            return new IegsCircleParameterData()
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

        private static IegsLineParameterData ParseLine(List<string> fields)
        {
            EnsureFieldCount(6, fields);
            return new IegsLineParameterData()
            {
                X1 = ParseDouble(fields[0]),
                Y1 = ParseDouble(fields[1]),
                Z1 = ParseDouble(fields[2]),
                X2 = ParseDouble(fields[3]),
                Y2 = ParseDouble(fields[4]),
                Z2 = ParseDouble(fields[5])
            };
        }

        private static IegsTransformationMatrixParameterData ParseTransformationMatrix(List<string> fields)
        {
            EnsureFieldCount(12, fields);
            return new IegsTransformationMatrixParameterData()
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
                throw new IegsException("Incorrect number of fields");
        }
    }
}
