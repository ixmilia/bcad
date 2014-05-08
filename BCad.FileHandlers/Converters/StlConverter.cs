using System;
using BCad.Collections;
using BCad.Core;
using BCad.Entities;
using BCad.FileHandlers.DrawingFiles;
using BCad.Stl;
using System.Collections.Generic;

namespace BCad.FileHandlers.Converters
{
    public class StlConverter : IDrawingConverter
    {
        public bool ConvertToDrawing(string fileName, IDrawingFile drawingFile, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag)
        {
            if (drawingFile == null)
                throw new ArgumentNullException("drawingFile");
            var stlFile = drawingFile as StlDrawingFile;
            if (stlFile == null)
                throw new ArgumentException("Drawing file was not an STL file.");
            if (stlFile.File == null)
                throw new ArgumentException("Drawing file had no internal STL file.");

            propertyBag = new Dictionary<string, object>
            {
                { "ColorMap", ColorMap.Default }
            };

            var layer = new Layer(stlFile.File.SolidName ?? "stl", IndexedColor.Auto);
            foreach (var triangle in stlFile.File.Triangles)
            {
                layer = layer.Add(new Line(ToPoint(triangle.Vertex1), ToPoint(triangle.Vertex2), IndexedColor.Auto));
                layer = layer.Add(new Line(ToPoint(triangle.Vertex2), ToPoint(triangle.Vertex3), IndexedColor.Auto));
                layer = layer.Add(new Line(ToPoint(triangle.Vertex3), ToPoint(triangle.Vertex1), IndexedColor.Auto));
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, -1),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer));
            drawing.Tag = stlFile.File;

            viewPort = null; // auto-set it later

            return true;
        }

        public bool ConvertFromDrawing(string fileName, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag, out IDrawingFile drawingFile)
        {
            throw new NotImplementedException();
        }

        private static Point ToPoint(StlVertex vertex)
        {
            return new Point(vertex.X, vertex.Y, vertex.Z);
        }
    }
}
