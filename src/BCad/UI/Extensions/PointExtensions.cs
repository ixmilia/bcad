// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using UIPoint = System.Windows.Point;

namespace IxMilia.BCad.UI.Extensions
{
    public static class PointExtensions
    {
        public static Point ToPoint(this UIPoint point)
        {
            return new Point(point.X, point.Y, 0.0);
        }
    }
}
