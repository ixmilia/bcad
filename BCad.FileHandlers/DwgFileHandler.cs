using System;
using System.IO;
using System.Linq;
using BCad.Collections;
using IxMilia.Dwg;
using BCad.Entities;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DwgFileHandler.DisplayName, true, false, DwgFileHandler.FileExtension)]
    public class DwgFileHandler : IFileHandler
    {
        public const string DisplayName = "DWG Files (" + FileExtension + ")";
        public const string FileExtension = ".dwg";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
        {
            var file = DwgFile.Load(fileStream);
            var layers = new ReadOnlyTree<string, Layer>();
            foreach (var layer in file.ObjectMap.Objects.OfType<DwgLayer>())
            {
                var blayer = new Layer(layer.Name, ToColor(layer.Color));
                layers = layers.Insert(blayer.Name, blayer);
            }

            foreach (var ent in file.ObjectMap.Objects.OfType<DwgEntity>().Where(e => e.SubentityReferenceHandle == 0))
            {
                Entity entity = null;
                switch (ent.Type)
                {
                    case DwgObjectType.Text:
                        entity = ToText((DwgText)ent);
                        break;
                    case DwgObjectType.Arc:
                        entity = ToArc((DwgArc)ent);
                        break;
                    case DwgObjectType.Circle:
                        entity = ToCircle((DwgCircle)ent);
                        break;
                    case DwgObjectType.Point:
                        entity = ToLocation((DwgEntityPoint)ent);
                        break;
                    case DwgObjectType.Line:
                        entity = ToLine((DwgLine)ent);
                        break;
                }

                if (entity != null)
                {
                    var layerCandidate = file.ObjectMap.Objects.FirstOrDefault(o => o.Handle == ent.LayerHandle);
                    var dwgLayer = layerCandidate as DwgLayer;
                    var layerName = dwgLayer == null ? "0" : dwgLayer.Name;
                    var newLayer = layers.GetValue(layerName).Add(entity);
                    layers = layers.Insert(layerName, newLayer);
                }
            }

            if (layers.Count == 0)
            {
                layers = layers.Insert("0", new Layer("0", null));
            }

            drawing = new Drawing(
                new DrawingSettings(),
                layers);
            drawing.Tag = file;
            viewPort = null;

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort)
        {
            throw new NotImplementedException();
        }

        private static Line ToLine(DwgLine line)
        {
            return new Line(ToPoint(line.StartPoint), ToPoint(line.EndPoint), ToColor(line.Color), line);
        }

        private static Circle ToCircle(DwgCircle circle)
        {
            return new Circle(ToPoint(circle.Center), circle.Radius, ToVector(circle.Extrusion), ToColor(circle.Color), circle);
        }

        private static Location ToLocation(DwgEntityPoint point)
        {
            return new Location(ToPoint(point.Location), ToColor(point.Color), point);
        }

        private static Arc ToArc(DwgArc arc)
        {
            return new Arc(ToPoint(arc.Center), arc.Radius, arc.StartAngle, arc.EndAngle, ToVector(arc.Extrusion), ToColor(arc.Color), arc);
        }

        private static Text ToText(DwgText text)
        {
            return new Text(text.Value, new Point(text.InsertionPoint.X, text.InsertionPoint.Y, text.Elevation), ToVector(text.Extrusion), text.Height, text.RotationAngle, ToColor(text.Color), text);
        }

        private static CadColor? ToColor(int color)
        {
            if (color == 0 || color == 256)
                return null;
            return CadColor.FromInt32(color);
        }

        private static Point ToPoint(DwgPoint point)
        {
            return new Point(point.X, point.Y, point.Z);
        }

        private static Vector ToVector(DwgVector vector)
        {
            return new Vector(vector.X, vector.Y, vector.Z);
        }
    }
}
