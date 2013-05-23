using BCad.Iegs.Directory;
using BCad.Iegs.Entities;

namespace BCad.Iegs.Parameter
{
    internal class IegsCircleParameterData : IegsParameterData
    {
        public double ZT { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }

        public double X2 { get; set; }
        public double Y2 { get; set; }

        public double X3 { get; set; }
        public double Y3 { get; set; }

        public override IegsEntity ToEntity(IegsDirectoryData dir)
        {
            if (dir.LineCount != 1)
                throw new IegsException("Invalid line count");
            return new IegsCircle()
            {
                PlaneDisplacement = ZT,
                Center = new IegsPoint(X1, Y1, 0.0),
                StartPoint = new IegsPoint(X2, Y2, 0.0),
                EndPoint = new IegsPoint(X3, Y3, 0.0)
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
