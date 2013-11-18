using System.Windows.Media;

namespace BCad.UI.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToMediaColor(this RealColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
