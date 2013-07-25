using System;
using System.Diagnostics;
using System.IO;
using BCad.Collections;
using BCad.DrawingFiles;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Iges;
using BCad.Iges.Entities;
using BCad.Primitives;

namespace BCad.Converters
{
    public class IgesConverter : IDrawingConverter
    {
        public bool ConvertToDrawing(string fileName, IDrawingFile drawingFile, out Drawing drawing, out ViewPort viewPort)
        {
            if (drawingFile == null)
                throw new ArgumentNullException("drawingFile");
            var igesFile = drawingFile as IgesDrawingFile;
            if (igesFile == null)
                throw new ArgumentException("Drawing file was not an IGES file.");
            if (igesFile.File == null)
                throw new ArgumentException("Drawing file had no internal IGES file.");
            var layer = new Layer("igs", Color.Auto);
            foreach (var entity in igesFile.File.Entities)
            {
                var cadEntity = ToEntity(entity);
                if (cadEntity != null)
                {
                    layer = layer.Add(cadEntity);
                }
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));

            viewPort = new ViewPort(
                Point.Origin,
                Vector.ZAxis,
                Vector.YAxis,
                100.0);

            return true;
        }

        public bool ConvertFromDrawing(string fileName, Drawing drawing, ViewPort viewPort, out IDrawingFile drawingFile)
        {
            var file = new IgesFile();
            file.Author = Environment.UserName;
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
                    default:
                        Debug.Fail("Unsupported entity type: " + entity.Kind);
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
                Color = IgesColorNumber.Color0,
                P1 = ToIgesPoint(line.P1),
                P2 = ToIgesPoint(line.P2)
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
            switch (entity.Type)
            {
                case IgesEntityType.Circle:
                    result = ToArc((IgesCircle)entity);
                    break;
                case IgesEntityType.Line:
                    result = ToLine((IgesLine)entity);
                    break;
            }

            return result;
        }

        private static Line ToLine(IgesLine line)
        {
            // TODO: handle different forms (segment, ray, continuous)
            return new Line(TransformPoint(line, line.P1), TransformPoint(line, line.P2), ToColor(line.Color));
        }

        private static Entity ToArc(IgesCircle arc)
        {
            var center = TransformPoint(arc, arc.Center);
            var startPoint = TransformPoint(arc, arc.StartPoint);
            var endPoint = TransformPoint(arc, arc.EndPoint);

            // generate normal; points are given CCW
            var startVector = startPoint - center;
            var endVector = endPoint - center;
            Vector normal;
            if (((Point)startVector).CloseTo(endVector))
                normal = Vector.NormalFromRightVector(startVector.Normalize());
            else
                normal = startVector.Cross(endVector).Normalize();
            Debug.Assert(startVector.IsOrthoganalTo(normal));
            Debug.Assert(endVector.IsOrthoganalTo(normal));

            // find radius from start/end points
            var startRadius = startVector.Length;
            var endRadius = endVector.Length;
            Debug.Assert(MathHelper.CloseTo(startRadius, endRadius));
            var radius = startRadius;

            // if start/end points are the same, it's a circle.  otherwise it's an arc
            if (startPoint.CloseTo(endPoint))
            {
                return new Circle(center, radius, normal, ToColor(arc.Color));
            }
            else
            {
                // project back to unit circle to find start/end angles
                var primitiveCircle = new PrimitiveEllipse(center, radius, normal);
                var toUnit = primitiveCircle.FromUnitCircleProjection();
                toUnit.Invert();
                var startUnit = startPoint.Transform(toUnit);
                var endUnit = endPoint.Transform(toUnit);
                var startAngle = ((Vector)startUnit).ToAngle();
                var endAngle = ((Vector)endUnit).ToAngle();
                return new Arc(center, radius, startAngle, endAngle, normal, ToColor(arc.Color));
            }
        }

        private static Color ToColor(IgesColorNumber color)
        {
            return new Color((byte)color);
        }

        private static Point TransformPoint(IgesEntity entity, IgesPoint point)
        {
            var transformed = entity.TransformationMatrix.Transform(point);
            return new Point(transformed.X, transformed.Y, transformed.Z);
        }
    }
}
