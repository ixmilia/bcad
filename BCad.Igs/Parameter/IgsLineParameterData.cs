using BCad.Igs.Directory;
using BCad.Igs.Entities;

namespace BCad.Igs.Parameter
{
    internal class IgsLineParameterData : IgsParameterData
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }

        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public override IgsEntity ToEntity(IgsDirectoryData dir)
        {
            if (dir.LineCount != 1)
                throw new IgsException("Invalid line count");
            return new IgsLine(IgsBounding.BoundOnBothSides, X1, Y1, Z1, X2, Y2, Z2)
            {
                Color = dir.Color
            };
        }
    }
}
