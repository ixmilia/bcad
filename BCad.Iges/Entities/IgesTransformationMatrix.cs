using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCad.Iges.Entities
{
    public class IgesTransformationMatrix : IgesEntity
    {
        public override IgesEntityType Type { get { return IgesEntityType.TransformationMatrix; } }

        public override int LineCount { get { return 0; } }

        public double R11 { get; set; }
        public double R12 { get; set; }
        public double R13 { get; set; }

        public double R21 { get; set; }
        public double R22 { get; set; }
        public double R23 { get; set; }

        public double R31 { get; set; }
        public double R32 { get; set; }
        public double R33 { get; set; }

        public double T1 { get; set; }
        public double T2 { get; set; }
        public double T3 { get; set; }

        public IgesTransformationMatrix()
        {
        }

        public IgesPoint Transform(IgesPoint point)
        {
            return new IgesPoint(
                (R11 * point.X + R12 * point.Y + R13 * point.Z) + T1,
                (R21 * point.X + R22 * point.Y + R23 * point.Z) + T2,
                (R31 * point.X + R32 * point.Y + R33 * point.Z) + T3);
        }

        public static IgesTransformationMatrix Identity
        {
            get
            {
                return new IgesTransformationMatrix()
                {
                    R11 = 1.0,
                    R12 = 0.0,
                    R13 = 0.0,
                    R21 = 0.0,
                    R22 = 1.0,
                    R23 = 0.0,
                    R31 = 0.0,
                    R32 = 0.0,
                    R33 = 1.0,
                    T1 = 0.0,
                    T2 = 0.0,
                    T3 = 0.0
                };
            }
        }
    }
}
