using System.Collections.Generic;
using BCad.Igs.Directory;
using BCad.Igs.Entities;

namespace BCad.Igs.Parameter
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
                throw new IgsException("Incorrect number of fields");
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

        private static IgsTransformationMatrixParameterData ParseTransformationMatrix(List<string> fields)
        {
            if (fields.Count != 12)
                throw new IgsException("Incorrect number of fields");
            return new IgsTransformationMatrixParameterData()
            {
            };
        }

        private static double ParseDouble(string value)
        {
            return double.Parse(value);
        }
    }
}
