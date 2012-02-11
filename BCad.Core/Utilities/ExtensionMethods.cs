using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace BCad.Utilities
{
    public static class ExtensionMethods
    {
        public static System.Drawing.Point ToPoint(this Point3D vector)
        {
            return new System.Drawing.Point((int)vector.X, (int)vector.Y);
        }
    }
}
