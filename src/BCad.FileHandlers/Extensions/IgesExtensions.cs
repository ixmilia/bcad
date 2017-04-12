// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using IxMilia.Iges;
using IxMilia.Iges.Entities;

namespace BCad.FileHandlers.Extensions
{
    public static class IgesExtensions
    {
        public static IgesEntity ToIgesEntity(this Entity entity)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    return ((Line)entity).ToIgesLine();
                case EntityKind.Location:
                    return ((Location)entity).ToIgesLocation();
                case EntityKind.Arc:
                case EntityKind.Circle:
                    return entity.ToIgesCircle();
                default:
                    //Debug.Assert(false, "Unsupported entity type: " + entity.Kind);
                    return null;
            }
        }

        public static IgesLine ToIgesLine(this Line line)
        {
            var result = new IgesLine()
            {
                Bounding = IgesLineBounding.BoundOnBothSides,
                P1 = ToIgesPoint(line.P1),
                P2 = ToIgesPoint(line.P2)
            };
            AssignColor(result, line.Color);
            return result;
        }

        public static IgesLocation ToIgesLocation(this Location location)
        {
            var result = new IgesLocation()
            {
                X = location.Point.X,
                Y = location.Point.Y,
                Z = location.Point.Z
            };
            AssignColor(result, location.Color);
            return result;
        }

        public static IgesCircularArc ToIgesCircle(this Entity entity)
        {
            Point center;
            double startAngle, endAngle;
            CadColor? color;
            switch (entity.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)entity;
                    center = arc.Center;
                    startAngle = arc.StartAngle;
                    endAngle = arc.EndAngle;
                    color = arc.Color;
                    break;
                case EntityKind.Circle:
                    var circle = (Circle)entity;
                    center = circle.Center;
                    startAngle = 0.0;
                    endAngle = 360.0;
                    color = circle.Color;
                    break;
                default:
                    throw new ArgumentException();
            }

            startAngle *= MathHelper.DegreesToRadians;
            endAngle *= MathHelper.DegreesToRadians;

            // TODO: if normal isn't z-axis, create a transformation matrix
            var fromUnit = entity.GetUnitCircleProjection();
            var startPoint = fromUnit.Transform(new Point(Math.Cos(startAngle), Math.Sin(startAngle), 0.0));
            var endPoint = fromUnit.Transform(new Point(Math.Cos(endAngle), Math.Sin(endAngle), 0.0));
            var result = new IgesCircularArc()
            {
                PlaneDisplacement = center.Z,
                Center = new IgesPoint(center.X, center.Y, 0),
                StartPoint = new IgesPoint(startPoint.X, startPoint.Y, 0),
                EndPoint = new IgesPoint(endPoint.X, endPoint.Y, 0)
            };
            AssignColor(result, color);
            return result;
        }

        public static void AssignColor(this IgesEntity entity, CadColor? color)
        {
            entity.Color = color.ToIgesColor();
            if (entity.Color == IgesColorNumber.Custom)
            {
                Debug.Assert(color != null);
                entity.CustomColor = new IgesColorDefinition(color.GetValueOrDefault().R / 255.0, color.GetValueOrDefault().G / 255.0, color.GetValueOrDefault().B / 255.0);
            }
        }

        public static IgesPoint ToIgesPoint(this Point point)
        {
            return new IgesPoint(point.X, point.Y, point.Z);
        }

        public static IgesUnits ToIgesUnits(this UnitFormat unitFormat)
        {
            switch (unitFormat)
            {
                case UnitFormat.Architectural:
                    return IgesUnits.Inches;
                case UnitFormat.Metric:
                    return IgesUnits.Millimeters;
                default:
                    throw new Exception("Unsupported unit type: " + unitFormat);
            }
        }

        public static Entity ToEntity(this IgesEntity entity)
        {
            Entity result = null;
            switch (entity.EntityType)
            {
                case IgesEntityType.CircularArc:
                    result = ToArc((IgesCircularArc)entity);
                    break;
                case IgesEntityType.Line:
                    result = ToLine((IgesLine)entity);
                    break;
                case IgesEntityType.Point:
                    result = ToLocation((IgesLocation)entity);
                    break;
            }

            return result;
        }

        public static Line ToLine(this IgesLine line)
        {
            // TODO: handle different forms (segment, ray, continuous)
            return new Line(TransformPoint(line, line.P1), TransformPoint(line, line.P2), GetColor(line), line);
        }

        public static Location ToLocation(this IgesLocation point)
        {
            return new Location(new Point(point.X, point.Y, point.Z), GetColor(point), point);
        }

        public static Entity ToArc(this IgesCircularArc arc)
        {
            var center = TransformPoint(arc, arc.ProperCenter);
            var startPoint = TransformPoint(arc, arc.ProperStartPoint);
            var endPoint = TransformPoint(arc, arc.ProperEndPoint);

            // all points have the same Z-value, so the normal will be the transformed Z-axis vector
            var igesNormal = TransformPoint(arc, IgesVector.ZAxis);
            var normal = new Vector(igesNormal.X, igesNormal.Y, igesNormal.Z).Normalize();

            // find radius from start/end points
            var startVector = startPoint - center;
            var endVector = endPoint - center;
            var startRadius = startVector.Length;
            var endRadius = endVector.Length;
            // these should be very close, if not identical, but not necessarily
            var radius = (startRadius + endRadius) / 2;

            // if start/end points are the same, it's a circle.  otherwise it's an arc
            if (startPoint.CloseTo(endPoint))
            {
                return new Circle(center, radius, normal, GetColor(arc), arc);
            }
            else
            {
                // project back to unit circle to find start/end angles
                var primitiveCircle = new PrimitiveEllipse(center, radius, normal);
                var fromUnit = primitiveCircle.FromUnitCircle;
                Debug.Assert(AreAllValuesValid(fromUnit));
                var toUnit = fromUnit.Inverse();
                Debug.Assert(AreAllValuesValid(toUnit));
                var startUnit = toUnit.Transform(startPoint);
                var endUnit = toUnit.Transform(endPoint);
                var startAngle = ((Vector)startUnit).ToAngle();
                var endAngle = ((Vector)endUnit).ToAngle();
                return new Arc(center, radius, startAngle, endAngle, normal, GetColor(arc), arc);
            }
        }

        public static bool AreAllValuesValid(Matrix4 matrix)
        {
            return !double.IsNaN(matrix.M11)
                && !double.IsNaN(matrix.M12)
                && !double.IsNaN(matrix.M13)
                && !double.IsNaN(matrix.M14)
                && !double.IsNaN(matrix.M21)
                && !double.IsNaN(matrix.M22)
                && !double.IsNaN(matrix.M23)
                && !double.IsNaN(matrix.M24)
                && !double.IsNaN(matrix.M31)
                && !double.IsNaN(matrix.M32)
                && !double.IsNaN(matrix.M33)
                && !double.IsNaN(matrix.M34)
                && !double.IsNaN(matrix.M41)
                && !double.IsNaN(matrix.M42)
                && !double.IsNaN(matrix.M43)
                && !double.IsNaN(matrix.M44);
        }

        public static CadColor? GetColor(this IgesEntity entity)
        {
            switch (entity.Color)
            {
                case IgesColorNumber.Default:
                    return null;
                case IgesColorNumber.Black:
                    return CadColor.Black;
                case IgesColorNumber.Red:
                    return CadColor.Red;
                case IgesColorNumber.Green:
                    return CadColor.Green;
                case IgesColorNumber.Blue:
                    return CadColor.Blue;
                case IgesColorNumber.Yellow:
                    return CadColor.Yellow;
                case IgesColorNumber.Magenta:
                    return CadColor.Magenta;
                case IgesColorNumber.Cyan:
                    return CadColor.Cyan;
                case IgesColorNumber.White:
                    return CadColor.Cyan;
                default:
                    Debug.Assert(entity.CustomColor != null);
                    var custom = entity.CustomColor;
                    return CadColor.FromArgb(
                        255,
                        (byte)(custom.RedIntensity * 255),
                        (byte)(custom.GreenIntensity * 255),
                        (byte)(custom.BlueIntensity * 255));
            }
        }

        public static IgesColorNumber ToIgesColor(this CadColor? color)
        {
            if (color == null)
            {
                return IgesColorNumber.Default;
            }
            else
            {
                switch (color.GetValueOrDefault().ToInt32() & 0xFFFFFF)
                {
                    case 0x000000:
                        return IgesColorNumber.Black;
                    case 0xFF0000:
                        return IgesColorNumber.Red;
                    case 0xFFFF00:
                        return IgesColorNumber.Yellow;
                    case 0x00FF00:
                        return IgesColorNumber.Green;
                    case 0x00FFFF:
                        return IgesColorNumber.Cyan;
                    case 0x0000FF:
                        return IgesColorNumber.Blue;
                    case 0xFF00FF:
                        return IgesColorNumber.Magenta;
                    case 0xFFFFFF:
                        return IgesColorNumber.White;
                    default:
                        return IgesColorNumber.Custom;
                }
            }
        }

        public static Point TransformPoint(this IgesEntity entity, IgesPoint point)
        {
            var transformed = entity.TransformationMatrix.Transform(point);
            return new Point(transformed.X, transformed.Y, transformed.Z);
        }
    }
}
