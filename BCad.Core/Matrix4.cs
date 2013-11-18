using MathNet.Numerics.LinearAlgebra.Double;

namespace BCad
{
    public struct Matrix4
    {
        /// <summary>
        /// First row, first column.
        /// </summary>
        public double M11 { get; set; }

        /// <summary>
        /// First row, second column.
        /// </summary>
        public double M12 { get; set; }

        /// <summary>
        /// First row, third column.
        /// </summary>
        public double M13 { get; set; }
        
        /// <summary>
        /// First row, fourth column.
        /// </summary>
        public double M14 { get; set; }


        /// <summary>
        /// Second row, first column.
        /// </summary>
        public double M21 { get; set; }

        /// <summary>
        /// Second row, second column.
        /// </summary>
        public double M22 { get; set; }

        /// <summary>
        /// Second row, third column.
        /// </summary>
        public double M23 { get; set; }

        /// <summary>
        /// Second row, fourth column.
        /// </summary>
        public double M24 { get; set; }


        /// <summary>
        /// Third row, first column.
        /// </summary>
        public double M31 { get; set; }

        /// <summary>
        /// Third row, second column.
        /// </summary>
        public double M32 { get; set; }

        /// <summary>
        /// Third row, third column.
        /// </summary>
        public double M33 { get; set; }

        /// <summary>
        /// Third row, fourth column.
        /// </summary>
        public double M34 { get; set; }


        /// <summary>
        /// Fourth row, first column.
        /// </summary>
        public double OffsetX { get; set; }

        /// <summary>
        /// Fourth row, second column.
        /// </summary>
        public double OffsetY { get; set; }

        /// <summary>
        /// Fourth row, third column.
        /// </summary>
        public double OffsetZ { get; set; }

        /// <summary>
        /// Fourth row, fourth column.
        /// </summary>
        public double M44 { get; set; }

        private double M41 { set { OffsetX = value; } get { return OffsetX; } }
        private double M42 { set { OffsetY = value; } get { return OffsetY; } }
        private double M43 { set { OffsetZ = value; } get { return OffsetZ; } }

        public Matrix4(double m11, double m12, double m13, double m14, double m21, double m22, double m23, double m24, double m31, double m32, double m33, double m34, double offsetX, double offsetY, double offsetZ, double m44)
            : this()
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            OffsetX = offsetX;
            OffsetY = offsetY;
            OffsetZ = offsetZ;
            M44 = m44;
        }

        public static Matrix4 operator *(Matrix4 a, Matrix4 b)
        {
            return new Matrix4(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
        }

        public static Matrix4 Identity
        {
            get
            {
                return new Matrix4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);
            }
        }

        public Vector Transform(Vector b)
        {
            var x = b.X * (M11 + M12 + M13 + M14);
            var y = b.Y * (M21 + M22 + M23 + M24);
            var z = b.Z * (M31 + M32 + M33 + M34);
            var w = M41 + M42 + M43 + M44;
            return new Vector(x / w, y / w, z / w);
        }

        public Point Transform(Point b)
        {
            var x = b.X * (M11 + M12 + M13 + M14);
            var y = b.Y * (M21 + M22 + M23 + M24);
            var z = b.Z * (M31 + M32 + M33 + M34);
            var w = M41 + M42 + M43 + M44;
            return new Point(x / w, y / w, z / w);
        }

        public Matrix4 Scale(Vector v)
        {
            return new Matrix4(
                M11 * v.X, M12, M13, M14,
                M21, M22 * v.Y, M23, M24,
                M31, M32, M33 * v.Z, M34,
                M41, M42, M43, M44);
        }

        public void Invert()
        {
            var matrix = ToDenseMatrix();
            var inverse = matrix.Inverse();
            M11 = inverse[0, 0];
            M12 = inverse[0, 1];
            M13 = inverse[0, 2];
            M14 = inverse[0, 3];
            M21 = inverse[1, 0];
            M22 = inverse[1, 1];
            M23 = inverse[1, 2];
            M24 = inverse[1, 3];
            M31 = inverse[2, 0];
            M32 = inverse[2, 1];
            M33 = inverse[2, 2];
            M34 = inverse[2, 3];
            M41 = inverse[3, 0];
            M42 = inverse[3, 1];
            M43 = inverse[3, 2];
            M44 = inverse[3, 3];
        }

        internal void RotateAt(Quaternion quaternion, Point center)
        {
            var rotation = CreateRotationMatrix(quaternion, center);
            var result = this * rotation;
            M11 = rotation.M11;
            M12 = rotation.M12;
            M13 = rotation.M13;
            M14 = rotation.M14;
            M21 = rotation.M21;
            M22 = rotation.M22;
            M23 = rotation.M23;
            M24 = rotation.M24;
            M31 = rotation.M31;
            M32 = rotation.M32;
            M33 = rotation.M33;
            M34 = rotation.M34;
            M41 = rotation.M41;
            M42 = rotation.M42;
            M43 = rotation.M43;
            M44 = rotation.M44;
        }

        internal static Matrix4 CreateRotationMatrix(Quaternion quaternion, Point center)
        {
            var matrix = Identity;

            var x2 = quaternion.X + quaternion.X;
            var y2 = quaternion.Y + quaternion.Y;
            var z2 = quaternion.Z + quaternion.Z;
            var xx = quaternion.X * x2;
            var xy = quaternion.X * y2;
            var xz = quaternion.X * z2;
            var yy = quaternion.Y * y2;
            var yz = quaternion.Y * z2;
            var zz = quaternion.Z * z2;
            var wx = quaternion.W * x2;
            var wy = quaternion.W * y2;
            var wz = quaternion.W * z2;

            matrix.M11 = 1.0 - (yy + zz);
            matrix.M12 = xy + wz;
            matrix.M13 = xz - wy;
            matrix.M21 = xy - wz;
            matrix.M22 = 1.0 - (xx + zz);
            matrix.M23 = yz + wx;
            matrix.M31 = xz + wy;
            matrix.M32 = yz - wx;
            matrix.M33 = 1.0 - (xx + yy);

            if (center.X != 0 || center.Y != 0 || center.Z != 0)
            {
                matrix.OffsetX = -center.X * matrix.M11 - center.Y * matrix.M21 - center.Z * matrix.M31 + center.X;
                matrix.OffsetY = -center.X * matrix.M12 - center.Y * matrix.M22 - center.Z * matrix.M32 + center.Y;
                matrix.OffsetZ = -center.X * matrix.M13 - center.Y * matrix.M23 - center.Z * matrix.M33 + center.Z;
            }

            return matrix;
        }

        private DenseMatrix ToDenseMatrix()
        {
            var data = new double[4, 4];
            data[0, 0] = M11;
            data[0, 1] = M12;
            data[0, 2] = M13;
            data[0, 3] = M14;
            data[1, 0] = M21;
            data[1, 1] = M22;
            data[1, 2] = M23;
            data[1, 3] = M24;
            data[2, 0] = M31;
            data[2, 1] = M32;
            data[2, 2] = M33;
            data[2, 3] = M34;
            data[3, 0] = M41;
            data[3, 1] = M42;
            data[3, 2] = M43;
            data[3, 3] = M44;
            return DenseMatrix.OfArray(data);
        }
    }
}
