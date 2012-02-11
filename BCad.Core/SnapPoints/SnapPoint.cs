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

        public abstract GeometryDrawing Icon { get; }

        private static ResourceDictionary _resources = null;

        protected static ResourceDictionary Resources
        {
            get
            {
                if (_resources == null)
                {
                    _resources = new ResourceDictionary();
                    _resources.Source = new Uri("/BCad.Core;component/SnapPoints/SnapPointIcons.xaml", UriKind.RelativeOrAbsolute);
                }
                return _resources;
            }
        }

        public SnapPoint(Point p)
        {
            Point = p;
        }
    }
}
