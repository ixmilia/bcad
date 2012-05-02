using System.Globalization;
using Media = System.Windows.Media;

namespace BCad
{
    internal static class ColorExtensions
    {
        public static Media.Color ParseColor(this string s)
        {
            int c = int.Parse(s.Substring(1), NumberStyles.HexNumber);
            int r = (c & 0xFF0000) >> 16;
            int g = (c & 0x00FF00) >> 8;
            int b = (c & 0x0000FF);
            return Media.Color.FromRgb((byte)r, (byte)g, (byte)b);
        }

        public static string ToColorString(this Media.Color color)
        {
            int c = (color.R << 16) | (color.G << 8) | color.B;
            return string.Format("#{0:X}", c);
        }
    }
}
