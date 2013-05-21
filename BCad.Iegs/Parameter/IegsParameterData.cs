using System.Collections.Generic;
using BCad.Iegs.Directory;
using BCad.Iegs.Entities;

namespace BCad.Iegs.Parameter
{
    internal abstract class IegsParameterData
    {
        public abstract IegsEntity ToEntity(IegsDirectoryData dir);

        public static IegsParameterData ParseFields(IegsEntityType type, List<string> fields)
        {
            switch (type)
            {
                case IegsEntityType.Line:
                    return ParseLine(fields);
                case IegsEntityType.TransformationMatrix:
                    return ParseTransformationMatrix(fields);
                default:
                    return null;
            }
        }

        private static IegsLineParameterData ParseLine(List<string> fields)
        {
            if (fields.Count != 6)
                throw new IegsException("Incorrect number of fields");
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
            if (fields.Count != 12)
                throw new IegsException("Incorrect number of fields");
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
    }
}
