// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad
{
    public class SelectionRectangle
    {
        public Point TopLeftScreen { get; private set; }
        public Point BottomRightScreen { get; private set; }
        public Point TopLeftWorld { get; private set; }
        public Point BottomRightWorld { get; private set; }

        public SelectionRectangle(Point topLeftScreen, Point bottomRightScreen, Point topLeftWorld, Point bottomRightWorld)
        {
            TopLeftScreen = topLeftScreen;
            BottomRightScreen = bottomRightScreen;
            TopLeftWorld = topLeftWorld;
            BottomRightWorld = bottomRightWorld;
        }
    }
}
