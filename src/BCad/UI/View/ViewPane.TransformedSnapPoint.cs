// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.UI.View
{
    public partial class ViewPane
    {
        private class TransformedSnapPoint
        {
            public Point WorldPoint;
            public Point ControlPoint;
            public SnapPointKind Kind;

            public TransformedSnapPoint(Point worldPoint, Point controlPoint, SnapPointKind kind)
            {
                WorldPoint = worldPoint;
                ControlPoint = controlPoint;
                Kind = kind;
            }
        }
    }
}
