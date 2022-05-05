using System;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.Stl;

namespace IxMilia.BCad.FileHandlers
{
    public class StlFileHandler : IFileHandler
    {
        public object GetFileSettingsFromDrawing(Drawing drawing)
        {
            throw new NotImplementedException();
        }

        public Task<ReadDrawingResult> ReadDrawing(string fileName, Stream fileStream, Func<string, Task<byte[]>> contentResolver)
        {
            var file = StlFile.Load(fileStream);
            var lines = new Line[file.Triangles.Count * 3];
            var index = 0;
            foreach (var triangle in file.Triangles)
            {
                lines[index++] = new Line(ToPoint(triangle.Vertex1), ToPoint(triangle.Vertex2));
                lines[index++] = new Line(ToPoint(triangle.Vertex2), ToPoint(triangle.Vertex3));
                lines[index++] = new Line(ToPoint(triangle.Vertex3), ToPoint(triangle.Vertex1));
            }

            var layer = new Layer(file.SolidName ?? "stl", lines);
            var drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, -1, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));
            drawing.Tag = file;

            return Task.FromResult(ReadDrawingResult.Succeeded(drawing, null));
        }

        public Task<bool> WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings)
        {
            throw new NotImplementedException();
        }

        private static Point ToPoint(StlVertex vertex)
        {
            return new Point(vertex.X, vertex.Y, vertex.Z);
        }
    }
}
