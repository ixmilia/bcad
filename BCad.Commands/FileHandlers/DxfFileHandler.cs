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

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(DxfFileHandler.DisplayName, DxfFileHandler.FileExtension)]
    [ExportFileWriter(DxfFileHandler.DisplayName, DxfFileHandler.FileExtension)]
    internal class DxfFileHandler : IFileReader, IFileWriter
    {
        public const string DisplayName = "DXF Files (" + FileExtension + ")";

        public const string FileExtension = ".dxf";

        public Document ReadFile(Stream stream)
        {
            var doc = new Document();
            var file = new DxfFile(stream);

            foreach (var layer in file.Layers)
            {
                if (layer.Name != "0")
                {
                    // default layer is already added
                    var newLayer = new Layer(layer.Name, layer.Color.ToColor());
                    doc.AddLayer(newLayer);
                }
            }

            foreach (var item in file.Entities)
            {
                Layer layer = null;

                if (item.Layer == null || item.Layer == "0")
                    layer = doc.DefaultLayer;
                else
                    layer = doc.GetLayerByName(item.Layer);
                if (layer == null)
                {
                    layer = new Layer(item.Layer, Color.Default);
                    doc.AddLayer(layer);
                }

                IObject obj = null;
                if (item is DxfLine)
                    obj = ((DxfLine)item).ToLine(layer);
                else if (item is DxfCircle)
                    obj = ((DxfCircle)item).ToCircle(layer);
                else if (item is DxfArc)
                    obj = ((DxfArc)item).ToArc(layer);

                if (obj != null)
                    doc.AddObject(obj);
            }

            return doc;
        }

        public void WriteFile(Document document, Stream stream)
        {
            var file = new DxfFile();

            foreach (var layer in document.Layers)
            {
                string layerName = layer.Name;
                if (layer == document.DefaultLayer)
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

        public static Line ToLine(this DxfLine line, Layer layer)
        {
            return new Line(line.P1.ToPoint(), line.P2.ToPoint(), line.Color.ToColor(), layer);
        }

        public static Circle ToCircle(this DxfCircle circle, Layer layer)
        {
            return new Circle(circle.Center.ToPoint(), circle.Radius, circle.Normal.ToVector(), circle.Color.ToColor(), layer);
        }

        public static Arc ToArc(this DxfArc arc, Layer layer)
        {
            return new Arc(arc.Center.ToPoint(), arc.Radius, arc.StartAngle, arc.EndAngle, arc.Normal.ToVector(), arc.Color.ToColor(), layer);
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
