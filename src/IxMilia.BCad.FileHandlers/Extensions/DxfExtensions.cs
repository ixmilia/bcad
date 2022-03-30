using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using IxMilia.Dxf.Objects;

namespace IxMilia.BCad.FileHandlers.Extensions
{
    public static class DxfExtensions
    {
        public static DxfFileVersion ToFileVersion(this DxfAcadVersion version)
        {
            switch (version)
            {
                case DxfAcadVersion.R12:
                    return DxfFileVersion.R12;
                case DxfAcadVersion.R13:
                    return DxfFileVersion.R13;
                case DxfAcadVersion.R14:
                    return DxfFileVersion.R14;
                case DxfAcadVersion.R2000:
                    return DxfFileVersion.R2000;
                case DxfAcadVersion.R2004:
                    return DxfFileVersion.R2004;
                case DxfAcadVersion.R2007:
                    return DxfFileVersion.R2007;
                case DxfAcadVersion.R2010:
                    return DxfFileVersion.R2010;
                case DxfAcadVersion.R2013:
                    return DxfFileVersion.R2013;
                default:
                    return DxfFileVersion.R12;
            }
        }

        public static DxfAcadVersion ToDxfFileVersion(this DxfFileVersion version)
        {
            switch (version)
            {
                case DxfFileVersion.R12:
                    return DxfAcadVersion.R12;
                case DxfFileVersion.R13:
                    return DxfAcadVersion.R13;
                case DxfFileVersion.R14:
                    return DxfAcadVersion.R14;
                case DxfFileVersion.R2000:
                    return DxfAcadVersion.R2000;
                case DxfFileVersion.R2004:
                    return DxfAcadVersion.R2004;
                case DxfFileVersion.R2007:
                    return DxfAcadVersion.R2007;
                case DxfFileVersion.R2010:
                    return DxfAcadVersion.R2010;
                case DxfFileVersion.R2013:
                    return DxfAcadVersion.R2013;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static CadColor? ToColor(this DxfColor color)
        {
            // from a color index, get the real RGB values
            if (color.IsIndex)
                return CadColor.FromUInt32(DxfColor.DefaultColors[color.Index]);
            else
                return null;
        }

        public static DxfColor ToDxfColor(this CadColor? color)
        {
            if (color == null)
            {
                return DxfColor.ByLayer;
            }
            else
            {
                // find the index of the matching default color
                int i;
                for (i = 0; i < DxfColor.DefaultColors.Count; i++)
                {
                    if (color.Value.ToUInt32() == DxfColor.DefaultColors[i])
                    {
                        break;
                    }
                }

                if (i < 256)
                {
                    return DxfColor.FromIndex((byte)i);
                }
                else
                {
                    // TODO: find closest matching color?
                    return DxfColor.ByLayer;
                }
            }
        }

        public static void AssignDxfEntityColor(this DxfEntity dxfEntity, CadColor? color)
        {
            dxfEntity.Color = color.ToDxfColor();
            if (dxfEntity.Color == DxfColor.ByLayer && color.HasValue)
            {
                // specified color not found in color table
                dxfEntity.Color24Bit = color.GetValueOrDefault().ToInt32();
                dxfEntity.ColorName = "Custom";
            }
            else
            {
                dxfEntity.Color24Bit = 0;
                dxfEntity.ColorName = null;
            }
        }

        public static CadColor? GetEntityColor(this DxfEntity dxfEntity)
        {
            if (dxfEntity.Color24Bit == 0)
            {
                return dxfEntity.Color.ToColor();
            }
            else
            {
                // custom-assigned color
                return CadColor.FromInt32(dxfEntity.Color24Bit);
            }
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

        public static Vertex ToVertex(this DxfVertex vertex)
        {
            return VertexFromPointAndBulge(vertex.Location.ToPoint(), vertex.Bulge);
        }

        public static Vertex ToVertex(this DxfLwPolylineVertex vertex, double elevation)
        {
            return VertexFromPointAndBulge(new Point(vertex.X, vertex.Y, elevation), vertex.Bulge);
        }

        private static Vertex VertexFromPointAndBulge(Point point, double bulge)
        {
            if (bulge == 0.0)
            {
                // it's a line
                return new Vertex(point);
            }
            else
            {
                // it's an arc; according to the spec:
                //   The bulge is the tangent of one fourth the included angle for an arc segment, made negative
                //   if the arc goes clockwise from the start point to the end point.  A bulge of 0 indicates a
                //   straight segment, and a bulge of 1.0 is a semicircle.
                var includedAngle = Math.Atan(Math.Abs(bulge)) * 4.0 * MathHelper.RadiansToDegrees;
                var direction = bulge > 0.0 ? VertexDirection.CounterClockwise : VertexDirection.Clockwise;
                return new Vertex(point, includedAngle, direction);
            }
        }

        public static DxfVertex ToDxfVertex(this Vertex vertex)
        {
            var bulge = vertex.IsLine
                ? 0.0
                : Math.Tan(vertex.IncludedAngle * MathHelper.DegreesToRadians * 0.25) *
                    (vertex.Direction == VertexDirection.Clockwise ? -1.0 : 1.0);
            return new DxfVertex(vertex.Location.ToDxfPoint())
            {
                Bulge = bulge
            };
        }

        public static Line ToLine(this DxfLine line)
        {
            return new Line(line.P1.ToPoint(), line.P2.ToPoint(), line.GetEntityColor(), line, line.Thickness);
        }

        public static Polyline ToPolyline(this DxfLwPolyline poly)
        {
            var vertices = poly.Vertices.Select(v => v.ToVertex(poly.Elevation)).ToList();
            if (poly.IsClosed && vertices.Count > 0)
            {
                vertices.Add(vertices[0]);
            }

            return new Polyline(vertices, poly.GetEntityColor(), poly);
        }

        public static Polyline ToPolyline(this DxfPolyline poly)
        {
            var vertices = poly.Vertices.Select(v => v.ToVertex()).ToList();
            if (poly.IsClosed && vertices.Count > 0)
            {
                vertices.Add(vertices[0]);
            }

            return new Polyline(vertices, poly.GetEntityColor(), poly);
        }

        public static Polyline ToPolyline(this DxfLeader leader)
        {
            return new Polyline(leader.Vertices.Select(v => new Vertex(v.ToPoint())), leader.GetEntityColor(), leader);
        }

        public static Circle ToCircle(this DxfCircle circle)
        {
            return new Circle(circle.Center.ToPoint(), circle.Radius, circle.Normal.ToVector(), circle.GetEntityColor(), circle, circle.Thickness);
        }

        public static Arc ToArc(this DxfArc arc)
        {
            return new Arc(arc.Center.ToPoint(), arc.Radius, arc.StartAngle, arc.EndAngle, arc.Normal.ToVector(), arc.GetEntityColor(), arc, arc.Thickness);
        }

        public static Ellipse ToEllipse(this DxfEllipse el)
        {
            return new Ellipse(el.Center.ToPoint(), el.MajorAxis.ToVector(), el.MinorAxisRatio, el.StartParameter * MathHelper.RadiansToDegrees, el.EndParameter * MathHelper.RadiansToDegrees, el.Normal.ToVector(), el.GetEntityColor(), el);
        }
        
        public static Image ToImage(this DxfImage i)
        {
            // TODO: if path is not rooted, base off of drawing directory
            var data = File.ReadAllBytes(i.ImageDefinition.FilePath);
            var width = i.UVector.Length * i.ImageSize.X;
            var height = i.VVector.Length * i.ImageSize.Y;
            var rotation = Math.Atan2(i.UVector.Y, i.UVector.X) * MathHelper.RadiansToDegrees;
            var image = new Image(i.Location.ToPoint(), i.ImageDefinition.FilePath, data, width, height, rotation, i.GetEntityColor());
            return image;
        }

        public static Text ToText(this DxfText text)
        {
            return new Text(text.Value ?? string.Empty, text.Location.ToPoint(), text.Normal.ToVector(), text.TextHeight, text.Rotation, text.GetEntityColor(), text);
        }

        public static Spline ToSpline(this DxfSpline spline)
        {
            // only degree 3 curves are currently supported
            return spline.DegreeOfCurve == 3
                ? new Spline(spline.DegreeOfCurve, spline.ControlPoints.Select(p => p.Point.ToPoint()), spline.KnotValues, spline.GetEntityColor(), spline)
                : null;
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
                case DxfEntityType.Image:
                    entity = ((DxfImage)item).ToImage();
                    break;
                case DxfEntityType.Leader:
                    entity = ((DxfLeader)item).ToPolyline();
                    break;
                case DxfEntityType.Line:
                    entity = ((DxfLine)item).ToLine();
                    break;
                case DxfEntityType.LwPolyline:
                    entity = ((DxfLwPolyline)item).ToPolyline();
                    break;
                case DxfEntityType.Point:
                    entity = ((DxfModelPoint)item).ToPoint();
                    break;
                case DxfEntityType.Polyline:
                    entity = ((DxfPolyline)item).ToPolyline();
                    break;
                case DxfEntityType.Spline:
                    entity = ((DxfSpline)item).ToSpline();
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
                Layer = layer.Name
            };
        }

        public static DxfLine ToDxfLine(this Line line, Layer layer)
        {
            return new DxfLine(line.P1.ToDxfPoint(), line.P2.ToDxfPoint())
            {
                Layer = layer.Name,
                Thickness = line.Thickness
            };
        }

        public static DxfPolyline ToDxfPolyline(this Polyline poly, Layer layer)
        {
            var dp = new DxfPolyline(poly.Vertices.Select(v => v.ToDxfVertex()))
            {
                Elevation = poly.Vertices.Any() ? poly.Vertices.First().Location.Z : 0.0,
                Layer = layer.Name,
                Normal = DxfVector.ZAxis
            };

            return dp;
        }

        public static DxfCircle ToDxfCircle(this Circle circle, Layer layer)
        {
            return new DxfCircle(circle.Center.ToDxfPoint(), circle.Radius)
            {
                Normal = circle.Normal.ToDxfVector(),
                Layer = layer.Name,
                Thickness = circle.Thickness
            };
        }

        public static DxfArc ToDxfArc(this Arc arc, Layer layer)
        {
            return new DxfArc(arc.Center.ToDxfPoint(), arc.Radius, arc.StartAngle, arc.EndAngle)
            {
                Normal = arc.Normal.ToDxfVector(),
                Layer = layer.Name,
                Thickness = arc.Thickness
            };
        }

        public static DxfEllipse ToDxfEllipse(this Ellipse el, Layer layer)
        {
            return new DxfEllipse(el.Center.ToDxfPoint(), el.MajorAxis.ToDxfVector(), el.MinorAxisRatio)
            {
                StartParameter = el.StartAngle * MathHelper.DegreesToRadians,
                EndParameter = el.EndAngle * MathHelper.DegreesToRadians,
                Normal = el.Normal.ToDxfVector(),
                Layer = layer.Name
            };
        }

        public static DxfText ToDxfText(this Text text, Layer layer)
        {
            return new DxfText(text.Location.ToDxfPoint(), text.Height, text.Value)
            {
                Layer = layer.Name,
                Normal = text.Normal.ToDxfVector(),
                Rotation = text.Rotation
            };
        }

        public static DxfSpline ToDxfSpline(this Spline spline, Layer layer)
        {
            var dxfSpline = new DxfSpline()
            {
                DegreeOfCurve = 3,
                Layer = layer.Name
            };

            var beziers = spline.GetPrimitives().OfType<PrimitiveBezier>().ToArray();
            var knotDelta = 1.0 / beziers.Length;
            dxfSpline.KnotValues.Add(0.0);
            dxfSpline.KnotValues.Add(0.0);
            dxfSpline.KnotValues.Add(0.0);
            dxfSpline.KnotValues.Add(0.0);
            var currentKnot = knotDelta;
            foreach (var bezier in beziers)
            {
                dxfSpline.ControlPoints.Add(new DxfControlPoint(bezier.P1.ToDxfPoint()));
                dxfSpline.ControlPoints.Add(new DxfControlPoint(bezier.P2.ToDxfPoint()));
                dxfSpline.ControlPoints.Add(new DxfControlPoint(bezier.P3.ToDxfPoint()));
                dxfSpline.ControlPoints.Add(new DxfControlPoint(bezier.P4.ToDxfPoint()));
                dxfSpline.KnotValues.Add(currentKnot);
                dxfSpline.KnotValues.Add(currentKnot);
                dxfSpline.KnotValues.Add(currentKnot);
                dxfSpline.KnotValues.Add(currentKnot);
                currentKnot += knotDelta;
            }

            return dxfSpline;
        }

        public static DxfImage ToDxfImage(this Image image, Layer layer)
        {
            var (pixelWidth, pixelHeight) = Path.GetExtension(image.Path).ToLowerInvariant() switch
            {
                ".png" => (ToInt32BitEndian(image.ImageData, 16), ToInt32BitEndian(image.ImageData, 20)),
                // TODO: Add support for other image formats
                _ => throw new NotSupportedException(),
            };
            var dxfImage = new DxfImage(image.Path, image.Location.ToDxfPoint(), pixelWidth, pixelHeight, DxfVector.XAxis);
            dxfImage.Layer = layer.Name;
            var radians = image.Rotation * MathHelper.DegreesToRadians;
            var uVector = new DxfVector(Math.Cos(radians), Math.Sin(radians), 0.0);
            var vVector = new DxfVector(-Math.Sin(radians), Math.Cos(radians), 0.0);
            var uVectorLength = dxfImage.ImageSize.X / image.Width;
            var vVectorLength = dxfImage.ImageSize.Y / image.Height;
            uVector /= uVectorLength;
            vVector /= vVectorLength;
            dxfImage.UVector = uVector;
            dxfImage.VVector = vVector;
            return dxfImage;
        }

        private static int ToInt32BitEndian(byte[] array, int startIndex)
        {
            return array[startIndex] << 24 | array[startIndex + 1] << 16 | array[startIndex + 2] << 8 | array[startIndex + 3];
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
                case EntityKind.Spline:
                    entity = ((Spline)item).ToDxfSpline(layer);
                    break;
                case EntityKind.Image:
                    entity = ((Image)item).ToDxfImage(layer);
                    break;
                default:
                    Debug.Assert(false, "Unsupported entity type: " + item.GetType().Name);
                    break;
            }

            if (entity != null)
            {
                entity.AssignDxfEntityColor(item.Color);
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
