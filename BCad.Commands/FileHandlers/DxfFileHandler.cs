using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCad;
using BCad.Dxf;
using BCad.Dxf.Entities;
using BCad.Dxf.Tables;
using BCad.FileHandlers;
using BCad.Objects;
using System.ComponentModel.Composition;

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(DxfFileHandler.DisplayName, DxfFileHandler.FileExtension)]
    [ExportFileWriter(DxfFileHandler.DisplayName, DxfFileHandler.FileExtension)]
    internal class DxfFileHandler : IFileReader, IFileWriter
    {
        public const string DisplayName = "DXF Files (" + FileExtension + ")";
        public const string FileExtension = ".dxf";

        public void ReadFile(string fileName, Stream stream, out Document document, out Layer currentLayer)
        {
            var file = DxfFile.Open(stream);
            var layers = new Dictionary<string, Layer>()
            {
                { "Default", new Layer("Default", Color.Auto) } // ensure a default layer
            };

            foreach (var layer in file.Layers)
            {
                // remap layer 0 to Default and add to the collection
                var newName = layer.Name == "0" ? "Default" : layer.Name;
                layers[newName] = new Layer(newName, layer.Color.ToColor());
            }

            foreach (var item in file.Entities)
            {
                Layer layer = null;

                // remap layer 0 to Default and ensure the layer exists
                string objectLayer = (item.Layer == null || item.Layer == "0") ? "Default" : item.Layer;
                if (layers.ContainsKey(objectLayer))
                    layer = layers[objectLayer];
                else
                {
                    // add the layer if previously undefined
                    layer = new Layer(objectLayer, Color.Auto);
                    layers[objectLayer] = layer;
                }

                // create the object
                IObject obj = null;
                if (item is DxfLine)
                    obj = ((DxfLine)item).ToLine();
                else if (item is DxfCircle)
                    obj = ((DxfCircle)item).ToCircle();
                else if (item is DxfArc)
                    obj = ((DxfArc)item).ToArc();

                // add the object to the appropriate layer
                if (obj != null)
                {
                    layer = layer.Add(obj);
                    layers[objectLayer] = layer;
                }
            }

            document = new Document(Path.GetFullPath(fileName), layers);
            currentLayer = !string.IsNullOrEmpty(file.CurrentLayer)
                ? layers.First(l => l.Key == file.CurrentLayer).Value
                : layers.First().Value;
        }

        public void WriteFile(IWorkspace workspace, Stream stream)
        {
            var file = new DxfFile();
            file.CurrentLayer = workspace.CurrentLayer.Name;
            var document = workspace.Document;

            foreach (var layer in document.Layers.Values)
            {
                string layerName = layer.Name;
                if (layerName == "Default")
                    layerName = "0";
                file.Layers.Add(new DxfLayer(layerName, layer.Color.ToDxfColor()));
                foreach (var item in layer.Objects)
                {
                    DxfEntity entity = null;
                    if (item is Line)
                        entity = ((Line)item).ToDxfLine();
                    else if (item is Circle)
                        entity = ((Circle)item).ToDxfCircle();
                    else if (item is Arc)
                        entity = ((Arc)item).ToDxfArc();

                    if (entity != null)
                        file.Entities.Add(entity);
                }
            }

            file.Save(stream);
        }
    }

    internal static class DxfHelper
    {
        public static Color ToColor(this DxfColor color)
        {
            if (color.IsIndex)
                return new Color(color.Index);
            else
                return Color.Default;
        }

        public static DxfColor ToDxfColor(this Color color)
        {
            if (color.IsAuto)
                return DxfColor.ByLayer;
            else
                return DxfColor.FromIndex(color.Value);
        }

        public static Point ToPoint(this DxfPoint point)
        {
            return new Point(point.X, point.Y, point.Z);
        }

        public static DxfPoint ToDxfPoint(this Point point)
        {
            return new DxfPoint(point.X, point.Y, point.Z);
        }

        public static Vector ToVector(this DxfVector vector)
        {
            return new Vector(vector.X, vector.Y, vector.Z);
        }

        public static DxfVector ToDxfVector(this Vector vector)
        {
            return new DxfVector(vector.X, vector.Y, vector.Z);
        }

        public static Line ToLine(this DxfLine line)
        {
            return new Line(line.P1.ToPoint(), line.P2.ToPoint(), line.Color.ToColor());
        }

        public static Circle ToCircle(this DxfCircle circle)
        {
            return new Circle(circle.Center.ToPoint(), circle.Radius, circle.Normal.ToVector(), circle.Color.ToColor());
        }

        public static Arc ToArc(this DxfArc arc)
        {
            return new Arc(arc.Center.ToPoint(), arc.Radius, arc.StartAngle, arc.EndAngle, arc.Normal.ToVector(), arc.Color.ToColor());
        }

        public static DxfLine ToDxfLine(this Line line)
        {
            return new DxfLine(line.P1.ToDxfPoint(), line.P2.ToDxfPoint())
            {
                Color = line.Color.ToDxfColor(),
            };
        }

        public static DxfCircle ToDxfCircle(this Circle circle)
        {
            return new DxfCircle(circle.Center.ToDxfPoint(), circle.Radius)
            {
                Color = circle.Color.ToDxfColor(),
                Normal = circle.Normal.ToDxfVector()
            };
        }

        public static DxfArc ToDxfArc(this Arc arc)
        {
            return new DxfArc(arc.Center.ToDxfPoint(), arc.Radius, arc.StartAngle, arc.EndAngle)
            {
                Color = arc.Color.ToDxfColor(),
                Normal = arc.Normal.ToDxfVector()
            };
        }
    }
}
