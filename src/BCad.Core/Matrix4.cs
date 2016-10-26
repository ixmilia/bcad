// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using BCad.Helpers;

namespace BCad
{
    public struct Matrix4
    {
        /// <summary>
        /// First row, first column.
        /// </summary>
        public double M11 { get; }

        /// <summary>
        /// First row, second column.
        /// </summary>
        public double M12 { get; }

        /// <summary>
        /// First row, third column.
        /// </summary>
        public double M13 { get; }
        
        /// <summary>
        /// First row, fourth column.
        /// </summary>
        public double M14 { get; }


        /// <summary>
        /// Second row, first column.
        /// </summary>
        public double M21 { get; }

        /// <summary>
        /// Second row, second column.
        /// </summary>
        public double M22 { get; }

        /// <summary>
        /// Second row, third column.
        /// </summary>
        public double M23 { get; }

        /// <summary>
        /// Second row, fourth column.
        /// </summary>
        public double M24 { get; }


        /// <summary>
        /// Third row, first column.
        /// </summary>
        public double M31 { get; }

        /// <summary>
        /// Third row, second column.
        /// </summary>
        public double M32 { get; }

        /// <summary>
        /// Third row, third column.
        /// </summary>
        public double M33 { get; }

        /// <summary>
        /// Third row, fourth column.
        /// </summary>
        public double M34 { get; }


        /// <summary>
        /// Fourth row, first column.
        /// </summary>
        public double M41 { get; }

        /// <summary>
        /// Fourth row, second column.
        /// </summary>
        public double M42 { get; }

        /// <summary>
        /// Fourth row, third column.
        /// </summary>
        public double M43 { get; }

        /// <summary>
        /// Fourth row, fourth column.
        /// </summary>
        public double M44 { get; }

        static Matrix4()
        {
            Identity = new Matrix4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
        }

