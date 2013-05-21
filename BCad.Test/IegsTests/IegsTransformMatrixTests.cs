using BCad.Iegs;
using BCad.Iegs.Entities;
using Xunit;

namespace BCad.Test.IegsTests
{
    public class IegsTransformMatrixTests
    {
        private static void TestTransform(IegsPoint input, IegsTransformationMatrix matrix, IegsPoint expected)
        {
            var result = matrix.Transform(input);
            Assert.Equal(expected.X, result.X);
            Assert.Equal(expected.Y, result.Y);
            Assert.Equal(expected.Z, result.Z);
        }

        [Fact]
        public void IdentityTransformTest()
        {
            var point = new IegsPoint(0.0, 0.0, 0.0);
            TestTransform(point, IegsTransformationMatrix.Identity, point);

            point = new IegsPoint(1.0, 2.0, 3.0);
            TestTransform(point, IegsTransformationMatrix.Identity, point);
        }
    }
}
