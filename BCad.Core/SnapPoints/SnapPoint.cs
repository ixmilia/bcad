using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BCad.SnapPoints
{
    public abstract class SnapPoint
    {
        public Point Point { get; protected set; }

        public abstract SnapPointKind Kind { get; }

        public SnapPoint(Point p)
        {
            Point = p;
        }
    }
}
