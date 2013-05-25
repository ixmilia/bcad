using BCad.Iges.Directory;
using BCad.Iges.Entities;

namespace BCad.Iges.Parameter
{
    internal class IgesLineParameterData : IgesParameterData
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }

        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public override IgesEntity ToEntity(IgesDirectoryData dir)
        {
            if (dir.LineCount != 1)
                throw new IgesException("Invalid line count");
            return new IgesLine()
            {
                P1 = new IgesPoint(X1, Y1, Z1),
                P2 = new IgesPoint(X2, Y2, Z2),
                Bounding = GetBounding(dir.FormNumber)
            };
        }

        protected override object[] GetFields()
        {
            return new object[]
            {
                X1,
                Y1,
                Z1,
                X2,
                Y2,
                Z2
            };
        }

        private static IgesBounding GetBounding(int form)
        {
            switch (form)
            {
                case 0: return IgesBounding.BoundOnBothSides;
                case 1: return IgesBounding.BoundOnStart;
                case 2: return IgesBounding.Unbound;
                default:
                    throw new IgesException("Invalid line bounding value");
            }
        }
    }
}
