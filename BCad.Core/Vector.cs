using System;
using System.Windows.Media.Media3D;
using BCad.Helpers;

namespace BCad
{
    public class Vector
    {
        public double X { get; private set; }

        public double Y { get; private set; }

        public double Z { get; private set; }

        public double LengthSquared
        {
            get { return X * X + Y * Y + Z * Z; }
        }

        public double Length
        {
            get { return Math.Sqrt(LengthSquared); }
        }

        public bool IsZeroVector
        {
            get { return this.X == 0.0 && this.Y == 0.0 && this.Z == 0.0; }
        }

        public Vector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector(Vector3D vector)
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }

        public Vector Normalize()
        {
            return this / this.Length;
        }

        public Vector Cross(Vector v)
        {
            return new Vector(this.Y * v.Z - this.Z * v.Y, this.Z * v.X - this.X * v.Z, this.X * v.Y - this.Y * v.X);
        }

        public double Dot(Vector v)
        {
            return this.X * v.X + this.Y * v.Y + this.Z * v.Z;
        }

        public static implicit operator Point(Vector vector)
        {
            return new Point(vector.X, vector.Y, vector.Z);
        }

        public static implicit operator Vector3D(Vector vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static implicit operator Vector(Vector3D vector)
        {
            return new Vector(vector.X, vector.Y, vector.Z);
        }

        public static Vector operator -(Vector vector)
        {
            return new Vector(-vector.X, -vector.Y, -vector.Z);
        }

        public static Vector operator *(Vector vector, double operand)
        {
            return new Vector(vector.X * operand, vector.Y * operand, vector.Z * operand);
        }

        public static Vector operator /(Vector vector, double operand)
        {
            return new Vector(vector.X / operand, vector.Y / operand, vector.Z / operand);
        }

        public static bool operator ==(Vector p1, Vector p2)
        {
            if (object.ReferenceEquals(p1, p2))
                return true;
            if (((object)p1 == null) || ((object)p2 == null))
                return false;
            return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;
        }

        public static bool operator !=(Vector p1, Vector p2)
        {
            return !(p1 == p2);
        }

        public bool Equals(Vector p)
        {
            return this == p;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Vector)
            {
                return this == (Vector)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        /// <summary>
        /// Returns the angle in the XY plane in degrees.
        /// </summary>
        public double ToAngle()
        {
            var angle = Math.Atan2(this.Y, this.X) * MathHelper.RadiansToDegrees;
            // if > 0, quadrant 1 or 2
            // else quadrant 3 or 4
            return angle.CorrectAngleDegrees();
        }

        public bool IsOrthoganalTo(Vector other)
        {
            return Math.Abs(this.Dot(other)) < MathHelper.Epsilon;
        }

        public bool IsParallelTo(Vector other)
        {
            return this.Cross(other).IsZeroVector;
        }

        public static Vector XAxis
        {
            get { return new Vector(1, 0, 0); }
        }

        public static Vector YAxis
        {
            get { return new Vector(0, 1, 0); }
        }

        public static Vector ZAxis
        {
            get { return new Vector(0, 0, 1); }
        }

        public static Vector Zero
        {
            get { return new Vector(0, 0, 0); }
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", X, Y, Z);
        }

        public static Vector RightVectorFromNormal(Vector normal)
        {
            if (normal == Vector.XAxis)
                return Vector.ZAxis;
            var right = Vector.XAxis;
            var up = normal.Cross(right);
            return up.Cross(normal).Normalize();
        }

        public static Vector NormalFromRightVector(Vector right)
        {
            // these two functions are identical, but the separate name makes them easier to understand
            return RightVectorFromNormal(right);
        }

        /// <summary>
        /// Returns the angle between the two vectors in degrees.
        /// </summary>
        public static double AngleBetween(Vector a, Vector b)
        {
            var dot = a.Dot(b);
            return Math.Acos(dot) * MathHelper.RadiansToDegrees;
        }
    }
}
