using System;
using BCad.Helpers;

namespace BCad
{
    public struct Matrix4
    {
        private double _m11;
        private double _m12;
        private double _m13;
        private double _m14;
        private double _m21;
        private double _m22;
        private double _m23;
        private double _m24;
        private double _m31;
        private double _m32;
        private double _m33;
        private double _m34;
        private double _m41;
        private double _m42;
        private double _m43;
        private double _m44;

        /// <summary>
        /// First row, first column.
        /// </summary>
        public double M11 { get { return _m11; } set { _m11 = value; } }

        /// <summary>
        /// First row, second column.
        /// </summary>
        public double M12 { get { return _m12; } set { _m12 = value; } }

        /// <summary>
        /// First row, third column.
        /// </summary>
        public double M13 { get { return _m13; } set { _m13 = value; } }
        
        /// <summary>
        /// First row, fourth column.
        /// </summary>
        public double M14 { get { return _m14; } set { _m14 = value; } }


        /// <summary>
        /// Second row, first column.
        /// </summary>
        public double M21 { get { return _m21; } set { _m21 = value; } }

        /// <summary>
        /// Second row, second column.
        /// </summary>
        public double M22 { get { return _m22; } set { _m22 = value; } }

        /// <summary>
        /// Second row, third column.
        /// </summary>
        public double M23 { get { return _m23; } set { _m23 = value; } }

        /// <summary>
        /// Second row, fourth column.
        /// </summary>
        public double M24 { get { return _m24; } set { _m24 = value; } }


        /// <summary>
        /// Third row, first column.
        /// </summary>
        public double M31 { get { return _m31; } set { _m31 = value; } }

        /// <summary>
        /// Third row, second column.
        /// </summary>
        public double M32 { get { return _m32; } set { _m32 = value; } }

        /// <summary>
        /// Third row, third column.
        /// </summary>
        public double M33 { get { return _m33; } set { _m33 = value; } }

        /// <summary>
        /// Third row, fourth column.
        /// </summary>
        public double M34 { get { return _m34; } set { _m34 = value; } }


        /// <summary>
        /// Fourth row, first column.
        /// </summary>
        public double M41 { get { return _m41; } set { _m41 = value; } }

        /// <summary>
        /// Fourth row, second column.
        /// </summary>
        public double M42 { get { return _m42; } set { _m42 = value; } }

        /// <summary>
        /// Fourth row, third column.
        /// </summary>
        public double M43 { get { return _m43; } set { _m43 = value; } }

        /// <summary>
        /// Fourth row, fourth column.
        /// </summary>
        public double M44 { get { return _m44; } set { _m44 = value; } }

        public Matrix4(double m11, double m12, double m13, double m14, double m21, double m22, double m23, double m24, double m31, double m32, double m33, double m34, double m41, double m42, double m43, double m44)
            : this()
        {
            _m11 = m11;
            _m12 = m12;
            _m13 = m13;
            _m14 = m14;
            _m21 = m21;
            _m22 = m22;
            _m23 = m23;
            _m24 = m24;
            _m31 = m31;
            _m32 = m32;
            _m33 = m33;
            _m34 = m34;
            _m41 = m41;
            _m42 = m42;
            _m43 = m43;
            _m44 = m44;
        }

