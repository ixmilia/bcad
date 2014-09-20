using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IxMilia.Iges;
using IxMilia.Iges.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.IgesTests
{
    [TestClass]
    public class IgesEntityReaderTests
    {

        #region Private methods

        private static IgesEntity ParseSingleEntity(string content)
        {
            var file = IgesReaderTests.CreateFile(content.Trim('\r', '\n'));
            Assert.AreEqual(1, file.Entities.Count);
            return file.Entities[0];
        }

        #endregion

        [TestMethod]
        public void UnsupportedEntityReadTest()
        {
            // entity id 888 is invalid
            var file = IgesReaderTests.CreateFile(@"
     888       1       0       0       0                               0D      1
     888       0       0       0       0                               0D      2
888,11,22,33,44,55,66;                                                 1P      1
".Trim('\r', '\n'));
            Assert.AreEqual(0, file.Entities.Count);
        }

        [TestMethod]
        public void LineReadTest()
        {
            var line = (IgesLine)ParseSingleEntity(@"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                               0D      2
110,11,22,33,44,55,66;                                                 1P      1
");
            Assert.AreEqual(11.0, line.P1.X);
            Assert.AreEqual(22.0, line.P1.Y);
            Assert.AreEqual(33.0, line.P1.Z);
            Assert.AreEqual(44.0, line.P2.X);
            Assert.AreEqual(55.0, line.P2.Y);
            Assert.AreEqual(66.0, line.P2.Z);
            Assert.AreEqual(IgesColorNumber.Green, line.Color);

            // verify transformation matrix is identity
            Assert.AreEqual(1.0, line.TransformationMatrix.R11);
            Assert.AreEqual(0.0, line.TransformationMatrix.R12);
            Assert.AreEqual(0.0, line.TransformationMatrix.R13);
            Assert.AreEqual(0.0, line.TransformationMatrix.R21);
            Assert.AreEqual(1.0, line.TransformationMatrix.R22);
            Assert.AreEqual(0.0, line.TransformationMatrix.R23);
            Assert.AreEqual(0.0, line.TransformationMatrix.R31);
            Assert.AreEqual(0.0, line.TransformationMatrix.R32);
            Assert.AreEqual(1.0, line.TransformationMatrix.R33);
            Assert.AreEqual(0.0, line.TransformationMatrix.T1);
            Assert.AreEqual(0.0, line.TransformationMatrix.T2);
            Assert.AreEqual(0.0, line.TransformationMatrix.T3);
        }

        [TestMethod]
        public void TransformationMatrixReadTest()
        {
            var matrix = (IgesTransformationMatrix)ParseSingleEntity(@"
     124       1       0       0       0                               0D      1
     124       0       0       4       0                               0D      2
124,1,2,3,4,5,6,7,8,9,10,11,12;                                        1P      1
");
            Assert.AreEqual(1.0, matrix.R11);
            Assert.AreEqual(2.0, matrix.R12);
            Assert.AreEqual(3.0, matrix.R13);
            Assert.AreEqual(4.0, matrix.T1);
            Assert.AreEqual(5.0, matrix.R21);
            Assert.AreEqual(6.0, matrix.R22);
            Assert.AreEqual(7.0, matrix.R23);
            Assert.AreEqual(8.0, matrix.T2);
            Assert.AreEqual(9.0, matrix.R31);
            Assert.AreEqual(10.0, matrix.R32);
            Assert.AreEqual(11.0, matrix.R33);
            Assert.AreEqual(12.0, matrix.T3);
        }

        [TestMethod]
        public void TransformationMatrixFromEntityTest()
        {
            var file = IgesReaderTests.CreateFile(@"
     124       1       0       0       0                               0D      1
     124       0       0       4       0                               0D      2
     110       2       0       0       0               1               0D      3
     110       0       3       1       0                               0D      4
124,1,2,3,4,5,6,7,8,9,10,11,12;                                        1P      1
110,11,22,33,44,55,66;                                                 3P      2
".Trim('\r', '\n'));
            var matrix = file.Entities.Single(e => e.EntityType == IgesEntityType.Line).TransformationMatrix;
            Assert.AreEqual(1.0, matrix.R11);
            Assert.AreEqual(2.0, matrix.R12);
            Assert.AreEqual(3.0, matrix.R13);
            Assert.AreEqual(4.0, matrix.T1);
            Assert.AreEqual(5.0, matrix.R21);
            Assert.AreEqual(6.0, matrix.R22);
            Assert.AreEqual(7.0, matrix.R23);
            Assert.AreEqual(8.0, matrix.T2);
            Assert.AreEqual(9.0, matrix.R31);
            Assert.AreEqual(10.0, matrix.R32);
            Assert.AreEqual(11.0, matrix.R33);
            Assert.AreEqual(12.0, matrix.T3);
        }

        [TestMethod]
        public void CircleReadTest()
        {
            var circle = (IgesCircularArc)ParseSingleEntity(@"
     100       1       0       0       0                               0D      1
     100       0       3       1       0                               0D      2
100,11,22,33,44,55,66,77;                                              1P      1
");
            Assert.AreEqual(11.0, circle.PlaneDisplacement);
            Assert.AreEqual(22.0, circle.Center.X);
            Assert.AreEqual(33.0, circle.Center.Y);
            Assert.AreEqual(0.0, circle.Center.Z);
            Assert.AreEqual(44.0, circle.StartPoint.X);
            Assert.AreEqual(55.0, circle.StartPoint.Y);
            Assert.AreEqual(0.0, circle.StartPoint.Z);
            Assert.AreEqual(66.0, circle.EndPoint.X);
            Assert.AreEqual(77.0, circle.EndPoint.Y);
            Assert.AreEqual(0.0, circle.EndPoint.Z);
            Assert.AreEqual(IgesColorNumber.Green, circle.Color);
        }

        [TestMethod]
        public void ReadSubfigureTest()
        {
            var entity = ParseSingleEntity(@"
// The subfigure has two lines; one defined before and one after.       S      1
     110       1       0       0       0                               0D      1
     110       0       0       1       0                               0D      2
     308       2       0       0       0                               0D      3
     308       0       0       1       0                               0D      4
     110       3       0       0       0                               0D      5
     110       0       0       1       0                               0D      6
110,1.0,2.0,3.0,4.0,5.0,6.0;                                            P      1
308,0,21Hthis is the subfigure,2,1,5;                                   P      2
110,7.0,8.0,9.0,10.0,11.0,12.0;                                         P      3
");
            Assert.AreEqual(IgesEntityType.SubfigureDefinition, entity.EntityType);
            var subfigure = (IgesSubfigureDefinition)entity;
            Assert.AreEqual(0, subfigure.Depth);
            Assert.AreEqual("this is the subfigure", subfigure.Name);
            Assert.AreEqual(2, subfigure.Entities.Count);
            Assert.AreEqual(IgesEntityType.Line, subfigure.Entities[0].EntityType);
            Assert.AreEqual(IgesEntityType.Line, subfigure.Entities[1].EntityType);
            var line1 = (IgesLine)subfigure.Entities[0];
            Assert.AreEqual(1.0, line1.P1.X);
            Assert.AreEqual(2.0, line1.P1.Y);
            Assert.AreEqual(3.0, line1.P1.Z);
            Assert.AreEqual(4.0, line1.P2.X);
            Assert.AreEqual(5.0, line1.P2.Y);
            Assert.AreEqual(6.0, line1.P2.Z);
            var line2 = (IgesLine)subfigure.Entities[1];
            Assert.AreEqual(7.0, line2.P1.X);
            Assert.AreEqual(8.0, line2.P1.Y);
            Assert.AreEqual(9.0, line2.P1.Z);
            Assert.AreEqual(10.0, line2.P2.X);
            Assert.AreEqual(11.0, line2.P2.Y);
            Assert.AreEqual(12.0, line2.P2.Z);
        }
    }
}
