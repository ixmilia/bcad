using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BCad.Collections;
using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using BCad.Entities;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DxfFileHandler.DisplayName, true, true, DxfFileHandler.FileExtension)]
    public class DxfFileHandler : IFileHandler
    {
        public const string DisplayName = "DXF Files (" + FileExtension + ")";
        public const string FileExtension = ".dxf";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag)
        {
            var file = DxfFile.Load(fileStream);

            var layers = new ReadOnlyTree<string, Layer>();

            propertyBag = new Dictionary<string, object>()
            {
                { "ColorMap", ColorMap.AutoCADDefault }
            };

            foreach (var layer in file.Layers)
            {
                layers = layers.Insert(layer.Name, new Layer(layer.Name, layer.Color.ToColor()));
            }

            foreach (var item in file.Entities)
            {
                var layer = GetOrCreateLayer(ref layers, item.Layer);

                // create the entity
                var entity = item.ToEntity();

                // add the entity to the appropriate layer
                if (entity != null)
                {
                    layer = layer.Add(entity);
                    layers = layers.Insert(layer.Name, layer);
                }
            }

            foreach (var block in file.Blocks)
            {
                var layer = GetOrCreateLayer(ref layers, block.Layer);

                // create the aggregate entity
                var children = ReadOnlyList<Entity>.Empty();
                foreach (var item in block.Entities)
                {
                    var tempEnt = item.ToEntity();
                    if (tempEnt != null)
                    {
                        children = children.Add(tempEnt);
                    }
                }

                // add the entity to the appropriate layer
                if (children.Count != 0)
                {
                    layer = layer.Add(new AggregateEntity(block.BasePoint.ToPoint(), children, IndexedColor.Auto));
                    layers = layers.Insert(layer.Name, layer);
                }
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, file.Header.UnitFormat.ToUnitFormat(), file.Header.UnitPrecision),
                layers,
                file.Header.CurrentLayer ?? layers.GetKeys().OrderBy(x => x).First());
            drawing.Tag = file;

            var vp = file.ViewPorts.FirstOrDefault();
            if (vp != null)
            {
                viewPort = new ViewPort(
                    vp.LowerLeft.ToPoint(),
                    vp.ViewDirection.ToVector(),
                    Vector.YAxis,
                    vp.ViewHeight);
            }
            else
            {
                viewPort = null; // auto-set it later
            }

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag)
        {
            var file = new DxfFile();
            var oldFile = drawing.Tag as DxfFile;
            if (oldFile != null)
            {
                // preserve settings from the original file
                file.Header.CreationDate = oldFile.Header.CreationDate;
                file.Header.CreationDateUniversal = oldFile.Header.CreationDateUniversal;
            }

            // save layers and entities
            file.Header.CurrentLayer = drawing.CurrentLayer.Name;
            file.Header.UnitFormat = drawing.Settings.UnitFormat.ToDxfUnitFormat();
            file.Header.UnitPrecision = (short)drawing.Settings.UnitPrecision;
            foreach (var layer in drawing.GetLayers().OrderBy(x => x.Name))
            {
                file.Layers.Add(new DxfLayer(layer.Name, layer.Color.ToDxfColor()));
                foreach (var item in layer.GetEntities().OrderBy(e => e.Id))
                {
                    if (item.Kind == EntityKind.Aggregate)
                    {
                        // dxf files treat aggregate entities as separate items
                        var agg = (AggregateEntity)item;
                        var block = new DxfBlock();
                        block.Layer = layer.Name;
                        block.Entities.AddRange(agg.Children.Select(c => c.ToDxfEntity(layer)));
                    }
                    else
                    {
                        var entity = item.ToDxfEntity(layer);
                        if (entity != null)
                            file.Entities.Add(entity);
                    }
                }
            }

            // save viewport
            file.ViewPorts.Add(new DxfViewPort()
            {
                LowerLeft = viewPort.BottomLeft.ToDxfPoint(),
                ViewDirection = viewPort.Sight.ToDxfVector(),
                ViewHeight = viewPort.ViewHeight
            });

            file.Save(fileStream);
            return true;
        }

        private static Layer GetOrCreateLayer(ref ReadOnlyTree<string, Layer> layers, string layerName)
        {
            Layer layer = null;

            // entities without a layer go to '0'
            layerName = layerName ?? "0";
            if (layers.KeyExists(layerName))
                layer = layers.GetValue(layerName);
            else
            {
                // add the layer if previously undefined
                layer = new Layer(layerName, IndexedColor.Auto);
                layers = layers.Insert(layer.Name, layer);
            }

            return layer;
        }
    }

    internal static class DxfExtensions
    {
        public static IndexedColor ToColor(this DxfColor color)
        {
            if (color.IsIndex)
                return new IndexedColor(color.Index);
            else
                return IndexedColor.Default;
        }

        public static DxfColor ToDxfColor(this IndexedColor color)
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

        public static Location ToPoint(this DxfModelPoint point)
        {
            return new Location(point.Location.ToPoint(), point.Color.ToColor(), point);
        }

        public static Line ToLine(this DxfLine line)
        {
            return new Line(line.P1.ToPoint(), line.P2.ToPoint(), line.Color.ToColor(), line);
        }

        public static Polyline ToPolyline(this DxfPolyline poly)
        {
            return new Polyline(poly.Vertices.Select(v => v.Location.ToPoint()), poly.Color.ToColor(), poly);
        }

        public static Polyline ToPolyline(this DxfLeader leader)
        {
            return new Polyline(leader.Vertices.Select(v => v.ToPoint()), leader.Color.ToColor(), leader);
        }

        public static Circle ToCircle(this DxfCircle circle)
        {
            return new Circle(circle.Center.ToPoint(), circle.Radius, circle.Normal.ToVector(), circle.Color.ToColor(), circle);
        }

        public static Arc ToArc(this DxfArc arc)
        {
            return new Arc(arc.Center.ToPoint(), arc.Radius, arc.StartAngle, arc.EndAngle, arc.Normal.ToVector(), arc.Color.ToColor(), arc);
        }

        public static Ellipse ToEllipse(this DxfEllipse el)
        {
            return new Ellipse(el.Center.ToPoint(), el.MajorAxis.ToVector(), el.MinorAxisRatio, el.StartParameter, el.EndParameter, el.Normal.ToVector(), el.Color.ToColor(), el);
        }

        public static Text ToText(this DxfText text)
        {
            return new Text(text.Value ?? string.Empty, text.Location.ToPoint(), text.Normal.ToVector(), text.TextHeight, text.Rotation, text.Color.ToColor(), text);
        }

        public static Entity ToEntity(this DxfEntity item)
        {
            Entity entity = null;
            switch (item.EntityType)
            {
                case DxfEntityType.Arc:
                    entity = ((DxfArc)item).ToArc();
                    break;
                case DxfEntityType.Circle:
                    entity = ((DxfCircle)item).ToCircle();
                    break;
                case DxfEntityType.Ellipse:
                    entity = ((DxfEllipse)item).ToEllipse();
                    break;
                case DxfEntityType.Leader:
                    entity = ((DxfLeader)item).ToPolyline();
                    break;
                case DxfEntityType.Line:
                    entity = ((DxfLine)item).ToLine();
                    break;
                case DxfEntityType.Point:
                    entity = ((DxfModelPoint)item).ToPoint();
                    break;
                case DxfEntityType.Polyline:
                    entity = ((DxfPolyline)item).ToPolyline();
                    break;
                case DxfEntityType.Text:
                    entity = ((DxfText)item).ToText();
                    break;
                case DxfEntityType.Face:
                case DxfEntityType.ModelerGeometry:
                case DxfEntityType.ProxyEntity:
                case DxfEntityType.Ray:
                case DxfEntityType.Region:
                case DxfEntityType.Seqend:
                case DxfEntityType.Solid:
                case DxfEntityType.Tolerance:
                case DxfEntityType.Trace:
                case DxfEntityType.Vertex:
                case DxfEntityType.XLine:
                    //Debug.Fail("Unsupported DXF entity type: " + item.GetType().Name);
                    break;
            }

            return entity;
        }

        public static DxfModelPoint ToDxfLocation(this Location location, Layer layer)
        {
            return new DxfModelPoint(location.Point.ToDxfPoint())
            {
                Color = location.Color.ToDxfColor(),
                Layer = layer.Name
            };
        }

        public static DxfLine ToDxfLine(this Line line, Layer layer)
        {
            return new DxfLine(line.P1.ToDxfPoint(), line.P2.ToDxfPoint())
            {
                Color = line.Color.ToDxfColor(),
                Layer = layer.Name
            };
        }

        public static DxfPolyline ToDxfPolyline(this Polyline poly, Layer layer)
        {
            var dp = new DxfPolyline()
            {
                Color = poly.Color.ToDxfColor(),
                Elevation = poly.Points.Any() ? poly.Points.First().Z : 0.0,
                Layer = layer.Name,
                Normal = DxfVector.ZAxis
            };
            dp.Vertices.AddRange(poly.Points.Select(p => new DxfVertex(p.ToDxfPoint())));
            return dp;
        }

        public static DxfCircle ToDxfCircle(this Circle circle, Layer layer)
        {
            return new DxfCircle(circle.Center.ToDxfPoint(), circle.Radius)
            {
                Color = circle.Color.ToDxfColor(),
                Normal = circle.Normal.ToDxfVector(),
                Layer = layer.Name
            };
        }

        public static DxfArc ToDxfArc(this Arc arc, Layer layer)
        {
            return new DxfArc(arc.Center.ToDxfPoint(), arc.Radius, arc.StartAngle, arc.EndAngle)
            {
                Color = arc.Color.ToDxfColor(),
                Normal = arc.Normal.ToDxfVector(),
                Layer = layer.Name
            };
        }

        public static DxfEllipse ToDxfEllipse(this Ellipse el, Layer layer)
        {
            return new DxfEllipse(el.Center.ToDxfPoint(), el.MajorAxis.ToDxfVector(), el.MinorAxisRatio)
            {
                Color = el.Color.ToDxfColor(),
                StartParameter = el.StartAngle,
                EndParameter = el.EndAngle,
                Normal = el.Normal.ToDxfVector(),
                Layer = layer.Name
            };
        }

        public static DxfText ToDxfText(this Text text, Layer layer)
        {
            return new DxfText(text.Location.ToDxfPoint(), text.Height, text.Value)
            {
                Color = text.Color.ToDxfColor(),
                Layer = layer.Name,
                Normal = text.Normal.ToDxfVector(),
                Rotation = text.Rotation
            };
        }

        public static DxfEntity ToDxfEntity(this Entity item, Layer layer)
        {
            DxfEntity entity = null;
            switch (item.Kind)
            {
                case EntityKind.Aggregate:
                    // no-op.  aggregates are handled separately
                    break;
                case EntityKind.Arc:
                    // if start/end angles are a full circle, write it that way instead
                    var arc = (Arc)item;
                    if (arc.StartAngle == 0.0 && arc.EndAngle == 360.0)
                        entity = new Circle(arc.Center, arc.Radius, arc.Normal, arc.Color).ToDxfCircle(layer);
                    else
                        entity = ((Arc)item).ToDxfArc(layer);
                    break;
                case EntityKind.Circle:
                    entity = ((Circle)item).ToDxfCircle(layer);
                    break;
                case EntityKind.Ellipse:
                    entity = ((Ellipse)item).ToDxfEllipse(layer);
                    break;
                case EntityKind.Line:
                    entity = ((Line)item).ToDxfLine(layer);
                    break;
                case EntityKind.Location:
                    entity = ((Location)item).ToDxfLocation(layer);
                    break;
                case EntityKind.Polyline:
                    entity = ((Polyline)item).ToDxfPolyline(layer);
                    break;
                case EntityKind.Text:
                    entity = ((Text)item).ToDxfText(layer);
                    break;
                default:
                    Debug.Assert(false, "Unsupported entity type: " + item.GetType().Name);
                    break;
            }

            return entity;
        }

        public static DxfUnitFormat ToDxfUnitFormat(this UnitFormat format)
        {
            switch (format)
            {
                case UnitFormat.Architectural:
                    return DxfUnitFormat.Architectural;
                case UnitFormat.Metric:
                    return DxfUnitFormat.Decimal;
                default:
                    throw new ArgumentException("Unsupported unit format");
            }
        }

        public static UnitFormat ToUnitFormat(this DxfUnitFormat format)
        {
            switch (format)
            {
                case DxfUnitFormat.Architectural:
                case DxfUnitFormat.ArchitecturalStacked:
                case DxfUnitFormat.Fractional:
                case DxfUnitFormat.FractionalStacked:
                    return UnitFormat.Architectural;
                case DxfUnitFormat.Decimal:
                case DxfUnitFormat.Engineering:
                case DxfUnitFormat.Scientific:
                    return UnitFormat.Metric;
                default:
                    throw new ArgumentException("Unsupported unit format");
            }
        }
    }
}
