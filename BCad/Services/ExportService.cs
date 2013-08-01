using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media.Media3D;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;

namespace BCad.Services
{
    [Export(typeof(IExportService))]
    internal class ExportService : IExportService
    {
        public IEnumerable<ProjectedEntity> ProjectTo2D(Drawing drawing, ViewPort viewPort)
        {
            // create transform
            var normal = viewPort.Sight * -1.0;
            var up = viewPort.Up;
            var right = up.Cross(normal).Normalize();
            var transform = TranslationMatrix(-viewPort.BottomLeft.X, -viewPort.BottomLeft.Y - viewPort.ViewHeight, 0)
                * PrimitiveExtensions.FromUnitCircleProjection(
                    normal,
                    right,
                    up,
                    Point.Origin,
                    -1.0,
                    -1.0,
                    1.0);
            transform.Scale(new Vector3D(1.0, 1.0, 0.0));

            // project all entities
            var entities = new List<ProjectedEntity>();
            foreach (var layer in from l in drawing.GetLayers()
                                  where l.EntityCount > 0
                                  orderby l.Name
                                  select l)
            {
                foreach (var entity in layer.GetEntities().OrderBy(x => x.Id))
                {
                    var projected = Project(entity, layer, transform);
                    if (projected != null)
                        entities.Add(projected);
                }
            }

            return entities;
        }

        private ProjectedEntity Project(Entity entity, Layer layer, Matrix3D transform)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    return Project((Line)entity, layer, transform);
                case EntityKind.Text:
                    return Project((Text)entity, layer, transform);
                case EntityKind.Circle:
                    return Project((Circle)entity, layer, transform);
                case EntityKind.Arc:
                    return Project((Arc)entity, layer, transform);
                case EntityKind.Aggregate:
                    return Project((AggregateEntity)entity, layer, transform);
                default:
                    return null;
            }
        }

        private ProjectedLine Project(Line line, Layer layer, Matrix3D transform)
        {
            var p1 = transform.Transform(line.P1);
            var p2 = transform.Transform(line.P2);
            return new ProjectedLine(line, layer, p1, p2);
        }

        private ProjectedText Project(Text text, Layer layer, Matrix3D transform)
        {
            var loc = transform.Transform(text.Location);
            var rad = text.Rotation * MathHelper.DegreesToRadians;
            var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize();
            var up = text.Normal.Cross(right).Normalize();
            var top = transform.Transform((text.Location + up * text.Height));
            var height = (top - loc).Length;
            var rotation = new Vector((top - loc)).ToAngle() * -1.0 - 90.0;
            return new ProjectedText(text, layer, loc, height, rotation.CorrectAngleDegrees());
        }

        private ProjectedCircle Project(Circle circle, Layer layer, Matrix3D transform)
        {
            // find axis endpoints
            var rightVector = Vector.RightVectorFromNormal(circle.Normal);
            var upVector = circle.Normal.Cross(rightVector).Normalize();
            var pt = transform.Transform((circle.Center + (rightVector * circle.Radius)));
            var qt = transform.Transform((circle.Center + (upVector * circle.Radius)));
            var m = transform.Transform(circle.Center);
            return ProjectedCircle.FromConjugateDiameters(circle, layer, m, pt, qt);
        }

        private ProjectedArc Project(Arc arc, Layer layer, Matrix3D transform)
        {
            // find the containing circle
            var rightVector = Vector.RightVectorFromNormal(arc.Normal);
            var upVector = arc.Normal.Cross(rightVector).Normalize();
            var pt = transform.Transform((arc.Center + (rightVector * arc.Radius)));
            var qt = transform.Transform((arc.Center + (upVector * arc.Radius)));
            var m = transform.Transform(arc.Center);
            var circle = ProjectedCircle.FromConjugateDiameters(null, layer, m, pt, qt);

            // find the new start and end angles
            var startPoint = (Point)transform.Transform(arc.EndPoint1);
            var endPoint = (Point)transform.Transform(arc.EndPoint2);
            var startAngle = (startPoint - circle.Center).ToAngle();
            var endAngle = (endPoint - circle.Center).ToAngle();

            return new ProjectedArc(arc, layer, circle.Center, circle.RadiusX, circle.RadiusY, circle.Rotation, startAngle, endAngle, startPoint, endPoint);
        }

        private ProjectedAggregate Project(AggregateEntity aggregate, Layer layer, Matrix3D transform)
        {
            var loc = transform.Transform(aggregate.Location);
            return new ProjectedAggregate(aggregate, layer, loc, aggregate.Children.Select(c => Project(c, layer, transform)));
        }

        private static Matrix3D TranslationMatrix(double dx, double dy, double dz)
        {
            var matrix = Matrix3D.Identity;
            matrix.OffsetX = dx;
            matrix.OffsetY = dy;
            matrix.OffsetZ = dz;
            return matrix;
        }
    }
}
