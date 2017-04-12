// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Arc : Entity
    {
        private readonly PrimitiveEllipse _primitive;
        private readonly IPrimitive[] _primitives;
        private readonly SnapPoint[] _snapPoints;

        public Point Center => _primitive.Center;

        public Vector Normal => _primitive.Normal;

        public double Radius { get; }

        public double StartAngle => _primitive.StartAngle;

        public double EndAngle => _primitive.EndAngle;

        public Point EndPoint1 { get; }

        public Point EndPoint2 { get; }

        public Point MidPoint { get; }

        public Matrix4 FromUnitCircle => _primitive.FromUnitCircle;

        public double Thickness => _primitive.Thickness;

        public override EntityKind Kind => EntityKind.Arc;

        public override BoundingBox BoundingBox { get; }

        public Arc(Point center, double radius, double startAngle, double endAngle, Vector normal, CadColor? color = null, object tag = null, double thickness = default(double))
            : this(new PrimitiveEllipse(center, radius, startAngle, endAngle, normal, color, thickness), tag)
        {
        }

        public Arc(PrimitiveEllipse arc, object tag = null)
            : base(arc.Color, tag)
        {
            if (arc.MinorAxisRatio != 1.0)
            {
                throw new ArgumentException($"{nameof(PrimitiveEllipse)} is not circular");
            }

            _primitive = arc;
            _primitives = new[] { _primitive };
            Radius = arc.MajorAxis.Length;

            var right = Vector.RightVectorFromNormal(Normal);
            var midAngle = (StartAngle + EndAngle) / 2.0;
            if (StartAngle > EndAngle)
                midAngle -= 180.0;
            var points = Circle.TransformedPoints(Center, Normal, right, Radius, Radius, StartAngle, EndAngle, midAngle);
            EndPoint1 = points[0];
            EndPoint2 = points[1];
            MidPoint = points[2];

            _snapPoints = new SnapPoint[]
            {
                new CenterPoint(Center),
                new EndPoint(EndPoint1),
                new EndPoint(EndPoint2),
                new MidPoint(MidPoint)
            };
            BoundingBox = BoundingBox.FromPoints(Circle.TransformedPoints(Center, Normal, right, Radius, Radius, 0, 90, 180, 270));
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return _primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Center):
                    return Center;
                case nameof(Normal):
                    return Normal;
                case nameof(Radius):
                    return Radius;
                case nameof(StartAngle):
                    return StartAngle;
                case nameof(EndAngle):
                    return EndAngle;
                case nameof(Thickness):
                    return Thickness;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public Arc Update(
            Optional<Point> center = default(Optional<Point>),
            Optional<double> radius = default(Optional<double>),
            Optional<double> startAngle = default(Optional<double>),
            Optional<double> endAngle = default(Optional<double>),
            Optional<Vector> normal = default(Optional<Vector>),
            Optional<double> thickness = default(Optional<double>),
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newCenter = center.HasValue ? center.Value : Center;
            var newRadius = radius.HasValue ? radius.Value : Radius;
            var newStartAngle = startAngle.HasValue ? startAngle.Value : StartAngle;
            var newEndAngle = endAngle.HasValue ? endAngle.Value : EndAngle;
            var newNormal = normal.HasValue ? normal.Value : Normal;
            var newThickness = thickness.HasValue ? thickness.Value : Thickness;
            var newColor = color.HasValue ? color.Value : Color;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newCenter == Center &&
                newRadius == Radius &&
                newStartAngle == StartAngle &&
                newEndAngle == EndAngle &&
                newNormal == Normal &&
                newThickness == Thickness &&
                newColor == Color &&
                newTag == Tag)
            {
                return this;
            }

            return new Arc(newCenter, newRadius, newStartAngle, newEndAngle, newNormal, newColor, newTag, newThickness);
        }

        public override string ToString()
        {
            return string.Format("Arc: center={0}, normal={1}, radius={2}, start/end={3}/{4}, color={5}", Center, Normal, Radius, StartAngle, EndAngle, Color);
        }
    }
}
