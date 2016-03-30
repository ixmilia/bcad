using System;
using System.IO;
using BCad.Entities;
using IxMilia.Step;
using IxMilia.Step.Entities;

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
            foreach (var entity in file.Entities)
            {
                switch (entity.EntityType)
                {
                    case StepEntityType.Line:
                        var stepLine = (StepLine)entity;
                        var dx = stepLine.Vector.Direction.X * stepLine.Vector.Length;
                        var dy = stepLine.Vector.Direction.Y * stepLine.Vector.Length;
                        var dz = stepLine.Vector.Direction.Z * stepLine.Vector.Length;
                        var p1 = ToPoint(stepLine.Point);
                        var p2 = p1 + new Vector(dx, dy, dz);
                        var line = new Line(p1, p2, null);
                        layer = layer.Add(line);
                        break;
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

        private static Point ToPoint(StepCartesianPoint point)
        {
            return new Point(point.X, point.Y, point.Z);
        }
    }
}
