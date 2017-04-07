// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using BCad.Collections;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using IxMilia.Iges;
using IxMilia.Iges.Entities;
using BCad.Primitives;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, true, FileExtension1, FileExtension2)]
    public class IgesFileHandler: IFileHandler
    {
        public const string DisplayName = "IGES Files (" + FileExtension1 + ", " + FileExtension2 + ")";
        public const string FileExtension1 = ".igs";
        public const string FileExtension2 = ".iges";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
        {
            var file = IgesFile.Load(fileStream);
            var layer = new Layer("igs", null);
            foreach (var entity in file.Entities)
            {
                var cadEntity = ToEntity(entity);
                if (cadEntity != null)
                {
                    layer = layer.Add(cadEntity);
                }
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, 8),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer),
                layer.Name,
                file.Author);
            drawing.Tag = file;

            viewPort = null; // auto-set it later

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort)
        {
            var file = new IgesFile();
            var oldFile = drawing.Tag as IgesFile;
            if (oldFile != null)
            {
                // preserve settings from original file
                file.TimeStamp = oldFile.TimeStamp;
            }

            file.Author = drawing.Author;
            file.FullFileName = fileName;
            file.Identification = Path.GetFileName(fileName);
            file.Identifier = Path.GetFileName(fileName);
            file.ModelUnits = ToIgesUnits(drawing.Settings.UnitFormat);
            file.ModifiedTime = DateTime.Now;
            file.SystemIdentifier = "BCad";
            file.SystemVersion = "1.0";
            foreach (var entity in drawing.GetEntities())
            {
                IgesEntity igesEntity = null;
                switch (entity.Kind)
                {
                    case EntityKind.Line:
                        igesEntity = ToIgesLine((Line)entity);
                        break;
                    case EntityKind.Location:
                        igesEntity = ToIgesLocation((Location)entity);
                        break;
                    case EntityKind.Arc:
                    case EntityKind.Circle:
                        igesEntity = ToIgesCircle(entity);
                        break;
                    default:
                        //Debug.Assert(false, "Unsupported entity type: " + entity.Kind);
                        break;
                }

                if (igesEntity != null)
                    file.Entities.Add(igesEntity);
            }

            file.Save(fileStream);
            return true;
        }

        private static IgesLine ToIgesLine(Line line)
        {
            return new IgesLine()
            {
                Bounding = IgesLineBounding.BoundOnBothSides,
                Color = ToColor(line.Color),
                P1 = ToIgesPoint(line.P1),
                P2 = ToIgesPoint(line.P2)
            };
        }

        private static IgesLocation ToIgesLocation(Location location)
        {
            return new IgesLocation()
            {
                X = location.Point.X,
                Y = location.Point.Y,
                Z = location.Point.Z,
                Color = ToColor(location.Color)
            };
        }

        private static IgesCircularArc ToIgesCircle(Entity entity)
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
            return new IgesCircularArc()
            {
                PlaneDisplacement = center.Z,
                Center = new IgesPoint(center.X, center.Y, 0),
                StartPoint = new IgesPoint(startPoint.X, startPoint.Y, 0),
                EndPoint = new IgesPoint(endPoint.X, endPoint.Y, 0),
                Color = ToColor(color)
            };
        }

        private static IgesPoint ToIgesPoint(Point point)
        {
            return new IgesPoint(point.X, point.Y, point.Z);
        }

        private static IgesUnits ToIgesUnits(UnitFormat unitFormat)
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

        private static Entity ToEntity(IgesEntity entity)
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

        private static Line ToLine(IgesLine line)
        {
            // TODO: handle different forms (segment, ray, continuous)
            return new Line(TransformPoint(line, line.P1), TransformPoint(line, line.P2), ToColor(line.Color), line);
        }

        private static Location ToLocation(IgesLocation point)
        {
            return new Location(new Point(point.X, point.Y, point.Z), ToColor(point.Color), point);
        }

        private static Entity ToArc(IgesCircularArc arc)
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
                return new Circle(center, radius, normal, ToColor(arc.Color), arc);
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
                return new Arc(center, radius, startAngle, endAngle, normal, ToColor(arc.Color), arc);
            }
        }

        private static bool AreAllValuesValid(Matrix4 matrix)
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

        private static CadColor? ToColor(IgesColorNumber color)
        {
            switch (color)
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
                    // TODO: handle custom colors
                    return null;
            }
        }

        private static IgesColorNumber ToColor(CadColor? color)
        {
            if (color == null)
            {
                return IgesColorNumber.Default;
            }
            else
            {
                switch (color.Value.ToInt32())
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
                        return IgesColorNumber.Default;
                }
            }
        }

        private static Point TransformPoint(IgesEntity entity, IgesPoint point)
        {
            var transformed = entity.TransformationMatrix.Transform(point);
            return new Point(transformed.X, transformed.Y, transformed.Z);
        }
    }
}
