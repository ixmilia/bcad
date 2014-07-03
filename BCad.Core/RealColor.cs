namespace BCad
{
    public struct RealColor
    {
        public readonly byte A;
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        private RealColor(byte a, byte r, byte g, byte b)
            : this()
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public int ToInt()
        {
            return (A << 24) | (R << 16) | (G << 8) | B;
        }

        public static RealColor FromRgb(byte r, byte g, byte b)
        {
            return new RealColor(255, r, g, b);
        }

        public static RealColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new RealColor(a, r, g, b);
        }

        public static RealColor FromInt(int color)
        {
            byte a = (byte)0xFF;
            byte r = (byte)((color & 0xFF0000) >> 16);
            byte g = (byte)((color & 0x00FF00) >> 8);
            byte b = (byte)(color & 0x0000FF);
            return FromArgb(a, r, g, b);
        }

        public static bool operator ==(RealColor a, RealColor b)
        {
            return a.A == b.A && a.R == b.R && a.G == b.G && a.B == b.B;
        }

        public static bool operator !=(RealColor a, RealColor b)
        {
            return a.A != b.A || a.R != b.R || a.G != b.G || a.B != b.B;
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
            return brightness < 0.67 * 255 ? White : Black;
        }

        public static RealColor Black
        {
            get { return FromRgb(0, 0, 0); }
        }

        public static RealColor Blue
        {
            get { return FromRgb(0, 0, 255); }
        }

        public static RealColor CornflowerBlue
        {
            get { return FromRgb(0x64, 0x95, 0xED); }
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
