﻿using System.Diagnostics;
using System.IO;
using BCad.Collections;
using BCad.Entities;
using BCad.Extensions;
using BCad.FileHandlers;
using BCad.Helpers;
using BCad.Iges;
using BCad.Iges.Entities;
using BCad.Primitives;

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(IgesFileReader.DisplayName, IgesFileReader.FileExtension1, IgesFileReader.FileExtension2)]
    internal class IgesFileReader : IFileReader
    {
        public const string DisplayName = "IGES Files (" + FileExtension1 + ", " + FileExtension2 + ")";
        public const string FileExtension1 = ".igs";
        public const string FileExtension2 = ".iges";

        public void ReadFile(string fileName, Stream stream, out Drawing drawing, out ViewPort activeViewPort)
        {
            var file = IgesFile.Load(stream);
            var layer = new Layer("igs", Color.Auto);
            foreach (var entity in file.Entities)
            {
                var cadEntity = ToEntity(entity);
                if (cadEntity != null)
                {
                    layer = layer.Add(cadEntity);
                }
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.None, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));

            activeViewPort = new ViewPort(
                Point.Origin,
                Vector.ZAxis,
                Vector.YAxis,
                100.0);
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