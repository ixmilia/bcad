// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Drawing;

namespace IxMilia.BCad.Extensions
{
    public static class PointExtensions
    {
        public static PointF ToPointF(this Point p)
        {
            return new PointF((float)p.X, (float)p.Y);
        }
    }
}
