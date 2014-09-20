using System;
using System.Collections.Generic;
using System.IO;
using BCad.Collections;
using BCad.Entities;
using IxMilia.Stl;

namespace BCad.FileHandlers
{
    [ExportFileHandler(StlFileHandler.DisplayName, true, false, StlFileHandler.FileExtension)]
    public class StlFileHandler: IFileHandler
    {
        public const string DisplayName = "STL Files (" + FileExtension + ")";
        public const string FileExtension = ".stl";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag)
        {
            var file = StlFile.Load(fileStream);
            propertyBag = new Dictionary<string, object>
            {
                { "ColorMap", ColorMap.Default }
            };

            var lines = new Line[file.Triangles.Count * 3];
            var index = 0;
            foreach (var triangle in file.Triangles)
            {
                lines[index++] = new Line(ToPoint(triangle.Vertex1), ToPoint(triangle.Vertex2), IndexedColor.Auto);
                lines[index++] = new Line(ToPoint(triangle.Vertex2), ToPoint(triangle.Vertex3), IndexedColor.Auto);
                lines[index++] = new Line(ToPoint(triangle.Vertex3), ToPoint(triangle.Vertex1), IndexedColor.Auto);
            }

            var layer = new Layer(file.SolidName ?? "stl", IndexedColor.Auto, lines);
            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));
            drawing.Tag = file;

            viewPort = null; // auto-set it later

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag)
        {
            throw new NotImplementedException();
        }

        private static Point ToPoint(StlVertex vertex)
        {
            return new Point(vertex.X, vertex.Y, vertex.Z);
        }
    }
}
