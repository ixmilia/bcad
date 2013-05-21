using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Iegs;
using BCad.Iegs.Entities;
using Xunit;

namespace BCad.Test.IegsTests
{
    public class IegsEntityReaderTests
    {

        #region Private methods

        private static IegsEntity ParseSingleEntity(string content)
        {
            var file = IegsReaderTests.CreateFile(content.Trim('\r', '\n'));
            Assert.Equal(1, file.Entities.Count);
            return file.Entities[0];
        }

        #endregion

        [Fact]
        public void UnsupportedEntityReadTest()
        {
            // entity id 888 is invalid
            var file = IegsReaderTests.CreateFile(@"
     888       1       0       0       0                               0D      1
     888       0       0       0       0                               0D      2
888,11,22,33,44,55,66;                                                  P      1
".Trim('\r', '\n'));
            Assert.Equal(0, file.Entities.Count);
        }

        [Fact]
        public void LineReadTest()
        {
            var line = (IegsLine)ParseSingleEntity(@"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                               0D      2
110,11,22,33,44,55,66;                                                  P      1
");
            Assert.Equal(11.0, line.P1.X);
            Assert.Equal(22.0, line.P1.Y);
            Assert.Equal(33.0, line.P1.Z);
            Assert.Equal(44.0, line.P2.X);
            Assert.Equal(55.0, line.P2.Y);
            Assert.Equal(66.0, line.P2.Z);
            Assert.Equal(IegsColorNumber.Color3, line.Color);

            // verify transformation matrix is identity
            Assert.Equal(1.0, line.TransformationMatrix.R11);
            Assert.Equal(0.0, line.TransformationMatrix.R12);
            Assert.Equal(0.0, line.TransformationMatrix.R13);
            Assert.Equal(0.0, line.TransformationMatrix.R21);
            Assert.Equal(1.0, line.TransformationMatrix.R22);
            Assert.Equal(0.0, line.TransformationMatrix.R23);
            Assert.Equal(0.0, line.TransformationMatrix.R31);
            Assert.Equal(0.0, line.TransformationMatrix.R32);
            Assert.Equal(1.0, line.TransformationMatrix.R33);
            Assert.Equal(0.0, line.TransformationMatrix.T1);
            Assert.Equal(0.0, line.TransformationMatrix.T2);
            Assert.Equal(0.0, line.TransformationMatrix.T3);
        }

        [Fact]
        public void TransformationMatrixReadTest()
        {
            var matrix = (IegsTransformationMatrix)ParseSingleEntity(@"
     124       1       0       0       0                               0D      1
     124       0       0       4       0                               0D      2
124,1,2,3,4,5,6,7,8,9,10,11,12                                          P      1
");
            Assert.Equal(1.0, matrix.R11);
            Assert.Equal(2.0, matrix.R12);
            Assert.Equal(3.0, matrix.R13);
            Assert.Equal(4.0, matrix.T1);
            Assert.Equal(5.0, matrix.R21);
            Assert.Equal(6.0, matrix.R22);
            Assert.Equal(7.0, matrix.R23);
            Assert.Equal(8.0, matrix.T2);
            Assert.Equal(9.0, matrix.R31);
            Assert.Equal(10.0, matrix.R32);
            Assert.Equal(11.0, matrix.R33);
            Assert.Equal(12.0, matrix.T3);
        }

        [Fact]
        public void TransformationMatrixFromEntityTest()
        {
            // entity id 888 is invalid
            var file = IegsReaderTests.CreateFile(@"
     110       1       0       0       0               3               0D      1
     110       0       3       1       0                               0D      2
     124       1       0       0       0                               0D      3
     124       0       0       4       0                               0D      4
110,11,22,33,44,55,66;                                                  P      1
124,1,2,3,4,5,6,7,8,9,10,11,12                                          P      2
".Trim('\r', '\n'));
            var matrix = file.Entities.Single().TransformationMatrix;
            Assert.Equal(1.0, matrix.R11);
            Assert.Equal(2.0, matrix.R12);
            Assert.Equal(3.0, matrix.R13);
            Assert.Equal(4.0, matrix.T1);
            Assert.Equal(5.0, matrix.R21);
            Assert.Equal(6.0, matrix.R22);
            Assert.Equal(7.0, matrix.R23);
            Assert.Equal(8.0, matrix.T2);
            Assert.Equal(9.0, matrix.R31);
            Assert.Equal(10.0, matrix.R32);
            Assert.Equal(11.0, matrix.R33);
            Assert.Equal(12.0, matrix.T3);
        }
    }
}
