using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using BCad.EventArguments;
using BCad.Objects;

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
        Point GetCursorPoint();
        Control RegisteredControl { get; set; }
        Point ControlToWorld(Point point);
        Point WorldToControl(Point point);
    }
}