        public static Matrix4 operator *(Matrix4 matrix1, Matrix4 matrix2)
        {
            return new Matrix4(
                matrix1._m11 * matrix2._m11 + matrix1._m12 * matrix2._m21 +
                matrix1._m13 * matrix2._m31 + matrix1._m14 * matrix2._m41,
                matrix1._m11 * matrix2._m12 + matrix1._m12 * matrix2._m22 +
                matrix1._m13 * matrix2._m32 + matrix1._m14 * matrix2._m42,
                matrix1._m11 * matrix2._m13 + matrix1._m12 * matrix2._m23 +
                matrix1._m13 * matrix2._m33 + matrix1._m14 * matrix2._m43,
                matrix1._m11 * matrix2._m14 + matrix1._m12 * matrix2._m24 +
                matrix1._m13 * matrix2._m34 + matrix1._m14 * matrix2._m44,
                matrix1._m21 * matrix2._m11 + matrix1._m22 * matrix2._m21 +
                matrix1._m23 * matrix2._m31 + matrix1._m24 * matrix2._m41,
                matrix1._m21 * matrix2._m12 + matrix1._m22 * matrix2._m22 +
                matrix1._m23 * matrix2._m32 + matrix1._m24 * matrix2._m42,
                matrix1._m21 * matrix2._m13 + matrix1._m22 * matrix2._m23 +
                matrix1._m23 * matrix2._m33 + matrix1._m24 * matrix2._m43,
                matrix1._m21 * matrix2._m14 + matrix1._m22 * matrix2._m24 +
                matrix1._m23 * matrix2._m34 + matrix1._m24 * matrix2._m44,
                matrix1._m31 * matrix2._m11 + matrix1._m32 * matrix2._m21 +
                matrix1._m33 * matrix2._m31 + matrix1._m34 * matrix2._m41,
                matrix1._m31 * matrix2._m12 + matrix1._m32 * matrix2._m22 +
                matrix1._m33 * matrix2._m32 + matrix1._m34 * matrix2._m42,
                matrix1._m31 * matrix2._m13 + matrix1._m32 * matrix2._m23 +
                matrix1._m33 * matrix2._m33 + matrix1._m34 * matrix2._m43,
                matrix1._m31 * matrix2._m14 + matrix1._m32 * matrix2._m24 +
                matrix1._m33 * matrix2._m34 + matrix1._m34 * matrix2._m44,
                matrix1._m41 * matrix2._m11 + matrix1._m42 * matrix2._m21 +
                matrix1._m43 * matrix2._m31 + matrix1._m44 * matrix2._m41,
                matrix1._m41 * matrix2._m12 + matrix1._m42 * matrix2._m22 +
                matrix1._m43 * matrix2._m32 + matrix1._m44 * matrix2._m42,
                matrix1._m41 * matrix2._m13 + matrix1._m42 * matrix2._m23 +
                matrix1._m43 * matrix2._m33 + matrix1._m44 * matrix2._m43,
                matrix1._m41 * matrix2._m14 + matrix1._m42 * matrix2._m24 +
                matrix1._m43 * matrix2._m34 + matrix1._m44 * matrix2._m44);
        }

        public static bool operator ==(Matrix4 matrix1, Matrix4 matrix2)
        {
            return matrix1.M11 == matrix2.M11
                && matrix1.M12 == matrix2.M12
                && matrix1.M13 == matrix2.M13
                && matrix1.M14 == matrix2.M14
                && matrix1.M21 == matrix2.M21
                && matrix1.M22 == matrix2.M22
                && matrix1.M23 == matrix2.M23
                && matrix1.M24 == matrix2.M24
                && matrix1.M31 == matrix2.M31
                && matrix1.M32 == matrix2.M32
                && matrix1.M33 == matrix2.M33
                && matrix1.M34 == matrix2.M34
                && matrix1.M41 == matrix2.M41
                && matrix1.M42 == matrix2.M42
                && matrix1.M43 == matrix2.M43
                && matrix1.M44 == matrix2.M44;
        }

