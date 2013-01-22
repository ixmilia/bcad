using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCad;
using BCad.Collections;
using BCad.Dxf;
using BCad.Dxf.Entities;
using BCad.Dxf.Tables;
using BCad.Entities;
using BCad.Extensions;
using BCad.FileHandlers;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace BCad.Commands.FileHandlers
{
    [ExportFileReader(DxfFileHandler.DisplayName, DxfFileHandler.FileExtension)]
    [ExportFileWriter(DxfFileHandler.DisplayName, DxfFileHandler.FileExtension)]
    internal class DxfFileHandler : IFileReader, IFileWriter
    {
        public const string DisplayName = "DXF Files (" + FileExtension + ")";
        public const string FileExtension = ".dxf";

        public void ReadFile(string fileName, Stream stream, out Drawing drawing, out ViewPort activeViewPort)
        {
            var file = DxfFile.Load(stream);
            var layers = new Dictionary<string, Layer>();

            foreach (var layer in file.Layers)
            {
                layers[layer.Name] = new Layer(layer.Name, layer.Color.ToColor());
            }

            foreach (var item in file.Entities)
            {
                Layer layer = null;

                // entities without a layer go to '0'
                string entityLayer = item.Layer == null ? "0" : item.Layer;
                if (layers.ContainsKey(entityLayer))
                    layer = layers[entityLayer];
                else
                {
                    // add the layer if previously undefined
                    layer = new Layer(entityLayer, Color.Auto);
                    layers[entityLayer] = layer;
                }

                // create the entity
                Entity entity = null;
                if (item is DxfLine)
                    entity = ((DxfLine)item).ToLine();
                else if (item is DxfCircle)
                    entity = ((DxfCircle)item).ToCircle();
                else if (item is DxfArc)
                    entity = ((DxfArc)item).ToArc();
                else if (item is DxfEllipse)
                    entity = ((DxfEllipse)item).ToEllipse();
                else if (item is DxfPolyline)
                    entity = ((DxfPolyline)item).ToPolyline();
                else if (item is DxfText)
                    entity = ((DxfText)item).ToText();
                else
                {
                    //Debug.Fail("Unsupported DXF entity type: " + item.GetType().Name);
                }

                // add the entity to the appropriate layer
                if (entity != null)
                {
                    layer = layer.Add(entity);
                    layers[entityLayer] = layer;
                }
            }

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.None, -1),
                layers.ToReadOnlyDictionary(),
                file.HeaderSection.CurrentLayer ?? layers.Keys.OrderBy(x => x).First());

            var vp = file.ViewPorts.FirstOrDefault();
            if (vp != null)
            {
                activeViewPort = new ViewPort(
                    vp.LowerLeft.ToPoint(),
                    vp.ViewDirection.ToVector(),
                    Vector.YAxis,
                    vp.ViewHeight);
            }
            else
            {
                activeViewPort = new ViewPort(
                    Point.Origin,
                    Vector.ZAxis,
                    Vector.YAxis,
                    10.0);
            }
        }

        public void WriteFile(IWorkspace workspace, Stream stream)
        {
            var file = new DxfFile();

            // save layers and entities
            file.HeaderSection.CurrentLayer = workspace.Drawing.CurrentLayer.Name;
            foreach (var layer in workspace.Drawing.Layers.Values.OrderBy(x => x.Name))
            {
                file.Layers.Add(new DxfLayer(layer.Name, layer.Color.ToDxfColor()));
                foreach (var item in layer.Entities.OrderBy(e => e.Id))
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
                        file.Entities.Add(entity);
                }
            }

            // save viewport
            var vp = workspace.ActiveViewPort;
            file.ViewPorts.Add(new DxfViewPort()
            {
                LowerLeft = vp.BottomLeft.ToDxfPoint(),
                ViewDirection = vp.Sight.ToDxfVector(),
                ViewHeight = vp.ViewHeight
            });

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
            return new Text(text.Value, text.Location.ToPoint(), text.Normal.ToVector(), text.TextHeight, text.Rotation, text.Color.ToColor());
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
    }
}
