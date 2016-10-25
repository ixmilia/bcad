#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#elif WPF
using System.Windows.Controls;
#endif

namespace BCad.UI.Shared
{
    public class AbstractCadRenderer : UserControl
    {
        public virtual void Invalidate()
        {
        }

        public virtual void UpdateRubberBandLines()
        {
        }
    }
}
