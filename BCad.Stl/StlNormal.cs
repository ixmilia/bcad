using System;

namespace BCad.Stl
{
    public struct StlNormal
    {
        public float X;
        public float Y;
        public float Z;

        public StlNormal(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public StlNormal Normalize()
        {
            var length = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            if (length == 0.0f)
                return new StlNormal();
            return new StlNormal(X / length, Y / length, Z / length);
        }
    }
}