        public static bool operator !=(Matrix4 matrix1, Matrix4 matrix2)
        {
            return !(matrix1 == matrix2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix4)
            {
                return this == (Matrix4)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return M11.GetHashCode()
                ^ M12.GetHashCode()
                ^ M13.GetHashCode()
                ^ M14.GetHashCode()
                ^ M21.GetHashCode()
                ^ M22.GetHashCode()
                ^ M23.GetHashCode()
                ^ M24.GetHashCode()
                ^ M31.GetHashCode()
                ^ M32.GetHashCode()
                ^ M33.GetHashCode()
                ^ M34.GetHashCode()
                ^ M41.GetHashCode()
                ^ M42.GetHashCode()
                ^ M43.GetHashCode()
                ^ M44.GetHashCode();
        }

        public static Vector operator *(Matrix4 matrix, Vector vector)
        {
            var x = vector.X * matrix.M11 + vector.Y * matrix.M12 + vector.Z * matrix.M13 + matrix.M14;
            var y = vector.X * matrix.M21 + vector.Y * matrix.M22 + vector.Z * matrix.M23 + matrix.M24;
            var z = vector.X * matrix.M31 + vector.Y * matrix.M32 + vector.Z * matrix.M33 + matrix.M34;
            var w = matrix.M41 + matrix.M42 + matrix.M43 + matrix.M44;
            return new Vector(x / w, y / w, z / w);
        }

        public static Point operator *(Matrix4 matrix, Point point)
        {
            return matrix * (Vector)point;
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

        public bool IsIdentity
        {
            get
            {
                return _m11 == 1.0 && _m12 == 0.0 && _m13 == 0.0 && _m14 == 0.0
                    && _m21 == 0.0 && _m22 == 1.0 && _m23 == 0.0 && _m24 == 0.0
                    && _m31 == 0.0 && _m32 == 0.0 && _m33 == 1.0 && _m34 == 0.0
                    && _m41 == 0.0 && _m42 == 0.0 && _m43 == 0.0 && _m44 == 1.0;
            }
        }

        public static Matrix4 CreateTranslate(Vector vector)
        {
            return CreateTranslate(vector.X, vector.Y, vector.Z);
        }

        public Matrix4 CreateTranslate(Point point)
        {
            return CreateTranslate(point.X, point.Y, point.Z);
        }

        public static Matrix4 CreateTranslate(double x, double y, double z)
        {
            var matrix = Identity;
            matrix.M14 = x;
            matrix.M24 = y;
            matrix.M34 = z;
            return matrix;
        }

        public static Matrix4 CreateScale(Vector vector)
        {
            return CreateScale(vector.X, vector.Y, vector.Z);
        }

        public static Matrix4 CreateScale(double xs, double ys, double zs)
        {
            var matrix = Identity;
            matrix.M11 = xs;
            matrix.M22 = ys;
            matrix.M33 = zs;
            return matrix;
        }

        public static Matrix4 CreateScale(double scale)
        {
            return CreateScale(scale, scale, scale);
        }

        public Vector Transform(Vector vector)
        {
            return this * vector;
        }

        public Point Transform(Point point)
        {
            return this * point;
        }

        public Matrix4 Scale(Vector scale)
        {
            return CreateScale(scale) * this;
        }

        public void Invert()
        {
            // compute all six 2x2 determinants of 2nd two columns
            double y01 = _m13 * _m24 - _m23 * _m14;
            double y02 = _m13 * _m34 - _m33 * _m14;
            double y03 = _m13 * _m44 - _m43 * _m14;
            double y12 = _m23 * _m34 - _m33 * _m24;
            double y13 = _m23 * _m44 - _m43 * _m24;
            double y23 = _m33 * _m44 - _m43 * _m34;

            // Compute 3x3 cofactors for 1st the column
            double z30 = _m22 * y02 - _m32 * y01 - _m12 * y12;
            double z20 = _m12 * y13 - _m22 * y03 + _m42 * y01;
            double z10 = _m32 * y03 - _m42 * y02 - _m12 * y23;
            double z00 = _m22 * y23 - _m32 * y13 + _m42 * y12;

            // Compute 4x4 determinant
            double det = _m41 * z30 + _m31 * z20 + _m21 * z10 + _m11 * z00;

            if (det == 0.0)
            {
                //throw new InvalidOperationException("The matrix cannot be inverted");
                _m11 = double.NaN;
                _m12 = double.NaN;
                _m13 = double.NaN;
                _m14 = double.NaN;
                _m21 = double.NaN;
                _m22 = double.NaN;
                _m23 = double.NaN;
                _m24 = double.NaN;
                _m31 = double.NaN;
                _m32 = double.NaN;
                _m33 = double.NaN;
                _m34 = double.NaN;
                _m41 = double.NaN;
                _m42 = double.NaN;
                _m43 = double.NaN;
                _m44 = double.NaN;
                return;
            }

            // Compute 3x3 cofactors for the 2nd column
            double z31 = _m11 * y12 - _m21 * y02 + _m31 * y01;
            double z21 = _m21 * y03 - _m41 * y01 - _m11 * y13;
            double z11 = _m11 * y23 - _m31 * y03 + _m41 * y02;
            double z01 = _m31 * y13 - _m41 * y12 - _m21 * y23;

            // Compute all six 2x2 determinants of 1st two columns
            y01 = _m11 * _m22 - _m21 * _m12;
            y02 = _m11 * _m32 - _m31 * _m12;
            y03 = _m11 * _m42 - _m41 * _m12;
            y12 = _m21 * _m32 - _m31 * _m22;
            y13 = _m21 * _m42 - _m41 * _m22;
            y23 = _m31 * _m42 - _m41 * _m32;

            // Compute all 3x3 cofactors for 2nd two columns
            double z33 = _m13 * y12 - _m23 * y02 + _m33 * y01;
            double z23 = _m23 * y03 - _m43 * y01 - _m13 * y13;
            double z13 = _m13 * y23 - _m33 * y03 + _m43 * y02;
            double z03 = _m33 * y13 - _m43 * y12 - _m23 * y23;
            double z32 = _m24 * y02 - _m34 * y01 - _m14 * y12;
            double z22 = _m14 * y13 - _m24 * y03 + _m44 * y01;
            double z12 = _m34 * y03 - _m44 * y02 - _m14 * y23;
            double z02 = _m24 * y23 - _m34 * y13 + _m44 * y12;

            double rcp = 1.0 / det;

            // Multiply all 3x3 cofactors by reciprocal & transpose
            _m11 = z00 * rcp;
            _m12 = z10 * rcp;
            _m13 = z20 * rcp;
            _m14 = z30 * rcp;

            _m21 = z01 * rcp;
            _m22 = z11 * rcp;
            _m23 = z21 * rcp;
            _m24 = z31 * rcp;

            _m31 = z02 * rcp;
            _m32 = z12 * rcp;
            _m33 = z22 * rcp;
            _m34 = z32 * rcp;

            _m41 = z03 * rcp;
            _m42 = z13 * rcp;
            _m43 = z23 * rcp;
            _m44 = z33 * rcp;
        }

        public static Matrix4 FromUnitCircleProjection(Vector normal, Vector right, Vector up, Point center, double scaleX, double scaleY, double scaleZ)
        {
            var transformation = Identity;
            transformation.M11 = right.X;
            transformation.M12 = up.X; // right.y
            transformation.M13 = right.Z;
            transformation.M21 = right.Y; // up.x
            transformation.M22 = up.Y;
            transformation.M23 = up.Z;
            transformation.M31 = normal.X;
            transformation.M32 = normal.Y;
            transformation.M33 = normal.Z;
            transformation.M14 = center.X;
            transformation.M24 = center.Y;
            transformation.M34 = center.Z;
            var scale = Identity;
            scale.M11 = scaleX;
            scale.M22 = scaleY;
            scale.M33 = scaleZ;
            return transformation * scale;
        }

        internal void RotateAt(Quaternion quaternion, Point center)
        {
            var rotation = CreateRotationMatrix(quaternion, center);
            var result = this * rotation;
            _m11 = rotation._m11;
            _m12 = rotation._m12;
            _m13 = rotation._m13;
            _m14 = rotation._m14;
            _m21 = rotation._m21;
            _m22 = rotation._m22;
            _m23 = rotation._m23;
            _m24 = rotation._m24;
            _m31 = rotation._m31;
            _m32 = rotation._m32;
            _m33 = rotation._m33;
            _m34 = rotation._m34;
            _m41 = rotation._m41;
            _m42 = rotation._m42;
            _m43 = rotation._m43;
            _m44 = rotation._m44;
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

            matrix._m11 = 1.0 - (yy + zz);
            matrix._m12 = xy + wz;
            matrix._m13 = xz - wy;
            matrix._m21 = xy - wz;
            matrix._m22 = 1.0 - (xx + zz);
            matrix._m23 = yz + wx;
            matrix._m31 = xz + wy;
            matrix._m32 = yz - wx;
            matrix._m33 = 1.0 - (xx + yy);

            if (center.X != 0 || center.Y != 0 || center.Z != 0)
            {
                matrix._m41 = -center.X * matrix._m11 - center.Y * matrix._m21 - center.Z * matrix._m31 + center.X;
                matrix._m42 = -center.X * matrix._m12 - center.Y * matrix._m22 - center.Z * matrix._m32 + center.Y;
                matrix._m43 = -center.X * matrix._m13 - center.Y * matrix._m23 - center.Z * matrix._m33 + center.Z;
            }

            return matrix;
        }

        public static Matrix4 RotateAboutZ(double angleInDegrees)
        {
            var theta = angleInDegrees * MathHelper.DegreesToRadians;
            var cos = Math.Cos(theta);
            var sin = Math.Sin(theta);
            var m = Identity;
            m.M11 = cos;
            m.M12 = sin;
            m.M21 = -sin;
            m.M22 = cos;
            return m;
        }
    }
}
