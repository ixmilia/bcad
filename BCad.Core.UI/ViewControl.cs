using System.Windows.Controls;

namespace BCad.UI
{
    public abstract class ViewControl : UserControl, IViewControl
    {
        public abstract Point GetCursorPoint();
    }
}
