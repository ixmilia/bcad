using System;
using System.Diagnostics;
using System.IO;
using BCad.Collections;
using BCad.Core;
using BCad.Entities;
using BCad.Extensions;
using BCad.FileHandlers.DrawingFiles;
using BCad.Helpers;
using BCad.Iges;
using BCad.Iges.Entities;
using BCad.Primitives;
using System.Collections.Generic;

namespace BCad.FileHandlers.Converters
{
    public class IgesConverter : IDrawingConverter
    {
        public bool ConvertToDrawing(string fileName, IDrawingFile drawingFile, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag)
        {
            if (drawingFile == null)
                throw new ArgumentNullException("drawingFile");
            var igesFile = drawingFile as IgesDrawingFile;
            if (igesFile == null)
                throw new ArgumentException("Drawing file was not an IGES file.");
            if (igesFile.File == null)
                throw new ArgumentException("Drawing file had no internal IGES file.");

            propertyBag = new Dictionary<string, object>()
            {
                { "ColorMap", ColorMap.IgesDefault }
            };

            var layer = new Layer("igs", IndexedColor.Auto);
            foreach (var entity in igesFile.File.Entities)
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
                igesFile.File.Author);
            drawing.Tag = igesFile.File;

            viewPort = null; // auto-set it later

            return true;
        }

        public bool ConvertFromDrawing(string fileName, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag, out IDrawingFile drawingFile)
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

            drawingFile = new IgesDrawingFile(file);
            return true;
        }

        private static IgesLine ToIgesLine(Line line)
        {
            return new IgesLine()
            {
                Bounding = IgesBounding.BoundOnBothSides,
                Color = ToColor(line.Color),
                P1 = ToIgesPoint(line.P1),
                P2 = ToIgesPoint(line.P2)
            };
        }

        private static IgesLocation ToIgesLocation(Location location)
        {
            return new IgesLocation()
            {
                Location = ToIgesPoint(location.Point),
                Color = ToColor(location.Color)
            };
        }

        private static IgesCircularArc ToIgesCircle(Entity entity)
        {
            Point center;
            double startAngle, endAngle;
            IndexedColor color;
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
                case IgesEntityType.SingularSubfigureInstance:
                    result = ToAggregate((IgesSingularSubfigureInstance)entity);
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
            return new Location(TransformPoint(point, point.Location), ToColor(point.Color), point);
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
                var toUnit = primitiveCircle.FromUnitCircleProjection();
                Debug.Assert(AreAllValuesValid(toUnit));
                toUnit.Invert();
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

        private static Entity ToAggregate(IgesSingularSubfigureInstance subfigure)
        {
            var sub = subfigure.Subfigure;
            if (sub != null)
            {
                var ag = sub as IgesSubfigureDefinition;
                if (ag != null)
                {
                    var entities = ReadOnlyList<Entity>.Empty();
                    foreach (var e in ag.Entities)
                    {
                        var a = ToEntity(e);
                        if (a != null)
                            entities = entities.Add(a);
                    }

                    if (entities.Count != 0)
                    {
                        var offset = new Point(subfigure.Offset.X, subfigure.Offset.Y, subfigure.Offset.Z);
                        return new AggregateEntity(offset, entities, ToColor(subfigure.Color), subfigure);
                    }
                }
            }

            return null;
        }

        private static IndexedColor ToColor(IgesColorNumber color)
        {
            switch (color)
            {
                case IgesColorNumber.Default:
                    return IndexedColor.Auto;
                case IgesColorNumber.Black:
                    return new IndexedColor(1);
                case IgesColorNumber.Red:
                    return new IndexedColor(2);
                case IgesColorNumber.Green:
                    return new IndexedColor(3);
                case IgesColorNumber.Blue:
                    return new IndexedColor(4);
                case IgesColorNumber.Yellow:
                    return new IndexedColor(5);
                case IgesColorNumber.Magenta:
                    return new IndexedColor(6);
                case IgesColorNumber.Cyan:
                    return new IndexedColor(7);
                case IgesColorNumber.White:
                    return new IndexedColor(8);
                default:
                    return IndexedColor.Auto;
            }
        }

        private static IgesColorNumber ToColor(IndexedColor color)
        {
            switch (color.Value)
            {
                case 0:
                    return IgesColorNumber.Black;
                case 1:
                    return IgesColorNumber.Red;
                case 2:
                    return IgesColorNumber.Yellow;
                case 3:
                    return IgesColorNumber.Green;
                case 4:
                    return IgesColorNumber.Cyan;
                case 5:
                    return IgesColorNumber.Blue;
                case 6:
                    return IgesColorNumber.Magenta;
                case 7:
                    return IgesColorNumber.White;
                default:
                    return IgesColorNumber.Default;
            }
        }

        private static Point TransformPoint(IgesEntity entity, IgesPoint point)
        {
            var transformed = entity.TransformationMatrix.Transform(point);
            return new Point(transformed.X, transformed.Y, transformed.Z);
        }
    }
}
