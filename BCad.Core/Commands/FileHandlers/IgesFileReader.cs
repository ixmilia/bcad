using System.IO;
using BCad.Collections;
using BCad.Entities;
using BCad.FileHandlers;
using BCad.Iges;
using BCad.Iges.Entities;

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(IgesFileReader.DisplayName, IgesFileReader.FileExtension)]
    internal class IgesFileReader : IFileReader
    {
        public const string DisplayName = "IGES Files (" + FileExtension + ")";
        public const string FileExtension = ".igs";

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
                    result = ToCircle((IgesCircle)entity);
                    break;
                case IgesEntityType.Line:
                    result = ToLine((IgesLine)entity);
                    break;
            }

            return result;
        }

        private static Line ToLine(IgesLine line)
        {
            // TODO: transforms
            return new Line(ToPoint(line.P1), ToPoint(line.P2), ToColor(line.Color));
        }

        private static Circle ToCircle(IgesCircle circle)
        {
            // TODO: handle start/end points and arcs
            var center = ToPoint(circle.Center);
            var other = ToPoint(circle.StartPoint);
            var radius = (other - center).Length;
            // TODO: transforms and normal
            return new Circle(center, radius, Vector.ZAxis, ToColor(circle.Color));
        }

        private static Color ToColor(IgesColorNumber color)
        {
            return new Color((byte)color);
        }

        private static Point ToPoint(IgesPoint point)
        {
            return new Point(point.X, point.Y, point.Z);
        }
    }
}
