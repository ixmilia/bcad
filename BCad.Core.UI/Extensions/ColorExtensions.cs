using System.Drawing;
using Media = System.Windows.Media;

namespace BCad.Core.UI.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToDrawingColor(this RealColor color)
        {
            return Color.FromArgb((int)(0xFF000000 | (uint)color.ToInt()));
        }

        public static RealColor ToRealColor(this Media.Color color)
        {
            return RealColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
