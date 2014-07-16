using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCad.Collections;
using BCad.Dwg;
using BCad.Entities;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DwgFileHandler.DisplayName, true, true, DwgFileHandler.FileExtension)]
    public class DwgFileHandler : IFileHandler
    {
        public const string DisplayName = "DWG Files (" + FileExtension + ")";
        public const string FileExtension = ".dwg";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag)
        {
            var file = DwgFile.Load(fileStream);
            var layers = new ReadOnlyTree<string, Layer>();
            foreach (var layer in file.ObjectMap.Objects.OfType<DwgLayer>())
            {
                var blayer = new Layer(layer.Name, new IndexedColor((byte)layer.Color));
                layers = layers.Insert(blayer.Name, blayer);
            }

            foreach (var ent in file.ObjectMap.Objects.OfType<DwgEntity>())
            {
                Entity entity = null;
                switch (ent.Type)
                {
                    case DwgObjectType.Line:
                        var line = (DwgLine)ent;
                        entity = ToLine(line);
                        break;
                }

                if (entity != null)
                {
                    var layerCandidate = file.ObjectMap.Objects.FirstOrDefault(o => o.Handle == ent.LayerHandle);
                    var dwgLayer = layerCandidate as DwgLayer;
                    var layerName = dwgLayer.Name;
                    var newLayer = layers.GetValue(layerName).Add(entity);
                    layers = layers.Insert(layerName, newLayer);
                }
            }

            drawing = new Drawing(
                new DrawingSettings(),
                layers);
            drawing.Tag = file;
            viewPort = null;
            propertyBag = null;

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag)
        {
            throw new NotImplementedException();
        }

        private static Line ToLine(DwgLine line)
        {
            return new Line(ToPoint(line.StartPoint), ToPoint(line.EndPoint), ToColor(line.Color), line);
        }

        private static IndexedColor ToColor(short color)
        {
            if (color == 0 || color == 256)
                return IndexedColor.Auto;
            return new IndexedColor((byte)color);
        }

        private static Point ToPoint(DwgPoint point)
        {
            return new Point(point.X, point.Y, point.Z);
        }
    }
}
