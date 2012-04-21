using System.Windows.Controls;
using BCad.EventArguments;
using BCad.UI;

namespace BCad
{
    public delegate void ViewPortChangedEvent(object sender, ViewPortChangedEventArgs e);

    public interface IView
    {
        event ViewPortChangedEvent ViewPortChanged;
        void UpdateView(Point viewPoint = null, Vector sight = null, Vector up = null,
            double? viewWidth = null, Point bottomLeft = null);
        Point ViewPoint { get; }
        Vector Sight { get; }
        Vector Up { get; }
        double ViewWidth { get; }
        Point BottomLeft { get; }
        ViewControl RegisteredControl { get; set; }
        Point ControlToWorld(Point point);
        Point WorldToControl(Point point);
    }
}
