using System.Windows.Media;

namespace BCad.Extensions
{
    public static class ColorExtensions
    {
        public static System.Drawing.Color ToDrawingColor(this CadColor color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
