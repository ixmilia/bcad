using System.Collections.Generic;
using BCad.Igs.Entities;

namespace BCad.Igs.Parameter
{
    internal abstract class IgsParameterData
    {
        public abstract IgsEntity ToEntity();

        public static IgsParameterData ParseFields(IgsEntityType type, List<string> fields)
        {
            switch (type)
            {
                case IgsEntityType.Line:
                    return ParseLine(fields);
                default:
                    return null;
            }
        }

        private static IgsLineParameterData ParseLine(List<string> fields)
        {
            if (fields.Count != 6)
                throw new IgsException("Incorrect number of fields");
            return new IgsLineParameterData()
            {
                X1 = ParseDouble(fields[0]),
                Y1 = ParseDouble(fields[1]),
                Z1 = ParseDouble(fields[2]),
                X2 = ParseDouble(fields[3]),
                Y2 = ParseDouble(fields[4]),
                Z2 = ParseDouble(fields[5])
            };
        }

        private static double ParseDouble(string value)
        {
            return double.Parse(value);
        }
    }
}
