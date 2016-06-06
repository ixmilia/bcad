using System;
using System.IO;
using BCad.Entities;
using BCad.Helpers;
using BCad.Primitives;
using IxMilia.Step;
using IxMilia.Step.Items;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, false, FileExtension1, FileExtension2)]
    public class StepFileHandler : IFileHandler
    {
        public const string DisplayName = "Step Files (" + FileExtension1 + ", " + FileExtension2 + ")";
        public const string FileExtension1 = ".stp";
        public const string FileExtension2 = ".step";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
        {
            var layer = new Layer("step", null);
            var file = StepFile.Load(fileStream);
            foreach (var item in file.GetTopLevelitems())
            {
                switch (item.ItemType)
                {
                    case StepItemType.Circle:
                        {
                            var stepCircle = (StepCircle)item;
                            var center = ToPoint(stepCircle.Position.Location);
                            var normal = stepCircle.Position is StepAxis2Placement3D
                                ? ToVector(((StepAxis2Placement3D)stepCircle.Position).Axis)
                                : Vector.ZAxis;
                            var circle = new Circle(center, stepCircle.Radius, normal, null);
                            layer = layer.Add(circle);
                            break;
                        }
                    case StepItemType.EdgeCurve:
                        {
                            var edgeCurve = (StepEdgeCurve)item;
                            if (edgeCurve.EdgeGeometry is StepCircle)
                            {
                                var circle = (StepCircle)edgeCurve.EdgeGeometry;
                                var center = circle.Position.Location;
                                var normal = ToVector(circle.Position.RefDirection);
                                normal = Vector.ZAxis;
                                var primitiveCircle = new PrimitiveEllipse(ToPoint(center), circle.Radius, normal);
                                var toUnit = primitiveCircle.FromUnitCircle.Inverse();
                                var startPoint = toUnit.Transform(ToPoint((StepVertexPoint)edgeCurve.EdgeStart));
                                var endPoint = toUnit.Transform(ToPoint((StepVertexPoint)edgeCurve.EdgeEnd));
                                var startAngle = Math.Atan2(startPoint.Y, startPoint.X).CorrectAngleRadians() * MathHelper.RadiansToDegrees;
                                var endAngle = Math.Atan2(endPoint.Y, endPoint.X).CorrectAngleRadians() * MathHelper.RadiansToDegrees;
                                var arc = new Arc(primitiveCircle.Center, circle.Radius, startAngle, endAngle, primitiveCircle.Normal, null);
                                layer = layer.Add(arc);
                            }
                            else if (edgeCurve.EdgeGeometry is StepLine)
                            {
                                var stepLine = (StepLine)edgeCurve.EdgeGeometry;
                                layer = layer.Add(ToLine(stepLine));
                            }
                            break;
                        }
                    case StepItemType.Line:
                        {
                            var line = ToLine((StepLine)item);
                            layer = layer.Add(line);
                            break;
                        }
                }
            }

            drawing = new Drawing().Add(layer);
            viewPort = null;

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort)
        {
            throw new NotImplementedException();
        }

        private static Line ToLine(StepLine stepLine)
        {
            var dx = stepLine.Vector.Direction.X * stepLine.Vector.Length;
            var dy = stepLine.Vector.Direction.Y * stepLine.Vector.Length;
            var dz = stepLine.Vector.Direction.Z * stepLine.Vector.Length;
            var p1 = ToPoint(stepLine.Point);
            var p2 = p1 + new Vector(dx, dy, dz);
            return new Line(p1, p2, null);
        }

        private static Point ToPoint(StepCartesianPoint point)
        {
            return new Point(point.X, point.Y, point.Z);
        }

        private static Point ToPoint(StepVertexPoint vertex)
        {
            return ToPoint(vertex.Location);
        }

        private static Vector ToVector(StepDirection direction)
        {
            return new Vector(direction.X, direction.Y, direction.Z);
        }
    }
}
