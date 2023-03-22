using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;

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

        public static LineTypeSpecification GetLineTypeSpecification(this DxfEntity dxfEntity)
        {
            return new LineTypeSpecification(dxfEntity.LineTypeName, dxfEntity.LineTypeScale);
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
            return new Location(point.Location.ToPoint(), point.Color.ToColor(), point.GetLineTypeSpecification(), point);
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
            return new Line(line.P1.ToPoint(), line.P2.ToPoint(), line.GetEntityColor(), line.GetLineTypeSpecification(), line, line.Thickness);
        }

        public static Polyline ToPolyline(this DxfLwPolyline poly)
        {
            var vertices = poly.Vertices.Select(v => v.ToVertex(poly.Elevation)).ToList();
            if (poly.IsClosed && vertices.Count > 0)
            {
                vertices.Add(vertices[0]);
            }

            return new Polyline(vertices, poly.GetEntityColor(), poly.GetLineTypeSpecification(), poly);
        }

        public static Polyline ToPolyline(this DxfPolyline poly)
        {
            var vertices = poly.Vertices.Select(v => v.ToVertex()).ToList();
            if (poly.IsClosed && vertices.Count > 0)
            {
                vertices.Add(vertices[0]);
            }

            return new Polyline(vertices, poly.GetEntityColor(), poly.GetLineTypeSpecification(), poly);
        }

        public static Polyline ToPolyline(this DxfLeader leader)
        {
            return new Polyline(leader.Vertices.Select(v => new Vertex(v.ToPoint())), leader.GetEntityColor(), leader.GetLineTypeSpecification(), leader);
        }

        public static Circle ToCircle(this DxfCircle circle)
        {
            return new Circle(circle.Center.ToPoint(), circle.Radius, circle.Normal.ToVector(), circle.GetEntityColor(), circle.GetLineTypeSpecification(), circle, circle.Thickness);
        }

        public static LinearDimension ToDimension(this DxfDimensionBase dim)
        {
            Point firstPoint, secondPoint, selectedDimensionLineLocation, textMidPoint;
            bool isAligned;
            switch (dim.DimensionType)
            {
                case DxfDimensionType.Aligned:
                    var aligned = (DxfAlignedDimension)dim;
                    firstPoint = aligned.DefinitionPoint2.ToPoint();
                    secondPoint = aligned.DefinitionPoint3.ToPoint();
                    selectedDimensionLineLocation = aligned.DefinitionPoint1.ToPoint();
                    textMidPoint = aligned.TextMidPoint.ToPoint();
                    isAligned = true;
                    break;
                case DxfDimensionType.RotatedHorizontalOrVertical:
                    var rotated = (DxfRotatedDimension)dim;
                    firstPoint = rotated.DefinitionPoint2.ToPoint();
                    secondPoint = rotated.DefinitionPoint3.ToPoint();
                    selectedDimensionLineLocation = rotated.DefinitionPoint1.ToPoint();
                    textMidPoint = rotated.TextMidPoint.ToPoint();
                    isAligned = false;
                    break;
                default:
                    return null;
            }

            var textOverride = dim.Text is null || dim.Text == "<>"
                ? null
                : dim.Text == " "
                    ? string.Empty
                    : dim.Text;
            var dimension = new LinearDimension(
                firstPoint,
                secondPoint,
                selectedDimensionLineLocation,
                isAligned,
                textMidPoint,
                dim.DimensionStyleName,
                textOverride,
                null,
                dim.GetEntityColor(),
                dim.GetLineTypeSpecification(),
                dim);

            return dimension;
        }

        public static Arc ToArc(this DxfArc arc)
        {
            return new Arc(arc.Center.ToPoint(), arc.Radius, arc.StartAngle, arc.EndAngle, arc.Normal.ToVector(), arc.GetEntityColor(), arc.GetLineTypeSpecification(), arc, arc.Thickness);
        }

        public static Ellipse ToEllipse(this DxfEllipse el)
        {
            return new Ellipse(el.Center.ToPoint(), el.MajorAxis.ToVector(), el.MinorAxisRatio, el.StartParameter * MathHelper.RadiansToDegrees, el.EndParameter * MathHelper.RadiansToDegrees, el.Normal.ToVector(), el.GetEntityColor(), el.GetLineTypeSpecification(), el);
        }

        public static async Task<Image> ToImage(this DxfImage i, Func<string, Task<byte[]>> contentResolver)
        {
            var data = await contentResolver(i.ImageDefinition.FilePath);
            var width = i.UVector.Length * i.ImageSize.X;
            var height = i.VVector.Length * i.ImageSize.Y;
            var rotation = Math.Atan2(i.UVector.Y, i.UVector.X) * MathHelper.RadiansToDegrees;
            var image = new Image(i.Location.ToPoint(), i.ImageDefinition.FilePath, data, width, height, rotation, i.GetEntityColor());
            return image;
        }

        public static async Task<AggregateEntity> ToAggregate(this DxfInsert i, Func<string, Task<byte[]>> contentResolver, Dictionary<string, IEnumerable<DxfEntity>> blockMap)
        {
            var children = await Task.WhenAll(blockMap[i.Name].Select(e => e.ToEntity(contentResolver, blockMap)));
            var childrenList = ReadOnlyList<Entity>.Create(children);
            var aggregate = new AggregateEntity(i.Location.ToPoint(), childrenList);
            return aggregate;
        }

        public static Text ToText(this DxfText text)
        {
            return new Text(text.Value ?? string.Empty, text.Location.ToPoint(), text.Normal.ToVector(), text.TextHeight, text.Rotation, text.GetEntityColor(), text.GetLineTypeSpecification(), text);
        }

        public static Spline ToSpline(this DxfSpline spline)
        {
            // only degree 3 curves are currently supported
            return spline.DegreeOfCurve == 3
                ? new Spline(spline.DegreeOfCurve, spline.ControlPoints.Select(p => p.Point.ToPoint()), spline.KnotValues, spline.GetEntityColor(), spline.GetLineTypeSpecification(), spline)
                : null;
        }

        public static async Task<Entity> ToEntity(this DxfEntity item, Func<string, Task<byte[]>> contentResolver, Dictionary<string, IEnumerable<DxfEntity>> blockMap)
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
                case DxfEntityType.Dimension:
                    entity = ((DxfDimensionBase)item).ToDimension();
                    break;
                case DxfEntityType.Ellipse:
                    entity = ((DxfEllipse)item).ToEllipse();
                    break;
                case DxfEntityType.Image:
                    entity = await ((DxfImage)item).ToImage(contentResolver);
                    break;
                case DxfEntityType.Insert:
                    entity = await ((DxfInsert)item).ToAggregate(contentResolver, blockMap);
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

        public static DxfModelPoint ToDxfLocation(this Location location)
        {
            return new DxfModelPoint(location.Point.ToDxfPoint());
        }

        public static DxfLine ToDxfLine(this Line line)
        {
            return new DxfLine(line.P1.ToDxfPoint(), line.P2.ToDxfPoint())
            {
                Thickness = line.Thickness
            };
        }

        public static DxfDimensionBase ToDxfDimension(this LinearDimension linearDimension)
        {
            var textValue = linearDimension.TextOverride is null
                ? "<>"
                : string.IsNullOrWhiteSpace(linearDimension.TextOverride)
                    ? " "
                    : linearDimension.TextOverride;
            DxfDimensionBase dimension = linearDimension.IsAligned
                ? new DxfAlignedDimension()
                    {
                        DefinitionPoint2 = linearDimension.DefinitionPoint1.ToDxfPoint(),
                        DefinitionPoint3 = linearDimension.DefinitionPoint2.ToDxfPoint(),
                        DefinitionPoint1 = linearDimension.DimensionLineLocation.ToDxfPoint(),
                    }
                : new DxfRotatedDimension()
                    {
                        DefinitionPoint2 = linearDimension.DefinitionPoint1.ToDxfPoint(),
                        DefinitionPoint3 = linearDimension.DefinitionPoint2.ToDxfPoint(),
                        DefinitionPoint1 = linearDimension.DimensionLineLocation.ToDxfPoint(),
                    };
            dimension.DimensionStyleName = linearDimension.DimensionStyleName;
            dimension.TextMidPoint = linearDimension.TextMidPoint.ToDxfPoint();
            dimension.Text = textValue;
            return dimension;
        }

        public static DxfPolyline ToDxfPolyline(this Polyline poly)
        {
            var dp = new DxfPolyline(poly.Vertices.Select(v => v.ToDxfVertex()))
            {
                Elevation = poly.Vertices.Any() ? poly.Vertices.First().Location.Z : 0.0,
                Normal = DxfVector.ZAxis
            };

            return dp;
        }

        public static DxfCircle ToDxfCircle(this Circle circle)
        {
            return new DxfCircle(circle.Center.ToDxfPoint(), circle.Radius)
            {
                Normal = circle.Normal.ToDxfVector(),
                Thickness = circle.Thickness
            };
        }

        public static DxfArc ToDxfArc(this Arc arc)
        {
            return new DxfArc(arc.Center.ToDxfPoint(), arc.Radius, arc.StartAngle, arc.EndAngle)
            {
                Normal = arc.Normal.ToDxfVector(),
                Thickness = arc.Thickness
            };
        }

        public static DxfEllipse ToDxfEllipse(this Ellipse el)
        {
            return new DxfEllipse(el.Center.ToDxfPoint(), el.MajorAxis.ToDxfVector(), el.MinorAxisRatio)
            {
                StartParameter = el.StartAngle * MathHelper.DegreesToRadians,
                EndParameter = el.EndAngle * MathHelper.DegreesToRadians,
                Normal = el.Normal.ToDxfVector(),
            };
        }

        public static DxfText ToDxfText(this Text text)
        {
            return new DxfText(text.Location.ToDxfPoint(), text.Height, text.Value)
            {
                Normal = text.Normal.ToDxfVector(),
                Rotation = text.Rotation
            };
        }

        public static DxfSpline ToDxfSpline(this Spline spline, DrawingSettings settings)
        {
            var dxfSpline = new DxfSpline()
            {
                DegreeOfCurve = 3,
            };

            var beziers = spline.GetPrimitives(settings).OfType<PrimitiveBezier>().ToArray();
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

        public static DxfImage ToDxfImage(this Image image)
        {
            var (pixelWidth, pixelHeight) = ImageHelpers.GetImageDimensions(image.Path, image.ImageData);
            var dxfImage = new DxfImage(image.Path, image.Location.ToDxfPoint(), pixelWidth, pixelHeight, DxfVector.XAxis);
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

        public static DxfEntity ToDxfEntity(this Entity item, DrawingSettings settings)
        {
            var entity = item.MapEntity<DxfEntity>(
                aggregate => null, // no-op, handled elsewhere
                arc => arc.StartAngle == 0.0 && arc.EndAngle == 360.0
                    ? new Circle(arc.Center, arc.Radius, arc.Normal, arc.Color).ToDxfCircle()
                    : arc.ToDxfArc(),
                circle => circle.ToDxfCircle(),
                ellipse => ellipse.ToDxfEllipse(),
                image => image.ToDxfImage(),
                line => line.ToDxfLine(),
                linearDimension => linearDimension.ToDxfDimension(),
                location => location.ToDxfLocation(),
                polyline => polyline.ToDxfPolyline(),
                spline => spline.ToDxfSpline(settings),
                text => text.ToDxfText()
            );

            if (entity != null)
            {
                entity.AssignDxfEntityColor(item.Color);
            }

            return entity;
        }

        public static DxfDrawingUnits ToDxfDrawingUnits(this DrawingUnits units)
        {
            switch (units)
            {
                case DrawingUnits.English:
                    return DxfDrawingUnits.English;
                case DrawingUnits.Metric:
                    return DxfDrawingUnits.Metric;
                default:
                    throw new ArgumentException(nameof(units), "Unsupported unit format");
            }
        }

        public static DxfUnitFormat ToDxfUnitFormat(this UnitFormat format)
        {
            switch (format)
            {
                case UnitFormat.Architectural:
                    return DxfUnitFormat.Architectural;
                case UnitFormat.Fractional:
                    return DxfUnitFormat.Fractional;
                case UnitFormat.Decimal:
                    return DxfUnitFormat.Decimal;
                default:
                    throw new ArgumentException(nameof(format), "Unsupported unit format");
            }
        }

        public static DrawingUnits ToDrawingUnits(this DxfDrawingUnits units)
        {
            switch (units)
            {
                case DxfDrawingUnits.English:
                    return DrawingUnits.English;
                case DxfDrawingUnits.Metric:
                    return DrawingUnits.Metric;
                default:
                    throw new ArgumentException(nameof(units), "Unsupported unit format");
            }
        }

        public static UnitFormat ToUnitFormat(this DxfUnitFormat format)
        {
            switch (format)
            {
                case DxfUnitFormat.Architectural:
                case DxfUnitFormat.ArchitecturalStacked:
                    return UnitFormat.Architectural;
                case DxfUnitFormat.Fractional:
                case DxfUnitFormat.FractionalStacked:
                    return UnitFormat.Fractional;
                case DxfUnitFormat.Decimal:
                case DxfUnitFormat.Engineering:
                case DxfUnitFormat.Scientific:
                    return UnitFormat.Decimal;
                default:
                    throw new ArgumentException(nameof(format), "Unsupported unit format");
            }
        }
    }
}
