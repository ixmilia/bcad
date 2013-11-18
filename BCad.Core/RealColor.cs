namespace BCad
{
    public struct RealColor
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        private RealColor(byte a, byte r, byte g, byte b)
            : this()
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public static RealColor FromRgb(byte r, byte g, byte b)
        {
            return new RealColor(255, r, g, b);
        }

        public static RealColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new RealColor(a, r, g, b);
        }

        public static bool operator ==(RealColor a, RealColor b)
        {
            return a.A == a.A && a.R == b.R && a.G == b.G && a.B == b.B;
        }

        public static bool operator !=(RealColor a, RealColor b)
        {
            return a.A != a.A || a.R != b.R || a.G != b.G || a.B != b.B;
        }

        public override bool Equals(object obj)
        {
            if (obj is RealColor)
            {
                var c = (RealColor)obj;
                return this == c;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }
    }
}
