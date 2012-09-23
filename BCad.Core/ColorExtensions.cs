using System.Globalization;
using Media = System.Windows.Media;

namespace BCad
{
    public static class ColorExtensions
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
            var r = string.Format("{0:X2}", color.R);
            var g = string.Format("{0:X2}", color.G);
            var b = string.Format("{0:X2}", color.B);
            return string.Format("#{0}{1}{2}", r, g, b);
        }
    }
}
