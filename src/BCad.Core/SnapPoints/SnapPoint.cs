// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
