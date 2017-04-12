// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Ellipse : Entity
    {
        private readonly PrimitiveEllipse _primitive;
        private readonly IPrimitive[] _primitives;
        private readonly SnapPoint[] _snapPoints;

        public Point Center => _primitive.Center;

        public Vector MajorAxis => _primitive.MajorAxis;

        public Vector Normal => _primitive.Normal;

        public double MinorAxisRatio => _primitive.MinorAxisRatio;

        public double StartAngle => _primitive.StartAngle;

        public double EndAngle => _primitive.EndAngle;

        public Matrix4 FromUnitCircle => _primitive.FromUnitCircle;

        public double Thickness => _primitive.Thickness;

        public override EntityKind Kind => EntityKind.Ellipse;

        public override BoundingBox BoundingBox { get; }

        public Ellipse(Point center, Vector majorAxis, double minorAxisRatio, double startAngle, double endAngle, Vector normal, CadColor? color = null, object tag = null, double thickness = default(double))
            : this(new PrimitiveEllipse(center, majorAxis, normal, minorAxisRatio, startAngle, endAngle, color, thickness), tag)
        {
        }

        public Ellipse(PrimitiveEllipse ellipse, object tag = null)
            : base(ellipse.Color, tag)
        {
            _primitive = ellipse;
            _primitives = new[] { _primitive };

            var majorLength = MajorAxis.Length;
            var points = Circle.TransformedPoints(Center, Normal, MajorAxis, majorLength, majorLength * MinorAxisRatio, 0, 90, 180, 270, StartAngle, EndAngle, (StartAngle + EndAngle) / 2.0);
            var quadrant1 = points[0];
            var quadrant2 = points[1];
            var quadrant3 = points[2];
            var quadrant4 = points[3];
            var endPoint1 = points[4];
            var endPoint2 = points[5];
            var midPoint = points[6];

            var snaps = new List<SnapPoint>();
            snaps.Add(new CenterPoint(Center));
            if (StartAngle == 0.0 && EndAngle == 360.0)
            {
                // treat it like a circle
                snaps.Add(new QuadrantPoint(quadrant1));
                snaps.Add(new QuadrantPoint(quadrant2));
                snaps.Add(new QuadrantPoint(quadrant3));
                snaps.Add(new QuadrantPoint(quadrant4));
            }
            else
            {
                // treat it like an arc
                snaps.Add(new EndPoint(endPoint1));
                snaps.Add(new EndPoint(endPoint2));
                snaps.Add(new MidPoint(midPoint));
            }

            if (MinorAxisRatio != 1.0)
            {
                // a true ellipse with two distinct foci
                var majorNormalized = MajorAxis.Normalize();
                var minorLength = majorLength * MinorAxisRatio;
                var focusDistance = Math.Sqrt((majorLength * majorLength) - (minorLength * minorLength));
                var focus1 = (Point)((majorNormalized * focusDistance) + Center);
                var focus2 = (Point)((majorNormalized * -focusDistance) + Center);
                snaps.Add(new FocusPoint(focus1));
                snaps.Add(new FocusPoint(focus2));
            }

            _snapPoints = snaps.ToArray();
            BoundingBox = BoundingBox.FromPoints(quadrant1, quadrant2, quadrant3, quadrant4);
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
                case nameof(MajorAxis):
                    return MajorAxis;
                case nameof(MinorAxisRatio):
                    return MinorAxisRatio;
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

        public Ellipse Update(
            Optional<Point> center = default(Optional<Point>),
            Optional<Vector> majorAxis = default(Optional<Vector>),
            Optional<double> minorAxisRatio = default(Optional<double>),
            Optional<double> startAngle = default(Optional<double>),
            Optional<double> endAngle = default(Optional<double>),
            Optional<Vector> normal = default(Optional<Vector>),
            Optional<double> thickness = default(Optional<double>),
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newCenter = center.HasValue ? center.Value : Center;
            var newMajorAxis = majorAxis.HasValue ? majorAxis.Value : MajorAxis;
            var newMinorAxisRatio = minorAxisRatio.HasValue ? minorAxisRatio.Value : MinorAxisRatio;
            var newStartAngle = startAngle.HasValue ? startAngle.Value : StartAngle;
            var newEndAngle = endAngle.HasValue ? endAngle.Value : EndAngle;
            var newNormal = normal.HasValue ? normal.Value : Normal;
            var newThickness = thickness.HasValue ? thickness.Value : Thickness;
            var newColor = color.HasValue ? color.Value : Color;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newCenter == Center &&
                newMajorAxis == MajorAxis &&
                newMinorAxisRatio == MinorAxisRatio &&
                newStartAngle == StartAngle &&
                newEndAngle == EndAngle &&
                newNormal == Normal &&
                newThickness == Thickness &&
                newColor == Color &&
                newTag == Tag)
            {
                return this;
            }

            return new Ellipse(newCenter, newMajorAxis, newMinorAxisRatio, newStartAngle, newEndAngle, newNormal, newColor, newTag, newThickness);
        }

        public override string ToString()
        {
            return string.Format("Ellipse: center={0}, major-axis={1}, normal={2}, minor={3}, start/end={4}/{5}, color={6}", Center, MajorAxis, Normal, MinorAxisRatio, StartAngle, EndAngle, Color);
        }
    }
}
