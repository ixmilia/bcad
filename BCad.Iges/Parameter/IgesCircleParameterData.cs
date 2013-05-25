using BCad.Iges.Directory;
using BCad.Iges.Entities;

namespace BCad.Iges.Parameter
{
    internal class IgesCircleParameterData : IgesParameterData
    {
        public double ZT { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }

        public double X2 { get; set; }
        public double Y2 { get; set; }

        public double X3 { get; set; }
        public double Y3 { get; set; }

        public override IgesEntity ToEntity(IgesDirectoryData dir)
        {
            if (dir.LineCount != 1)
                throw new IgesException("Invalid line count");
            return new IgesCircle()
            {
                PlaneDisplacement = ZT,
                Center = new IgesPoint(X1, Y1, 0.0),
                StartPoint = new IgesPoint(X2, Y2, 0.0),
                EndPoint = new IgesPoint(X3, Y3, 0.0)
            };
        }

        protected override object[] GetFields()
        {
            return new object[]
            {
                ZT,
                X1,
                Y1,
                X2,
                Y2,
                X3,
                Y3
            };
        }
    }
}
