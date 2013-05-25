using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Iges.Directory;
using BCad.Iges.Entities;

namespace BCad.Iges.Parameter
{
    internal class IgesTransformationMatrixParameterData : IgesParameterData
    {
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

        public override IgesEntity ToEntity(IgesDirectoryData dir)
        {
            return new IgesTransformationMatrix()
            {
                R11 = R11,
                R12 = R12,
                R13 = R13,
                R21 = R21,
                R22 = R22,
                R23 = R23,
                R31 = R31,
                R32 = R32,
                R33 = R33,
                T1 = T1,
                T2 = T2,
                T3 = T3
            };
        }

        protected override object[] GetFields()
        {
            return new object[]
            {
                R11,
                R12,
                R13,
                T1,
                R21,
                R22,
                R23,
                T2,
                R31,
                R32,
                R33,
                T3
            };
        }
    }
}
