// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Drawing;
using SharpDX;

namespace BCad.Extensions
{
    public static class PointExtensions
    {
        public static PointF ToPointF(this Point p)
        {
            return new PointF((float)p.X, (float)p.Y);
        }

        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }
    }
}
