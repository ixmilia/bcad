using System;
using System.IO;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.Stl;

namespace IxMilia.BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, false, FileExtension)]
    public class StlFileHandler: IFileHandler
    {
        public const string DisplayName = "STL Files (" + FileExtension + ")";
        public const string FileExtension = ".stl";

        public object GetFileSettingsFromDrawing(Drawing drawing)
        {
            throw new NotImplementedException();
        }

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
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
            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));
            drawing.Tag = file;

            viewPort = null; // auto-set it later

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings)
        {
            throw new NotImplementedException();
        }

        private static Point ToPoint(StlVertex vertex)
        {
            return new Point(vertex.X, vertex.Y, vertex.Z);
        }
    }
}
