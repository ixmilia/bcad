using System;
using System.Diagnostics;
using System.Linq;
using BCad.Collections;
using BCad.DrawingFiles;
using BCad.Dxf;
using BCad.Dxf.Entities;
using BCad.Dxf.Sections;
using BCad.Dxf.Tables;
using BCad.Entities;

namespace BCad.Converters
{
    public class DxfConverter : IDrawingConverter
    {
        public bool ConvertToDrawing(string fileName, IDrawingFile drawingFile, out Drawing drawing, out ViewPort viewPort)
        {
            if (drawingFile == null)
                throw new ArgumentNullException("drawingFile");
            var dxfFile = drawingFile as DxfDrawingFile;
            if (dxfFile == null)
                throw new ArgumentException("Drawing file was not a DXF file.");
            if (dxfFile.File == null)
                throw new ArgumentException("Drawing file had no internal DXF file.");
            var layers = new ReadOnlyTree<string, Layer>();

            foreach (var layer in dxfFile.File.TablesSection.LayerTable.Layers)
            {
                layers = layers.Insert(layer.Name, new Layer(layer.Name, layer.Color.ToColor()));
            }

            foreach (var item in dxfFile.File.EntitiesSection.Entities)
            {
                Layer layer = null;

                // entities without a layer go to '0'
                string entityLayer = item.Layer == null ? "0" : item.Layer;
                if (layers.KeyExists(entityLayer))
                    layer = layers.GetValue(entityLayer);
                else
                {
                    // add the layer if previously undefined
                    layer = new Layer(entityLayer, Color.Auto);
                    layers = layers.Insert(layer.Name, layer);
                }

                // create the entity
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
                    case DxfEntityType.Line:
                        entity = ((DxfLine)item).ToLine();
                        break;
                    case DxfEntityType.Polyline:
                        entity = ((DxfPolyline)item).ToPolyline();
                        break;
                    case DxfEntityType.Text:
                        entity = ((DxfText)item).ToText();
                        break;
                    case DxfEntityType.Attribute:
                    case DxfEntityType.Seqend:
                    case DxfEntityType.Vertex:
                        //Debug.Fail("Unsupported DXF entity type: " + item.GetType().Name);
                        break;
                }

                // add the entity to the appropriate layer
                if (entity != null)
                {
                    layer = layer.Add(entity);
                    layers = layers.Insert(layer.Name, layer);
                }
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, dxfFile.File.HeaderSection.UnitFormat.ToUnitFormat(), dxfFile.File.HeaderSection.UnitPrecision),
                layers,
                dxfFile.File.HeaderSection.CurrentLayer ?? layers.GetKeys().OrderBy(x => x).First());

            var vp = dxfFile.File.TablesSection.ViewPortTable.ViewPorts.FirstOrDefault();
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
                viewPort = new ViewPort(
                    Point.Origin,
                    Vector.ZAxis,
                    Vector.YAxis,
                    10.0);
            }

            return true;
        }

        public bool ConvertFromDrawing(string fileName, Drawing drawing, ViewPort viewPort, out IDrawingFile drawingFile)
        {
            var file = new DxfFile();

            // save layers and entities
            file.HeaderSection.CurrentLayer = drawing.CurrentLayer.Name;
            file.HeaderSection.UnitFormat = drawing.Settings.UnitFormat.ToDxfUnitFormat();
            file.HeaderSection.UnitPrecision = (short)drawing.Settings.UnitPrecision;
            foreach (var layer in drawing.GetLayers().OrderBy(x => x.Name))
            {
                file.TablesSection.LayerTable.Layers.Add(new DxfLayer(layer.Name, layer.Color.ToDxfColor()));
                foreach (var item in layer.GetEntities().OrderBy(e => e.Id))
                {
                    DxfEntity entity = null;
                    switch (item.Kind)
                    {
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
                        case EntityKind.Polyline:
                            entity = ((Polyline)item).ToDxfPolyline(layer);
                            break;
                        case EntityKind.Text:
                            entity = ((Text)item).ToDxfText(layer);
                            break;
                        default:
                            Debug.Fail("Unsupported entity type: " + item.GetType().Name);
                            break;
                    }

                    if (entity != null)
                        file.EntitiesSection.Entities.Add(entity);
                }
            }

            // save viewport
            file.TablesSection.ViewPortTable.ViewPorts.Add(new DxfViewPort()
            {
                LowerLeft = viewPort.BottomLeft.ToDxfPoint(),
                ViewDirection = viewPort.Sight.ToDxfVector(),
                ViewHeight = viewPort.ViewHeight
            });

            drawingFile = new DxfDrawingFile(file);
            return true;
        }
    }

    internal static class DxfExtensions
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

        public static Polyline ToPolyline(this DxfPolyline poly)
        {
            return new Polyline(poly.Vertices.Select(v => v.Location.ToPoint()), poly.Color.ToColor());
        }

        public static Circle ToCircle(this DxfCircle circle)
        {
            return new Circle(circle.Center.ToPoint(), circle.Radius, circle.Normal.ToVector(), circle.Color.ToColor());
        }

        public static Arc ToArc(this DxfArc arc)
        {
            return new Arc(arc.Center.ToPoint(), arc.Radius, arc.StartAngle, arc.EndAngle, arc.Normal.ToVector(), arc.Color.ToColor());
        }

        public static Ellipse ToEllipse(this DxfEllipse el)
        {
            return new Ellipse(el.Center.ToPoint(), el.MajorAxis.ToVector(), el.MinorAxisRatio, el.StartParameter, el.EndParameter, el.Normal.ToVector(), el.Color.ToColor());
        }

        public static Text ToText(this DxfText text)
        {
            return new Text(text.Value ?? string.Empty, text.Location.ToPoint(), text.Normal.ToVector(), text.TextHeight, text.Rotation, text.Color.ToColor());
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
