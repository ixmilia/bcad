using BCad.Iegs.Directory;
using BCad.Iegs.Entities;

namespace BCad.Iegs.Parameter
{
    internal class IegsLineParameterData : IegsParameterData
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }

        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public override IegsEntity ToEntity(IegsDirectoryData dir)
        {
            if (dir.LineCount != 1)
                throw new IegsException("Invalid line count");
            return new IegsLine()
            {
                X1 = X1,
                Y1 = Y1,
                Z1 = Z1,
                X2 = X2,
                Y2 = Y2,
                Z2 = Z2,
                Bounding = GetBounding(dir.FormNumber),
                Color = dir.Color
            };
        }

        private static IegsBounding GetBounding(int form)
        {
            switch (form)
            {
                case 0: return IegsBounding.BoundOnBothSides;
                case 1: return IegsBounding.BoundOnStart;
                case 2: return IegsBounding.Unbound;
                default:
                    throw new IegsException("Invalid line bounding value");
            }
        }
    }
}
