using SharpDX;

namespace BCad.Extensions
{
    public static class ColorExtensions
    {
        public static Color4 ToColor4(this RealColor color)
        {
            return new Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static Color ToColor(this RealColor color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
    }
}
