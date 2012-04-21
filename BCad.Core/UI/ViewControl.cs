using System.Windows.Controls;

namespace BCad.UI
{
    public abstract class ViewControl : UserControl
    {
        public abstract Point GetCursorPoint();
    }
}