        public Matrix4(double m11, double m12, double m13, double m14, double m21, double m22, double m23, double m24, double m31, double m32, double m33, double m34, double m41, double m42, double m43, double m44)
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
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public static Matrix4 operator *(Matrix4 matrix1, Matrix4 matrix2)
        {
            return new Matrix4(
                matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 +
                matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41,
                matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 +
                matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42,
                matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 +
                matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43,
                matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 +
                matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44,
                matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 +
                matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41,
                matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 +
                matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42,
                matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 +
                matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43,
                matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 +
                matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44,
                matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 +
                matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41,
                matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 +
                matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42,
                matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 +
                matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43,
                matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 +
                matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44,
                matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 +
                matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41,
                matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 +
                matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42,
                matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 +
                matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43,
                matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 +
                matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44);
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

        public static Matrix4 Identity { get; private set; }

        public bool IsIdentity
        {
            get
            {
                return M11 == 1.0 && M12 == 0.0 && M13 == 0.0 && M14 == 0.0
                    && M21 == 0.0 && M22 == 1.0 && M23 == 0.0 && M24 == 0.0
                    && M31 == 0.0 && M32 == 0.0 && M33 == 1.0 && M34 == 0.0
                    && M41 == 0.0 && M42 == 0.0 && M43 == 0.0 && M44 == 1.0;
            }
        }

        public static Matrix4 CreateTranslate(Vector vector)
        {
            return CreateTranslate(vector.X, vector.Y, vector.Z);
        }

        public static Matrix4 CreateTranslate(double x, double y, double z)
        {
            return new Matrix4(
                1.0, 0.0, 0.0, x,
                0.0, 1.0, 0.0, y,
                0.0, 0.0, 1.0, z,
                0.0, 0.0, 0.0, 1.0);
        }

        public static Matrix4 CreateScale(Vector vector)
        {
            return CreateScale(vector.X, vector.Y, vector.Z);
        }

        public static Matrix4 CreateScale(double xs, double ys, double zs)
        {
            return new Matrix4(
                xs, 0.0, 0.0, 0.0,
                0.0, ys, 0.0, 0.0,
                0.0, 0.0, zs, 0.0,
                0.0, 0.0, 0.0, 1.0);
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

        public Matrix4 Inverse()
        {
            // compute all six 2x2 determinants of 2nd two columns
            double y01 = M13 * M24 - M23 * M14;
            double y02 = M13 * M34 - M33 * M14;
            double y03 = M13 * M44 - M43 * M14;
            double y12 = M23 * M34 - M33 * M24;
            double y13 = M23 * M44 - M43 * M24;
            double y23 = M33 * M44 - M43 * M34;

            // Compute 3x3 cofactors for 1st the column
            double z30 = M22 * y02 - M32 * y01 - M12 * y12;
            double z20 = M12 * y13 - M22 * y03 + M42 * y01;
            double z10 = M32 * y03 - M42 * y02 - M12 * y23;
            double z00 = M22 * y23 - M32 * y13 + M42 * y12;

            // Compute 4x4 determinant
            double det = M41 * z30 + M31 * z20 + M21 * z10 + M11 * z00;

            if (det == 0.0)
            {
                //throw new InvalidOperationException("The matrix cannot be inverted");
                return new Matrix4(
                    double.NaN, double.NaN, double.NaN, double.NaN,
                    double.NaN, double.NaN, double.NaN, double.NaN,
                    double.NaN, double.NaN, double.NaN, double.NaN,
                    double.NaN, double.NaN, double.NaN, double.NaN);
            }

            // Compute 3x3 cofactors for the 2nd column
            double z31 = M11 * y12 - M21 * y02 + M31 * y01;
            double z21 = M21 * y03 - M41 * y01 - M11 * y13;
            double z11 = M11 * y23 - M31 * y03 + M41 * y02;
            double z01 = M31 * y13 - M41 * y12 - M21 * y23;

            // Compute all six 2x2 determinants of 1st two columns
            y01 = M11 * M22 - M21 * M12;
            y02 = M11 * M32 - M31 * M12;
            y03 = M11 * M42 - M41 * M12;
            y12 = M21 * M32 - M31 * M22;
            y13 = M21 * M42 - M41 * M22;
            y23 = M31 * M42 - M41 * M32;

            // Compute all 3x3 cofactors for 2nd two columns
            double z33 = M13 * y12 - M23 * y02 + M33 * y01;
            double z23 = M23 * y03 - M43 * y01 - M13 * y13;
            double z13 = M13 * y23 - M33 * y03 + M43 * y02;
            double z03 = M33 * y13 - M43 * y12 - M23 * y23;
            double z32 = M24 * y02 - M34 * y01 - M14 * y12;
            double z22 = M14 * y13 - M24 * y03 + M44 * y01;
            double z12 = M34 * y03 - M44 * y02 - M14 * y23;
            double z02 = M24 * y23 - M34 * y13 + M44 * y12;

            double rcp = 1.0 / det;

            // Multiply all 3x3 cofactors by reciprocal & transpose
            var m11 = z00 * rcp;
            var m12 = z10 * rcp;
            var m13 = z20 * rcp;
            var m14 = z30 * rcp;

            var m21 = z01 * rcp;
            var m22 = z11 * rcp;
            var m23 = z21 * rcp;
            var m24 = z31 * rcp;

            var m31 = z02 * rcp;
            var m32 = z12 * rcp;
            var m33 = z22 * rcp;
            var m34 = z32 * rcp;

            var m41 = z03 * rcp;
            var m42 = z13 * rcp;
            var m43 = z23 * rcp;
            var m44 = z33 * rcp;

            return new Matrix4(
                m11, m12, m13, m14,
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44);
        }

        public static Matrix4 FromUnitCircleProjection(Vector normal, Vector right, Vector up, Point center, double scaleX, double scaleY, double scaleZ)
        {
            var transformation = new Matrix4(
                right.X, up.X, right.Z, center.X,
                right.Y, up.Y, up.Z, center.Y,
                normal.X, normal.Y, normal.Z, center.Z,
                0.0, 0.0, 0.0, 1.0);
            var scale = CreateScale(scaleX, scaleY, scaleZ);
            return transformation * scale;
        }

        internal Matrix4 RotateAt(Quaternion quaternion, Point center)
        {
            var rotation = CreateRotationMatrix(quaternion, center);
            var result = this * rotation;
            var m11 = rotation.M11;
            var m12 = rotation.M12;
            var m13 = rotation.M13;
            var m14 = rotation.M14;
            var m21 = rotation.M21;
            var m22 = rotation.M22;
            var m23 = rotation.M23;
            var m24 = rotation.M24;
            var m31 = rotation.M31;
            var m32 = rotation.M32;
            var m33 = rotation.M33;
            var m34 = rotation.M34;
            var m41 = rotation.M41;
            var m42 = rotation.M42;
            var m43 = rotation.M43;
            var m44 = rotation.M44;

            return new Matrix4(
                m11, m12, m13, m14,
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44);
        }

        internal static Matrix4 CreateRotationMatrix(Quaternion quaternion, Point center)
        {
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

            var m11 = 1.0 - (yy + zz);
            var m12 = xy + wz;
            var m13 = xz - wy;
            var m21 = xy - wz;
            var m22 = 1.0 - (xx + zz);
            var m23 = yz + wx;
            var m31 = xz + wy;
            var m32 = yz - wx;
            var m33 = 1.0 - (xx + yy);
            var m41 = 0.0;
            var m42 = 0.0;
            var m43 = 0.0;

            if (center.X != 0 || center.Y != 0 || center.Z != 0)
            {
                m41 = -center.X * m11 - center.Y * m21 - center.Z * m31 + center.X;
                m42 = -center.X * m12 - center.Y * m22 - center.Z * m32 + center.Y;
                m43 = -center.X * m13 - center.Y * m23 - center.Z * m33 + center.Z;
            }

            return new Matrix4(
                m11, m12, m13, 0.0,
                m21, m22, m23, 0.0,
                m31, m32, m33, 0.0,
                m41, m42, m43, 1.0);
        }

        public static Matrix4 RotateAboutZ(double angleInDegrees)
        {
            var theta = angleInDegrees * MathHelper.DegreesToRadians;
            var cos = Math.Cos(theta);
            var sin = Math.Sin(theta);
            return new Matrix4(
                    cos, sin, 0.0, 0.0,
                    -sin, cos, 0.0, 0.0,
                    0.0, 0.0, 1.0, 0.0,
                    0.0, 0.0, 0.0, 1.0);
        }
    }
}
