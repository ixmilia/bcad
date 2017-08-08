// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.Entities
{
    public class ProjectedArc : ProjectedEntity
    {
        public override EntityKind Kind
        {
            get { return EntityKind.Arc; }
        }

        public Arc OriginalArc { get; private set; }

        public Point Center { get; private set; }

        public double RadiusX { get; private set; }

        public double RadiusY { get; private set; }

        public double Rotation { get; private set; }

        public double StartAngle { get; private set; }

        public double EndAngle { get; private set; }

        public Point StartPoint { get; private set; }

        public Point EndPoint { get; private set; }

        public ProjectedArc(Arc arc, Layer layer, Point center, double radiusX, double radiusY, double rotation, double startAngle, double endAngle, Point startPoint, Point endPoint)
            : base(layer)
        {
            // copy values
            OriginalArc = arc;
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
            Rotation = rotation;
            StartAngle = startAngle;
            EndAngle = endAngle;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}
