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

        public System.Drawing.Point ToDrawingPoint()
        {
            return new System.Drawing.Point((int)this.X, (int)this.Y);
        }

        public Point3D ToPoint3D()
        {
            return point;
        }

        public static implicit operator Vector(Point point)
        {
            return new Vector(point.X, point.Y, point.Z);
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

        public static Point operator +(Point p1, Vector p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Vector operator -(Point p1, Vector p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Point operator *(Point p, double scalar)
        {
            return new Point(p.X * scalar, p.Y * scalar, p.Z * scalar);
        }

        public static Point operator /(Point p, double scalar)
        {
            return new Point(p.X / scalar, p.Y / scalar, p.Z / scalar);
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
}
