using System.Drawing;

namespace BCad.Core.UI.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToDrawingColor(this IndexedColor color)
        {
            return Color.FromArgb((int)(0xFF000000 | color.GetRgbValue()));
        }
    }
}
