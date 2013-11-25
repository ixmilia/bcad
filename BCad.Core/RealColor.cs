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

        public RealColor GetAutoContrastingColor()
        {
            var brightness = 0.2126 * R + 0.7152 * G + 0.0722 * B;
            return brightness < 0.67 ? White : Black;
        }

        public static RealColor Black
        {
            get { return FromRgb(0, 0, 0); }
        }

        public static RealColor DarkSlateGray
        {
            get { return FromRgb(0x2F, 0x2F, 0x2F); }
        }

        public static RealColor Yellow
        {
            get { return FromRgb(255, 255, 0); }
        }

        public static RealColor White
        {
            get { return FromRgb(255, 255, 255); }
        }
    }
}
