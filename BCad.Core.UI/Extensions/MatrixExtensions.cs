using System.Windows.Media.Media3D;

namespace BCad.Core.UI.Extensions
{
    public static class MatrixExtensions
    {
        public static Matrix3D ToMatrix3D(this Matrix4 matrix)
        {
            return new Matrix3D(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }
    }
}
