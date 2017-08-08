// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.Step;
using IxMilia.Step.Items;

namespace IxMilia.BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, false, FileExtension1, FileExtension2)]
    public class StepFileHandler : IFileHandler
    {
        public const string DisplayName = "Step Files (" + FileExtension1 + ", " + FileExtension2 + ")";
        public const string FileExtension1 = ".stp";
        public const string FileExtension2 = ".step";

        public INotifyPropertyChanged GetFileSettingsFromDrawing(Drawing drawing)
        {
            throw new NotImplementedException();
        }

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
        {
            var layer = new Layer("step");
            var file = StepFile.Load(fileStream);
            foreach (var item in file.GetTopLevelItems())
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
                            var circle = new Circle(center, stepCircle.Radius, normal);
                            layer = layer.Add(circle);
                            break;
                        }
                    case StepItemType.EdgeCurve:
                        {
                            layer = layer.Add(ToEntity((StepEdge)item));
                            break;
                        }
                    case StepItemType.Line:
                        {
                            var line = ToLine((StepLine)item);
                            layer = layer.Add(line);
                            break;
                        }
                    case StepItemType.OrientedEdge:
                        {
                            var orientedEdge = (StepOrientedEdge)item;
                            layer = layer.Add(ToEntity(orientedEdge.EdgeElement));
                            break;
                        }
                }
            }

            drawing = new Drawing().Add(layer);
            viewPort = null;

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, INotifyPropertyChanged fileSettings)
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
            return new Line(p1, p2);
        }

        private static Entity ToEntity(StepEdge edge)
        {
            switch (edge.ItemType)
            {
                case StepItemType.EdgeCurve:
                    {
                        var edgeCurve = (StepEdgeCurve)edge;
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
                            return new Arc(primitiveCircle.Center, circle.Radius, startAngle, endAngle, primitiveCircle.Normal);
                        }
                        else if (edgeCurve.EdgeGeometry is StepLine)
                        {
                            var stepLine = (StepLine)edgeCurve.EdgeGeometry;
                            Line line;
                            if (edgeCurve.EdgeStart == null || edgeCurve.EdgeEnd == null)
                            {
                                // use auto values
                                line = ToLine(stepLine);
                            }
                            else
                            {
                                // use explicit values
                                var p1 = ToPoint((StepVertexPoint)edgeCurve.EdgeStart);
                                var p2 = ToPoint((StepVertexPoint)edgeCurve.EdgeEnd);
                                line = new Line(p1, p2);
                            }

                            return line;
                        }

                        break;
                    }
            }

            return null;
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
