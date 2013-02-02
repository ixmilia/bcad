using System.IO;
using BCad.Collections;
using BCad.Entities;
using BCad.FileHandlers;
using BCad.Stl;

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(StlFileReader.DisplayName, StlFileReader.FileExtension)]
    internal class StlFileReader : IFileReader
    {
        public const string DisplayName = "STL Files (" + FileExtension + ")";
        public const string FileExtension = ".stl";

        public void ReadFile(string fileName, Stream stream, out Drawing drawing, out ViewPort activeViewPort)
        {
            var file = StlFile.Load(stream);
            var layer = new Layer(file.SolidName ?? "stl", Color.Auto);
            foreach (var triangle in file.Triangles)
            {
                layer = layer.Add(new Line(ToPoint(triangle.Vertex1), ToPoint(triangle.Vertex2), Color.Auto));
                layer = layer.Add(new Line(ToPoint(triangle.Vertex2), ToPoint(triangle.Vertex3), Color.Auto));
                layer = layer.Add(new Line(ToPoint(triangle.Vertex3), ToPoint(triangle.Vertex1), Color.Auto));
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.None, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));

            activeViewPort = new ViewPort(
                Point.Origin,
                Vector.ZAxis,
                Vector.YAxis,
                10.0);
        }

        private static Point ToPoint(StlVertex vertex)
        {
            return new Point(vertex.X, vertex.Y, vertex.Z);
        }
    }
}
