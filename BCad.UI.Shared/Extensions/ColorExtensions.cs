#if WPF
using UIColor = System.Windows.Media.Color;
#endif

#if WINDOWS_UWP
using UIColor = Windows.UI.Color;
#endif

namespace BCad.UI.Shared.Extensions
{
    public static class ColorExtensions
    {
        public static UIColor ToUIColor(this CadColor color)
        {
            return UIColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
