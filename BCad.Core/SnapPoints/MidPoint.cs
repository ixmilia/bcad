using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace BCad.SnapPoints
{
    public class MidPoint : SnapPoint
    {
        public MidPoint(Point p)
            : base(p)
        {
        }

        public override GeometryDrawing Icon
        {
            get
            {
                return (GeometryDrawing)SnapPoint.Resources["MidPointIcon"];
            }
        }
    }
}
