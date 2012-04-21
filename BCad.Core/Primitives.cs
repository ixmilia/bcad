using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;

namespace BCad
{
    public class Point
    {
        private readonly Point3D point;

        public double X { get { return point.X; } }

        public double Y { get { return point.Y; } }

        public double Z { get { return point.Z; } }

        public Point(double x, double y, double z)
        {
            this.point = new Point3D(x, y, z);
        }

        public Point(System.Windows.Point point)
        {
            this.point = new Point3D(point.X, point.Y, 0.0);
        }

        public Point(Point3D point)
        {
            this.point = point;
        }

        public System.Windows.Point ToWindowsPoint()
        {
            return new System.Windows.Point(this.X, this.Y);
        }

        public Point3D ToPoint3D()
        {
            return point;
        }

        public Vector ToVector()
        {
            return new Vector(this.X, this.Y, this.Z);
        }

        public static Point Parse(string text)
        {
            var parts = text.Split(",".ToCharArray(), 3);
            return new Point(double.Parse(parts[0]), double.Parse(parts[1]), double.Parse(parts[2]));
        }

        public static readonly string NumberPattern = @"-?(\d+(\.\d*)?|\.\d+)";

        public static readonly Regex PointPattern = new Regex(string.Format("{0},{0},{0}", NumberPattern), RegexOptions.Compiled);

        public static bool TryParse(string text, out Point point)
        {
            bool success = false;
            point = default(Point);
            if (PointPattern.IsMatch(text))
            {
                point = Point.Parse(text);
                success = true;
            }
            return success;
        }

        public static bool operator ==(Point p1, Point p2)
        {
            if (object.ReferenceEquals(p1, p2))
                return true;
            if (((object)p1 == null) || ((object)p2 == null))
                return false;
            return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public static Vector operator +(Point p1, Point p2)
        {
            return new Vector(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Vector operator +(Point p1, Vector p2)
        {
            return new Vector(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Vector operator -(Point p1, Point p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Vector operator -(Point p1, Vector p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public bool Equals(Point p)
        {
            return this == p;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Point)
            {
                return this == (Point)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", X, Y, Z);
        }

        public static Point Origin
        {
            get { return new Point(0, 0, 0); }
        }
    }

    public class Vector
    {
        private readonly Vector3D vector;

        public double X { get { return vector.X; } }

        public double Y { get { return vector.Y; } }

        public double Z { get { return vector.Z; } }

        public double LengthSquared
        {
            get { return X * X + Y * Y + Z * Z; }
        }

        public double Length
        {
            get { return Math.Sqrt(LengthSquared); }
        }

        public Vector(double x, double y, double z)
        {
            this.vector = new Vector3D(x, y, z);
        }

        public Vector Normalize()
        {
            var v = new Vector3D(this.X, this.Y, this.Z);
            v.Normalize();
            return new Vector(v.X, v.Y, v.Z);
        }

        public Vector Cross(Vector v)
        {
            return new Vector(this.Y * v.Z - this.Z * v.Y, this.Z * v.X - this.X * v.Z, this.X * v.Y - this.Y * v.X);
        }

        public Point ToPoint()
        {
            return new Point(vector.X, vector.Y, vector.Z);
        }

        public Vector3D ToVector3D()
        {
            return vector;
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
            var angle = Math.Atan2(this.Y, this.X);
            if (double.IsNaN(angle))
                return double.NaN;

            angle = angle * 180.0 / Math.PI;
            // if > 0, quadrant 1 or 2
            // else quadrant 3 or 4
            return angle > 0 ? angle : 360.0 + angle;
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

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", X, Y, Z);
        }
    }

    public struct ValueOrDirective<T>
    {
        public string Directive { get; private set; }
        public T Value { get; private set; }
        public bool HasValue { get; private set; }
        public bool Cancel { get; private set; }

        public ValueOrDirective(string directive)
            : this()
        {
            Cancel = false;
            Directive = directive;
            HasValue = false;
        }

        public ValueOrDirective(T value)
            : this()
        {
            Cancel = false;
            Value = value;
            HasValue = true;
        }

        public static ValueOrDirective<T> GetCancel()
        {
            return new ValueOrDirective<T>() { Cancel = true };
        }
    }
}
