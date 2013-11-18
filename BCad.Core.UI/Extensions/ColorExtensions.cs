using System.Drawing;
using Media = System.Windows.Media;

namespace BCad.Core.UI.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToDrawingColor(this IndexedColor color)
        {
            return Color.FromArgb((int)(0xFF000000 | color.GetRgbValue()));
        }

        public static RealColor ToRealColor(this Media.Color color)
        {
            return RealColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
