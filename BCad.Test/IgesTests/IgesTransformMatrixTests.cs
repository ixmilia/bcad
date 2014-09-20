using IxMilia.Iges;
using IxMilia.Iges.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.IgesTests
{
    [TestClass]
    public class IgesTransformMatrixTests
    {
        private static void TestTransform(IgesPoint input, IgesTransformationMatrix matrix, IgesPoint expected)
        {
            var result = matrix.Transform(input);
            Assert.AreEqual(expected.X, result.X);
            Assert.AreEqual(expected.Y, result.Y);
            Assert.AreEqual(expected.Z, result.Z);
        }

        [TestMethod]
        public void IdentityTransformTest()
        {
            var point = new IgesPoint(0.0, 0.0, 0.0);
            TestTransform(point, IgesTransformationMatrix.Identity, point);

            point = new IgesPoint(1.0, 2.0, 3.0);
            TestTransform(point, IgesTransformationMatrix.Identity, point);
        }
    }
}
